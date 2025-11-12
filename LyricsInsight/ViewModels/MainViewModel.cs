using System;
using LyricsInsight.Core.Services;
using System.Reactive.Linq;
using LyricsInsight.Core; // <-- ДОБАВИ ТОЗИ USING
using LyricsInsight.Core.Models;
using ReactiveUI; // За ViewModelBase и RaiseAndSetIfChanged

namespace LyricsInsight.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        // Това пропърти ще "държи" текущия изглед (ViewModel), който искаме да покажем.
        private ViewModelBase _currentView;
        public ViewModelBase CurrentView
        {
            get => _currentView;
            set => this.RaiseAndSetIfChanged(ref _currentView, value);
        }
        
        private readonly DeezerService _deezerService;
        private readonly LyricsService _lyricsService;
        private readonly GenAiService _genAiService;
        private readonly SearchViewModel _searchVM;
        public MainViewModel()
        {
            _deezerService = new DeezerService();
            _lyricsService = new LyricsService();
            _genAiService = new GenAiService();
            // Създаваме SearchViewModel
            _searchVM = new SearchViewModel(_deezerService);

            // "Абонираме" се за неговия сигнал OnSongSelected
            _searchVM.OnSongSelected
                .ObserveOn(RxApp.MainThreadScheduler) // Увери се, че сме на UI нишката
                .Subscribe((SongSearchResult song) => NavigateToSongDetails(song)); // Когато получим сигнал, викай този метод
            // При стартиране, задаваме текущия изглед да бъде SearchViewModel.
            CurrentView = _searchVM;
        }
        
        // --- ДОБАВИ ТОЗИ НОВ МЕТОД ---
        private void NavigateToSongDetails(SongSearchResult song)
        {
            // Тук е магията:
            // 1. Създаваме новия ViewModel
            // 2. Подаваме му избраната песен
            // 3. Задаваме CurrentView, което автоматично сменя UI-я
            CurrentView = new SongDetailsViewModel(song,  _lyricsService, _genAiService,  _deezerService, NavigateToSearch);
        }
        
        // 8. Нов метод, който просто връща стария изглед
        private void NavigateToSearch()
        {
            _searchVM.SelectedSong = null;
            CurrentView = _searchVM;
        }
    }
}