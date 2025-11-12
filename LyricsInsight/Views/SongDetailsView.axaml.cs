using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity; // За RoutedEventArgs
using System; // За Exception
using System.IO; // За File
using System.Net.Http; // За HttpClient
using LyricsInsight.ViewModels; // За да "виждаме" ViewModel-a
using Avalonia.Platform.Storage;

namespace LyricsInsight.Views;

public partial class SongDetailsView : UserControl
{
    public SongDetailsView()
    {
        InitializeComponent();
        SaveLyricsButton.Click += SaveLyricsButton_Click;
        SaveAnalysisButton.Click += SaveAnalysisButton_Click;
        // ДОБАВИ ТЕЗИ РЕДОВЕ:
        SaveCoverSmallItem.Click += (s, e) => 
            SaveCoverAsync(vm => vm.AlbumCoverSmallUrl, "(Малка 56x56)");

        SaveCoverMediumItem.Click += (s, e) => 
            SaveCoverAsync(vm => vm.AlbumCoverMediumUrl, "(Средна 250x250)");

        SaveCoverBigItem.Click += (s, e) => 
            SaveCoverAsync(vm => vm.AlbumCoverBigUrl, "(Голяма 500x500)");
    }
    
    // СТАТИЧЕН HttpClient за изтегляне на корицата
    private static readonly HttpClient _httpClient = new HttpClient();

    // МЕТОД 1: Запазване на ТЕКСТА (НОВ)
    private async void SaveLyricsButton_Click(object sender, RoutedEventArgs e)
    {
        var vm = this.DataContext as SongDetailsViewModel;
        if (vm == null || string.IsNullOrWhiteSpace(vm.LyricsText)) return;

        // 1. Вземи StorageProvider-а от TopLevel
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;
        
        // 2. Създай опциите за файла
        var filePickerOptions = new FilePickerSaveOptions
        {
            Title = "Запазване на Текст",
            SuggestedFileName = $"{vm.Artist} - {vm.Title}.txt",
            FileTypeChoices = new[] { FilePickerFileTypes.TextPlain }
        };

        // 3. Отвори диалога
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(filePickerOptions);

        if (file != null)
        {
            try
            {
                // 4. Запиши във файла (по новия начин)
                await using var stream = await file.OpenWriteAsync();
                await using var streamWriter = new StreamWriter(stream, System.Text.Encoding.UTF8);
                await streamWriter.WriteAsync(vm.LyricsText);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving lyrics: {ex.Message}");
            }
        }
    }

    // МЕТОД 2: Запазване на АНАЛИЗА (НОВ)
    private async void SaveAnalysisButton_Click(object sender, RoutedEventArgs e)
    {
        var vm = this.DataContext as SongDetailsViewModel;
        if (vm == null || string.IsNullOrWhiteSpace(vm.AiAnalysisText)) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        // "md" не е стандартен тип, затова го дефинираме ръчно
        var mdFileType = new FilePickerFileType("Markdown File")
        {
            Patterns = new[] { "*.md" }
        };

        var filePickerOptions = new FilePickerSaveOptions
        {
            Title = "Запазване на Анализ",
            SuggestedFileName = $"{vm.Artist} - {vm.Title} (Анализ).md",
            FileTypeChoices = new[] { mdFileType }
        };

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(filePickerOptions);

        if (file != null)
        {
            try
            {
                await using var stream = await file.OpenWriteAsync();
                await using var streamWriter = new StreamWriter(stream, System.Text.Encoding.UTF8);
                await streamWriter.WriteAsync(vm.AiAnalysisText);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving analysis: {ex.Message}");
            }
        }
    }

    // МЕТОД 3: Запазване на КОРИЦАТА (НОВ)
    private async void SaveCoverAsync(Func<SongDetailsViewModel, string> getUrlFunc, string sizeSuffix)
    {
        var vm = this.DataContext as SongDetailsViewModel;
        var url = getUrlFunc(vm);

        if (vm == null || string.IsNullOrWhiteSpace(url))
        {
            Console.WriteLine("Грешка: Няма URL за запазване.");
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var filePickerOptions = new FilePickerSaveOptions
        {
            Title = $"Запазване на Корица {sizeSuffix}",
            SuggestedFileName = $"{vm.Artist} - {vm.Album} {sizeSuffix}.jpg",
            FileTypeChoices = new[] { FilePickerFileTypes.ImageJpg }
        };

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(filePickerOptions);

        if (file != null)
        {
            try
            {
                // 1. Изтегли байтовете (както преди)
                var imageBytes = await _httpClient.GetByteArrayAsync(url);
                
                // 2. Запиши байтовете в stream-а (по новия начин)
                await using var stream = await file.OpenWriteAsync();
                await stream.WriteAsync(imageBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving cover: {ex.Message}");
            }
        }
    }
}