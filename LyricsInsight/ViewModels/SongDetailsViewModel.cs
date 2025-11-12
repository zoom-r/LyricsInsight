using System;
using LyricsInsight.Core;
using LyricsInsight.Core.Models; // Трябва ни SongSearchResult
using LyricsInsight.Core.Services;
using ReactiveUI; // За ViewModelBase
using System.Windows.Input;
using System.Threading.Tasks;
using Avalonia.Threading;


namespace LyricsInsight.ViewModels;
public class SongDetailsViewModel : ViewModelBase
{
    // Пропъртита за всичко, което ще показваме в UI-я
    private readonly LyricsService _lyricsService;
    private readonly GenAiService _genAiService;
    private readonly DeezerService _deezerService;
    
    public ICommand BackCommand { get; }
    
    private string _title;
    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    private string _artist;
    public string Artist
    {
        get => _artist;
        set => this.RaiseAndSetIfChanged(ref _artist, value);
    }

    private string _albumCoverUrl;
    public string AlbumCoverUrl
    {
        get => _albumCoverUrl;
        set => this.RaiseAndSetIfChanged(ref _albumCoverUrl, value);
    }
    
    // ...
    public string AlbumCoverSmallUrl { get; private set; }
    public string AlbumCoverMediumUrl { get; private set; }
    public string AlbumCoverBigUrl { get; private set; }
    
    private string _lyricsText;
    public string LyricsText
    {
        get => _lyricsText;
        set => this.RaiseAndSetIfChanged(ref _lyricsText, value);
    }

    private string _aiAnalysisText;
    public string AiAnalysisText
    {
        get => _aiAnalysisText;
        set => this.RaiseAndSetIfChanged(ref _aiAnalysisText, value);
    }
    
    private string _releaseDate;
    public string ReleaseDate { get => _releaseDate; set => this.RaiseAndSetIfChanged(ref _releaseDate, value); }
    
    private string _album;
    public string Album
    {
        get => _album;
        set => this.RaiseAndSetIfChanged(ref _album, value);
    }

    private bool _isAnalysisReady;
    public bool IsAnalysisReady
    {
        get => _isAnalysisReady;
        set => this.RaiseAndSetIfChanged(ref _isAnalysisReady, value);
    }
    
    // Конструктор, който приема избраната песен
    public SongDetailsViewModel(SongSearchResult selectedSong,  LyricsService lyricsService, GenAiService genAiService, DeezerService deezerService, Action onGoBack)
    {
        _lyricsService = lyricsService;
        _genAiService = genAiService;
        _deezerService = deezerService;
        BackCommand = ReactiveCommand.Create(onGoBack, outputScheduler: RxApp.MainThreadScheduler);
        // Попълваме данните, които вече имаме от търсенето
        Title = selectedSong.Title;
        Artist = selectedSong.Artist;
        AlbumCoverUrl = selectedSong.AlbumCoverUrl; // Ще вземем по-голяма снимка по-късно
        Album = $"Албум: {selectedSong.Album}";
        IsAnalysisReady = false;
        // --- ВРЕМЕННО: ФАЛШИВИ ДАННИ ---
        // Ще заредим истинските данни от Genius/OpenAI в следващите стъпки.
        // Засега слагаме фалшив текст, за да тестваме UI-я.
        // Задаваме "Зареждане..." съобщения
        LyricsText = "Зареждане на текста...";
        AiAnalysisText = "Очаква се текстът, за да започне анализ...";
        LoadSongDetails(selectedSong.Id);
        LoadLyrics();
    }
    
    private void LoadLyrics()
    {
        Task.Run(async () =>
        {
            try
            {
                // Това се случва на фонова нишка (OK)
                var lyricsResult = await _lyricsService.GetLyricsAsync(Artist, Title);

                // Това е UI ъпдейт -> трябва да е в Dispatcher!
            
                if (lyricsResult == null || string.IsNullOrWhiteSpace(lyricsResult.Text))
                {
                    LyricsText = "За съжаление текстът на песента не беше намерен.";
                    AiAnalysisText = "Няма текст, върху който да се извърши анализ.";
                }
                else
                {
                    string footer = $"\n\n\n(Източник: {lyricsResult.Source})";
                    LyricsText = lyricsResult.Text + footer;
                    // Безопасно е да извикаш LoadAnalysis оттук,
                    // защото сме на UI нишката.
                    LoadAnalysis(lyricsResult.Text); 
                }
            
            }
            catch (Exception ex)
            {
                LyricsText = $"Възникна грешка при зареждането на текста: {ex.Message}";
                AiAnalysisText = "Анализът не може да продължи.";
            }
        });
    }
    
    private void LoadAnalysis(string lyricsToAnalyze)
    {
        Task.Run(async () =>
        {
            try
            {
                // Този ъпдейт е OK, защото е ПРЕДИ 'await'-а
                AiAnalysisText = "Генериране на AI анализ... (това може да отнеме няколко секунди)";
        
                // Това се случва на фонова нишка (OK)
                var (analysis, success) = await _genAiService.GenerateAnalysisAsync(lyricsToAnalyze, Title, Artist, Album);
        
                // Това е UI ъпдейт СЛЕД 'await' -> трябва да е в Dispatcher!
                AiAnalysisText = analysis;
                if(success) IsAnalysisReady = true;
            
            }
            catch (Exception ex)
            {

                AiAnalysisText = $"Грешка при генерирането на анализ: {ex.Message}";
                IsAnalysisReady = false;
            }
        });
    }

    private void LoadSongDetails(string trackId)
    {
        Task.Run(async () =>
        {
            try
            {
                var details = await _deezerService.GetTrackDetailsAsync(trackId);
                if (details != null)
                {
                    ReleaseDate = $"Издадена: {details.ReleaseDate}";
                    AlbumCoverSmallUrl = details.Album?.CoverSmall;
                    AlbumCoverMediumUrl = details.Album?.CoverMedium;
                    AlbumCoverBigUrl = details.Album?.CoverBig;
                }
                // Актуализираме основната снимка с по-голямата!
                if (!string.IsNullOrWhiteSpace(AlbumCoverBigUrl))
                {
                    AlbumCoverUrl = AlbumCoverBigUrl;
                }
            }
            catch (Exception ex)
            {
                ReleaseDate = $"Грешка при зареждане: {ex.Message}";
            }
        });
    }
}
