using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using Google.GenAI;
using LyricsInsight.Core;
using LyricsInsight.Core.Models;
using LyricsInsight.Core.Services;
using ReactiveUI;

namespace LyricsInsight.ViewModels
{
    public class SongDetailsViewModel : ViewModelBase
    {
        // --- 1. СЕРВИЗИ ---
        private readonly LyricsService _lyricsService;
        private readonly GenAiService _genAiService;
        private readonly DeezerService _deezerService;
        
        // --- 2. СУРОВИ МОДЕЛИ (ЧИСТИ ДАННИ) ---
        // Това са "суровите" данни, които получаваме от сервизите.
        // Те са private, защото UI-ят не трябва да ги интересува.
        private Track? _track;
        private Album? _album;
        private Artist? _artist;
        private LyricsResult? _lyrics;
        
        // --- 3. ПОЛЕТА ЗА СЪСТОЯНИЕ (КАКВОТО ИМАХМЕ) ---
        private readonly string _songId;
        private readonly string _albumId;
        private readonly string _artistId;
        
        private bool _isLoadingDetails; // За Track и Album
        public bool IsLoadingDetails
        {
            get => _isLoadingDetails;
            set => this.RaiseAndSetIfChanged(ref _isLoadingDetails, value);
        }
        
        // ... (IsLoadingLyrics, IsLoadingAnalysis, IsAnalysisReady са същите) ...
        #region Loading Properties
        private bool _isLoadingLyrics;
        public bool IsLoadingLyrics
        {
            get => _isLoadingLyrics;
            set => this.RaiseAndSetIfChanged(ref _isLoadingLyrics, value);
        }

        private bool _isLoadingAnalysis;
        public bool IsLoadingAnalysis
        {
            get => _isLoadingAnalysis;
            set => this.RaiseAndSetIfChanged(ref _isLoadingAnalysis, value);
        }
        
        private bool _isAnalysisReady;
        public bool IsAnalysisReady
        {
            get => _isAnalysisReady;
            set => this.RaiseAndSetIfChanged(ref _isAnalysisReady, value);
        }
        #endregion

        // --- 4. ПУБЛИЧНИ СВОЙСТВА (ЗА UI) ---
        // Това са свойствата, за които XAML-ът се "връзва".
        // Те ЧЕТАТ от суровите модели и ги ФОРМАТИРАТ.
        
        public ICommand BackCommand { get; }
        public ICommand OpenLinkCommand { get; }
        
        // // Данни от търсенето (вече ги имаме)
        // public string Title { get; }
        // public string Artist { get; }
        // // public string InitialAlbumCoverUrl { get; } // Корицата от търсенето
        // public string AlbumName { get; }
        
        //
        public string TrackLink => _track == null ? "..." : _track.Link;
        public string TrackTitle => _track == null ? "..." : _track.Title;

        // --- Форматирани данни от _track (който се зарежда) ---
        public string FormattedTrackReleaseDate => _track == null ? "Зареждане..." : $"Дата на издаване: {_track.ReleaseDate:dd/MM/yyyy}";
        public string FormattedTrackDuration => _track == null ? "..." : $"Продължителност: {_track.Duration:m\\:ss}";
        public string FormattedTrackPosition => _track?.TrackPosition == null ? null : $"Позиция в албума: {_track.TrackPosition}";
        public string FormattedTrackRank => _track?.Rank == null ? null : $"Ранк: #{_track.Rank:N0}";
        public string FormattedBpm => _track?.Bpm == null || _track?.Bpm == 0 ? null : $"Удара в минута: {_track.Bpm}";
        public string FormattedTrackArtists => _track == null ? "..." : _track.Artists;
        public string FormattedTrackDiskNumber => _track?.DiskNumber == null ? "..." : $"Диск: {_track.DiskNumber}";
        
        
        //
        public string AlbumTitle => _album == null ? "..." : _album.Title;
        public string AlbumLink => _album ==null ? "..." : _album.Link;
        
        
        // --- Форматирани данни от _album (който се зарежда) ---
        public string FormattedAlbumGenres => _album == null ? "..." : $"Жанр: {string.Join(", ",  _album.Genres)}";
        public string FormattedAlbumLabel => _album == null ? "..." : $"Издателство: {_album.Label}";
        public string FormattedAlbumFans => _album?.Fans == null ? "..." : $"Фенове: {_album.Fans:N0}";
        public string FormattedRecordType => _album == null ? "..." : $"Вид на изданието: {_album.RecordType}";
        public string FormattedAlbumDuration => _album?.Duration == null ? "..." : $"Продължителност: {_album.Duration:hh\\:mm\\:ss}";
        
        
        // --- Данни за кориците (от _album) ---
        public string CoverUrlSmall => _album?.CoverSmall;
        public string CoverUrlMedium => _album?.CoverMedium;
        public string CoverUrlBig => _album?.CoverBig; // Ако няма голяма, върни тази от търсенето
        public string CoverUrlXl => _album?.CoverXl;
        
        //
        public string ArtistName => _artist == null ? "..." : _artist.Name;
        public string ArtistLink => _artist ==  null ? "..." : _artist.Link;
        public string ArtistPicture => _artist == null ? "..." : _artist.Picture;
        public string ArtistNbAlbum => _artist?.NbAlbum == null ? "..." : $"Брой албуми: {_artist.NbAlbum}";
        public string ArtistNbFan => _artist?.NbFan == null ? "..." : $"Фенове:  {_artist.NbFan:N0}";
        
        // --- Данни за текстовете (от _lyrics) ---
        private string? _lyricsText;
        public string LyricsText
        {
            get => _lyricsText;
            set => this.RaiseAndSetIfChanged(ref _lyricsText, value);
        }

        private string? _aiAnalysisText;
        public string AiAnalysisText
        {
            get => _aiAnalysisText;
            set => this.RaiseAndSetIfChanged(ref _aiAnalysisText, value);
        }
        
        // --- 5. КОНСТРУКТОР ---
        public SongDetailsViewModel(SongSearchResult selectedSong, LyricsService lyricsService, GenAiService genAiService, DeezerService deezerService, Action onGoBack)
        {
            // Запазваме сервизите
            _lyricsService = lyricsService;
            _genAiService = genAiService;
            _deezerService = deezerService;
            
            BackCommand = ReactiveCommand.Create(onGoBack, outputScheduler: RxApp.MainThreadScheduler);
            OpenLinkCommand = ReactiveCommand.Create<string>(OpenUrl);
            
            // Запазваме данните от търсенето
            _songId = selectedSong.Id;
            _albumId = selectedSong.AlbumId;
            _artistId = selectedSong.ArtistId;
            
            // Включваме спинърите
            IsLoadingDetails = true;
            IsLoadingLyrics = true;
            IsLoadingAnalysis = true;
            IsAnalysisReady = false;

            // СТАРТИРАМЕ ВСИЧКО АСИНХРОННО
            LoadDetails();
        }
        
        // --- 6. МЕТОДИ ЗА ЗАРЕЖДАНЕ (С ПРАВИЛЕН THREADING) ---
        
        private void LoadDetails()
        {
            Task.Run(async () =>
            {
                try
                {
                    // Изпълняваме двете заявки ПАРАЛЕЛНО
                    var trackTask = _deezerService.GetTrackDetailsAsync(_songId);
                    var albumTask = _deezerService.GetAlbumDetailsAsync(_albumId);
                    var artistTask = _deezerService.GetArtistDetailsAsync(_artistId);

                    // Чакаме да приключат
                    await Task.WhenAll(trackTask, albumTask,  artistTask);

                    // Вземаме "суровите" МОДЕЛИ
                    var trackResult = await trackTask;
                    var albumResult = await albumTask;
                    var artistResult = await artistTask;

                    // --- ТУК Е КЛЮЧЪТ: ВРЪЩАМЕ СЕ НА UI НИШКАТА ---
                    Dispatcher.UIThread.Post(() =>
                    {
                        // 1. Запазваме суровите модели
                        _track = trackResult;
                        _album = albumResult;
                        _artist = artistResult;
                        
                        // 2. Спираме спинъра
                        IsLoadingDetails = false;
                        
                        // 3. УВЕДОМЯВАМЕ UI-a, че форматираните свойства са се променили
                        // UI-ят сега ще извика "get"-ърите на всички тези свойства
                        this.RaisePropertyChanged(nameof(FormattedTrackReleaseDate));
                        this.RaisePropertyChanged(nameof(FormattedTrackDuration));
                        this.RaisePropertyChanged(nameof(FormattedTrackPosition));
                        this.RaisePropertyChanged(nameof(FormattedTrackRank));
                        this.RaisePropertyChanged(nameof(FormattedBpm));
                        this.RaisePropertyChanged(nameof(FormattedTrackArtists));
                        this.RaisePropertyChanged(nameof(FormattedTrackDiskNumber));
                        this.RaisePropertyChanged(nameof(TrackLink));
                        this.RaisePropertyChanged(nameof(TrackTitle));
                        
                        this.RaisePropertyChanged(nameof(FormattedAlbumGenres));
                        this.RaisePropertyChanged(nameof(FormattedAlbumLabel));
                        this.RaisePropertyChanged(nameof(FormattedAlbumFans));
                        this.RaisePropertyChanged(nameof(FormattedRecordType));
                        this.RaisePropertyChanged(nameof(FormattedAlbumDuration));
                        this.RaisePropertyChanged(nameof(AlbumTitle));
                        this.RaisePropertyChanged(nameof(AlbumLink));
                        
                        this.RaisePropertyChanged(nameof(CoverUrlSmall));
                        this.RaisePropertyChanged(nameof(CoverUrlMedium));
                        this.RaisePropertyChanged(nameof(CoverUrlBig));
                        this.RaisePropertyChanged(nameof(CoverUrlXl));
                        
                        this.RaisePropertyChanged(nameof(ArtistName));
                        this.RaisePropertyChanged(nameof(ArtistLink));
                        this.RaisePropertyChanged(nameof(ArtistPicture));
                        this.RaisePropertyChanged(nameof(ArtistNbAlbum));
                        this.RaisePropertyChanged(nameof(ArtistNbFan));
                        
                        LoadLyrics();
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        IsLoadingDetails = false;
                        // Можем да покажем грешката някъде
                        // FormattedReleaseDate = $"Грешка: {ex.Message}";
                    });
                }
            });
        }
        
        private void LoadLyrics()
        {
            Task.Run(async () =>
            {
                try
                {
                    // Това е на фонова нишка (OK)
                    var lyricsResult = await _lyricsService.GetLyricsAsync(ArtistName, TrackTitle);

                    // ВРЪЩАМЕ СЕ НА UI НИШКАТА
                    Dispatcher.UIThread.Post(() =>
                    {
                        IsLoadingLyrics = false;
                        _lyrics = lyricsResult; // Запазваме суровия резултат
                        
                        if (_lyrics == null || string.IsNullOrWhiteSpace(_lyrics.Text))
                        {
                            LyricsText = "За съжаление текстът на песента не беше намерен.";
                            AiAnalysisText = "Няма текст, върху който да се извърши анализ.";
                            IsLoadingAnalysis = false;
                            IsAnalysisReady = false;
                        }
                        else
                        {
                            string footer = $"\n\n\n(Източник: {_lyrics.Source})";
                            LyricsText = _lyrics.Text + footer;
                            
                            // Извикваме анализа
                            LoadAnalysis(_lyrics.Text);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        IsLoadingLyrics = false;
                        IsLoadingAnalysis = false;
                        LyricsText = $"Възникна грешка при зареждането на текста: {ex.Message}";
                        AiAnalysisText = "Анализът не може да продължи.";
                    });
                }
            });
        }
    
        private void LoadAnalysis(string lyricsToAnalyze)
        {
            Task.Run(async () =>
            {
                try
                {
                    // Това е на фонова нишка (OK)
                    // (Предполагам, че новият ти 'Track' модел има 'Album' свойство)
                    string albumName = _album?.Title ?? "Неизвестен албум";
                    
                    var (analysis, success) = await _genAiService.GenerateAnalysisAsync(lyricsToAnalyze, TrackTitle, ArtistName, albumName);

                    // ВРЪЩАМЕ СЕ НА UI НИШКАТА
                    Dispatcher.UIThread.Post(() =>
                    {
                        IsLoadingAnalysis = false;
                        AiAnalysisText = analysis; // Твоят GenAiService вече връща само string
                        IsAnalysisReady = success; // (Предполагам, че ако не гръмне, е 'true')
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        AiAnalysisText = ex is ServerError
                            ? "Има проблеми със сървърите на Google GenAI. Моля опитайте по-късно"
                            : $"Грешка при генерирането на анализ: {ex.Message}";
                        IsLoadingAnalysis = false;
                        IsAnalysisReady = false;
                    });
                }
            });
        }
        
        /// <summary>
        /// Отваря URL в браузъра по подразбиране,
        /// като се съобразява с операционната система.
        /// </summary>
        private void OpenUrl(string url)
        {
            // Провери дали URL-ът е валиден, преди да се опиташ да го отвориш
            if (string.IsNullOrWhiteSpace(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                Console.WriteLine($"Опит за отваряне на невалиден URL: {url}");
                return;
            }

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // За Windows
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // За Linux
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // За macOS (както си ти)
                    Process.Start("open", url);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Грешка при отваряне на URL: {ex.Message}");
            }
        }
    }
}