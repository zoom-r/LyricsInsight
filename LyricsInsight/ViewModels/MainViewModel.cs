using System;
using LyricsInsight.Core.Services;
using System.Reactive.Linq;
using LyricsInsight.Core;
using LyricsInsight.Core.Models;
using ReactiveUI;

namespace LyricsInsight.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentView;
        public ViewModelBase CurrentView
        {
            get => _currentView;
            set => this.RaiseAndSetIfChanged(ref _currentView, value);
        }
        
        private readonly SettingsService _settingsService;
        private DeezerService _deezerService;
        private LyricsService _lyricsService;
        private GenAiService _genAiService;
        private SearchViewModel _searchVm;
        public MainViewModel()
        {
            _settingsService = new SettingsService();
            InitializeApplication();
        }
        
        private async void InitializeApplication()
        {
            var savedKey = await _settingsService.LoadKeyAsync();

            if (string.IsNullOrWhiteSpace(savedKey))
                CurrentView = new ApiKeyViewModel(_settingsService, InitializeAppServices);
            else
                InitializeAppServices(savedKey);
        }
        
        private void InitializeAppServices(string apiKey)
        {
            try
            {
                _deezerService = new DeezerService();
                _lyricsService = new LyricsService();
                _genAiService = new GenAiService(apiKey);
            }
            catch (Exception ex)
            {
                CurrentView = new ApiKeyViewModel(_settingsService, InitializeAppServices);
                return;
            }
            
            _searchVm = new SearchViewModel(_deezerService);

            _searchVm.OnSongSelected
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe((SongSearchResult song) => NavigateToSongDetails(song)); 

            CurrentView = _searchVm;
        }
        
        private void NavigateToSongDetails(SongSearchResult song)
        {
            CurrentView = new SongDetailsViewModel(song,  _lyricsService, _genAiService, _deezerService, NavigateToSearch);
        }
        
        private void NavigateToSearch()
        {
            _searchVm.SelectedSong = null;
            CurrentView = _searchVm;
        }
    }
}