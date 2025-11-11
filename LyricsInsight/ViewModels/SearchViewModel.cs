using System; // За TimeSpan
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
            _service = service; // Запазваме го
            OnSongSelected = this.WhenAnyValue(vm => vm.SelectedSong)
                .Where(song => song != null); // Само когато не е null
            // --- ТОВА Е РЕАКТИВНАТА МАГИЯ ---
            
            // "WhenAnyValue" казва: "Винаги, когато пропъртито 'SearchQuery' се промени..."
            this.WhenAnyValue(vm => vm.SearchQuery)
                // "...изчакай 500 милисекунди, след като потребителят спре да пише."
                // (Това спира спама към API-то при всяка буква)
                .Throttle(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler)
                // "Вземи стойността и я почисти (trim)"
                .Select(query => query?.Trim())
                // "Не прави нищо, ако новата стойност е същата като старата"
                .DistinctUntilChanged()
                // "Ако стойността не е празна..."
                .Where(query => !string.IsNullOrWhiteSpace(query))
                // "...извикай АСИНХРОННО нашия API сервиз."
                // (SelectMany е за работа с async задачи в "поток")
                .SelectMany(query => _service.SearchSongsAsync(query))
                // "Когато резултатите пристигнат, върни се на UI нишката."
                .ObserveOn(RxApp.MainThreadScheduler)
                // "...и се 'абонирай' за резултата."
                .Subscribe(results =>
                {
                    // Това е кодът, който се изпълнява,
                    // когато API-то върне данни.
                    
                    SearchResults.Clear(); // Изчисти старите резултати
                    foreach (var result in results)
                    {
                        SearchResults.Add(result); // Добави новите
                    }
                });
        }
    }
}