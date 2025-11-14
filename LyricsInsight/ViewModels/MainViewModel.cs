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
        
        private readonly SettingsService _settingsService;
        private DeezerService _deezerService;
        private LyricsService _lyricsService;
        private GenAiService _genAiService;
        private SearchViewModel _searchVM;
        public MainViewModel()
        {
            _settingsService = new SettingsService();
            InitializeApplication();
        }
        
        private async void InitializeApplication()
        {
            // Опитай да заредиш ключа от файла
            var savedKey = await _settingsService.LoadKeyAsync();

            if (string.IsNullOrWhiteSpace(savedKey))
            {
                // НЯМА КЛЮЧ: Показваме екрана за въвеждане
                // Подаваме му "callback" към метода InitializeAppServices
                CurrentView = new ApiKeyViewModel(_settingsService, InitializeAppServices);
            }
            else
            {
                // ИМА КЛЮЧ: Директно инициализираме приложението
                InitializeAppServices(savedKey);
            }
        }
        
        private void InitializeAppServices(string apiKey)
        {
            // Вече имаме ключ! Инициализираме всички сервизи
            try
            {
                _deezerService = new DeezerService();
                _lyricsService = new LyricsService();
                _genAiService = new GenAiService(apiKey); // Подаваме ключа тук!
            }
            catch (Exception ex)
            {
                // Ако ключът е грешен, GenAiService ще гръмне.
                // Връщаме потребителя обратно да въведе нов ключ.
                CurrentView = new ApiKeyViewModel(_settingsService, InitializeAppServices);
                // (Тук можем да добавим съобщение за грешка)
                return;
            }
            
            // Създаваме SearchVM (вече можем)
            _searchVM = new SearchViewModel(_deezerService);

            // Абонираме се за навигацията
            _searchVM.OnSongSelected
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe((DeezerTrack song) => NavigateToSongDetails(song)); 

            // Показваме търсачката
            CurrentView = _searchVM;
        }
        
        // --- ДОБАВИ ТОЗИ НОВ МЕТОД ---
        private void NavigateToSongDetails(DeezerTrack song)
        {
            // Тук е магията:
            // 1. Създаваме новия ViewModel
            // 2. Подаваме му избраната песен
            // 3. Задаваме CurrentView, което автоматично сменя UI-я
            CurrentView = new SongDetailsViewModel(song,  _lyricsService, _genAiService, NavigateToSearch);
        }
        
        // 8. Нов метод, който просто връща стария изглед
        private void NavigateToSearch()
        {
            _searchVM.SelectedSong = null;
            CurrentView = _searchVM;
        }
    }
}