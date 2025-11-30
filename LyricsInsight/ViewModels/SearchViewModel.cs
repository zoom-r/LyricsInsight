using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using LyricsInsight.Core.Models;
using LyricsInsight.Core.Services;
using ReactiveUI;

namespace LyricsInsight.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
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

        public IObservable<SongSearchResult> OnSongSelected { get; }
        
        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
        }

        public ObservableCollection<SongSearchResult> SearchResults { get; } = new();
        
        public SearchViewModel(DeezerService service)
        {
            _service = service; 
            IsLoadingResults = false;
    
            OnSongSelected = this.WhenAnyValue(vm => vm.SelectedSong)
                .Where(song => song != null);

            var whenQueryChanged = this.WhenAnyValue(vm => vm.SearchQuery)
                .Throttle(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler)
                .Select(query => query?.Trim())
                .DistinctUntilChanged();
            
            whenQueryChanged
                .Where(query => !string.IsNullOrWhiteSpace(query) && query.Length > 2)
                .Do(_ => IsLoadingResults = true)
                .SelectMany(query => 
                        Observable.FromAsync(() => _service.SearchSongsAsync(query))
                            .Catch(Observable.Return(new List<SongSearchResult>()))
                )
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(results =>
                {
                    SearchResults.Clear();
                    foreach (var result in results)
                    {
                        SearchResults.Add(result);
                    }
                    IsLoadingResults = false;
                });
            
            whenQueryChanged
                .Where(query => string.IsNullOrWhiteSpace(query) || query.Length <= 2)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    SearchResults.Clear();
                    IsLoadingResults = false;
                });
        }
    }
}