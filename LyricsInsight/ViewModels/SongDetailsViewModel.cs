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
        private readonly LyricsService _lyricsService;
        private readonly GenAiService _genAiService;
        private readonly DeezerService _deezerService;
        
        private Track? _track;
        private Album? _album;
        private Artist? _artist;
        private LyricsResult? _lyrics;
        
        private readonly string _songId;
        private readonly string _albumId;
        private readonly string _artistId;
        
        private bool _isLoadingDetails;
        public bool IsLoadingDetails
        {
            get => _isLoadingDetails;
            set => this.RaiseAndSetIfChanged(ref _isLoadingDetails, value);
        }
        
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
        
        public ICommand BackCommand { get; }
        public ICommand OpenLinkCommand { get; }
        
        public string TrackLink => _track == null ? "..." : _track.Link;
        public string TrackTitle => _track == null ? "..." : _track.Title;

        public string FormattedTrackReleaseDate => _track == null ? "Зареждане..." : $"Дата на издаване: {_track.ReleaseDate:dd/MM/yyyy}";
        public string FormattedTrackDuration => _track == null ? "..." : $"Продължителност: {_track.Duration:m\\:ss}";
        public string FormattedTrackPosition => _track?.TrackPosition == null ? null : $"Позиция в албума: {_track.TrackPosition}";
        public string FormattedTrackRank => _track?.Rank == null ? null : $"Ранк: #{_track.Rank:N0}";
        public string FormattedBpm => _track?.Bpm == null || _track?.Bpm == 0 ? null : $"Удара в минута: {_track.Bpm}";
        public string FormattedTrackArtists => _track == null ? "..." : _track.Artists;
        public string FormattedTrackDiskNumber => _track?.DiskNumber == null ? "..." : $"Диск: {_track.DiskNumber}";
        
        public string AlbumTitle => _album == null ? "..." : _album.Title;
        public string AlbumLink => _album ==null ? "..." : _album.Link;
        
        public string FormattedAlbumGenres => _album == null ? "..." : $"Жанр: {string.Join(", ",  _album.Genres)}";
        public string FormattedAlbumLabel => _album == null ? "..." : $"Издателство: {_album.Label}";
        public string FormattedAlbumFans => _album?.Fans == null ? "..." : $"Фенове: {_album.Fans:N0}";
        public string FormattedRecordType => _album == null ? "..." : $"Вид на изданието: {_album.RecordType}";
        public string FormattedAlbumDuration => _album?.Duration == null ? "..." : $"Продължителност: {_album.Duration:hh\\:mm\\:ss}";
        
        public string CoverUrlSmall => _album?.CoverSmall;
        public string CoverUrlMedium => _album?.CoverMedium;
        public string CoverUrlBig => _album?.CoverBig;
        public string CoverUrlXl => _album?.CoverXl;
        
        public string ArtistName => _artist == null ? "..." : _artist.Name;
        public string ArtistLink => _artist ==  null ? "..." : _artist.Link;
        public string ArtistPicture => _artist == null ? "..." : _artist.Picture;
        public string ArtistNbAlbum => _artist?.NbAlbum == null ? "..." : $"Брой албуми: {_artist.NbAlbum}";
        public string ArtistNbFan => _artist?.NbFan == null ? "..." : $"Фенове:  {_artist.NbFan:N0}";
        
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
        
        public SongDetailsViewModel(SongSearchResult selectedSong, LyricsService lyricsService, GenAiService genAiService, DeezerService deezerService, Action onGoBack)
        {
            _lyricsService = lyricsService;
            _genAiService = genAiService;
            _deezerService = deezerService;
            
            BackCommand = ReactiveCommand.Create(onGoBack, outputScheduler: RxApp.MainThreadScheduler);
            OpenLinkCommand = ReactiveCommand.Create<string>(OpenUrl);
            
            _songId = selectedSong.Id;
            _albumId = selectedSong.AlbumId;
            _artistId = selectedSong.ArtistId;
            
            IsLoadingDetails = true;
            IsLoadingLyrics = true;
            IsLoadingAnalysis = true;
            IsAnalysisReady = false;

            LoadDetails();
        }
        
        private void LoadDetails()
        {
            Task.Run(async () =>
            {
                try
                {
                    var trackTask = _deezerService.GetTrackDetailsAsync(_songId);
                    var albumTask = _deezerService.GetAlbumDetailsAsync(_albumId);
                    var artistTask = _deezerService.GetArtistDetailsAsync(_artistId);

                    await Task.WhenAll(trackTask, albumTask,  artistTask);

                    var trackResult = await trackTask;
                    var albumResult = await albumTask;
                    var artistResult = await artistTask;

                    Dispatcher.UIThread.Post(() =>
                    {
                        _track = trackResult;
                        _album = albumResult;
                        _artist = artistResult;
                        
                        IsLoadingDetails = false;
                        
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
                    var lyricsResult = await _lyricsService.GetLyricsAsync(ArtistName, TrackTitle);

                    Dispatcher.UIThread.Post(() =>
                    {
                        IsLoadingLyrics = false;
                        _lyrics = lyricsResult;
                        
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
                    string albumName = _album?.Title ?? "Неизвестен албум";
                    
                    var (analysis, success) = await _genAiService.GenerateAnalysisAsync(lyricsToAnalyze, TrackTitle, ArtistName, albumName);

                    Dispatcher.UIThread.Post(() =>
                    {
                        IsLoadingAnalysis = false;
                        AiAnalysisText = analysis;
                        IsAnalysisReady = success;
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
            if (string.IsNullOrWhiteSpace(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                Console.WriteLine($"Опит за отваряне на невалиден URL: {url}");
                return;
            }

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
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