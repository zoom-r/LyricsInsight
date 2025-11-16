using System;
using System.Collections.Generic; // За TimeSpan
using System.Collections.ObjectModel;
using System.Linq; // За .Select()
using System.Reactive.Linq; // КЛЮЧОВО! Това е за .Throttle, .SelectMany и др.
using LyricsInsight.Core.Models;
using LyricsInsight.Core.Services; // Нашият сервиз
using ReactiveUI; // За ViewModelBase и RxApp

namespace LyricsInsight.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        // Поле, в което да пазим "инжекторания" сервиз
        private readonly DeezerService _service;
        private SongSearchResult _selectedSong;
        public SongSearchResult SelectedSong
        {
            get => _selectedSong;
            set => this.RaiseAndSetIfChanged(ref _selectedSong, value);
        }
        private bool _isLoadingResults;

        public bool IsLoadingResults
        {
            get => _isLoadingResults;
            set => this.RaiseAndSetIfChanged(ref _isLoadingResults, value);
        }

        // --- 2. ДОБАВИ ТОВА ПРОПЪРТИ ---
        // То казва на света: "Хей, потребителят избра песен!"
        public IObservable<SongSearchResult> OnSongSelected { get; }
        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
        }

        public ObservableCollection<SongSearchResult> SearchResults { get; } = new();

        // Конструкторът вече ПРИЕМА DeezerApiService
        public SearchViewModel(DeezerService service)
        {
            _service = service; 
            IsLoadingResults = false; // Начална стойност
    
            OnSongSelected = this.WhenAnyValue(vm => vm.SelectedSong)
                .Where(song => song != null);

            // --- ОБЕДИНЯВАМЕ ВСИЧКО В ЕДИН ПОТОК ---

            // "when" ще "слуша" за промени в SearchQuery
            var whenQueryChanged = this.WhenAnyValue(vm => vm.SearchQuery)
                .Throttle(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler)
                .Select(query => query?.Trim())
                .DistinctUntilChanged();

            // --- ПОТОК 1: За ВАЛИДНИ търсения ---
            whenQueryChanged
                // 1. ОПРАВЕН WHERE: Търси само ако текстът е > 2 символа
                .Where(query => !string.IsNullOrWhiteSpace(query) && query.Length > 2)
                .Do(_ => IsLoadingResults = true) // Показва спинъра (на UI нишка)
                // 2. ПО-СИГУРЕН ASYNC:
                //    Изрично казваме "пусни това във фон, дори ако
                //    SelectMany се обърка"
                .SelectMany(query => 
                        Observable.FromAsync(() => _service.SearchSongsAsync(query))
                            .Catch(Observable.Return(new List<SongSearchResult>())) // Хвани грешки
                )
                .ObserveOn(RxApp.MainThreadScheduler) // Върни се на UI нишка
                .Subscribe(results =>
                {
                    SearchResults.Clear();
                    foreach (var result in results)
                    {
                        SearchResults.Add(result);
                    }
                    IsLoadingResults = false; // Скрий спинъра
                });

            // --- ПОТОК 2: За НЕВАЛИДНИ/празни търсения ---
            whenQueryChanged
                // 3. ОПРАВЕН WHERE: Изпълни само ако текстът е празен или твърде къс
                .Where(query => string.IsNullOrWhiteSpace(query) || query.Length <= 2)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    SearchResults.Clear(); // Изчисти резултатите
                    IsLoadingResults = false; // Увери се, че спинърът е скрит
                });
        }
    }
}