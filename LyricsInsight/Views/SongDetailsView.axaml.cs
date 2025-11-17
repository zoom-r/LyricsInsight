using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity; // За RoutedEventArgs
using System; // За Exception
using System.IO; // За File
using System.Net.Http; // За HttpClient
using LyricsInsight.ViewModels; // За да "виждаме" ViewModel-a
using Avalonia.Platform.Storage;
using Markdown2Pdf;
using System.Text;
using Markdown2Pdf.Options;

namespace LyricsInsight.Views;

public partial class SongDetailsView : UserControl
{
    public SongDetailsView()
    {
        InitializeComponent();
        SaveLyricsButton.Click += SaveLyricsButton_Click;
        SaveAnalysisButton.Click += SaveAnalysisButton_Click;
        
        SaveCoverSmall.Click += (s, e) => SaveCoverButton_Click("small");
        SaveCoverMedium.Click += (s, e) => SaveCoverButton_Click("medium");
        SaveCoverBig.Click += (s, e) => SaveCoverButton_Click("big");
        SaveCoverXl.Click += (s, e) => SaveCoverButton_Click("xl");
    }
    
    // СТАТИЧЕН HttpClient за изтегляне на корицата
    private static readonly HttpClient _httpClient = new HttpClient();

    // МЕТОД 1: Запазване на ТЕКСТА (НОВ)
    private async void SaveLyricsButton_Click(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as SongDetailsViewModel;
        if (vm == null || string.IsNullOrWhiteSpace(vm.LyricsText)) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var txtFileType = new FilePickerFileType("Text File")
        {
            Patterns = new[] { "*.txt" }
        };

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Запазване на Текст",
            SuggestedFileName = $"{vm.ArtistName} - {vm.TrackTitle} (Текст).txt",
            FileTypeChoices = new[] { txtFileType }
        });

        if (file != null)
        {
            try
            {
                await using var stream = await file.OpenWriteAsync();
                await using var writer = new StreamWriter(stream, Encoding.UTF8);
                await writer.WriteAsync(vm.LyricsText);
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

        // 2. Дефинираме PDF типа
        var pdfFileType = new FilePickerFileType("PDF Document")
        {
            Patterns = new[] { "*.pdf" }
        };

        var filePickerOptions = new FilePickerSaveOptions
        {
            Title = "Запазване на Анализ",
            SuggestedFileName = $"{vm.ArtistName} - {vm.TrackTitle} (Анализ).pdf", // <-- .pdf
            FileTypeChoices = new[] { pdfFileType } // <-- PDF тип
        };

        // 3. Отваряме диалога
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(filePickerOptions);
        if(file == null ) return;
        string tempMdPath = null; // Пътят до нашия временен .md файл
    
        try
        {
            // Вземаме Markdown текста
            var markdown = vm.AiAnalysisText;
        
            // Вземаме пътя, където потребителят иска да запази PDF-а
            var outputPdfPath = file.Path.LocalPath;

            // Създаваме уникално име за временен .md файл
            tempMdPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.md");

            // Записваме нашия Markdown във временния файл
            await File.WriteAllTextAsync(tempMdPath, markdown, Encoding.UTF8);
            
            var options = new Markdown2PdfOptions {
                HeaderHtml = "<div class=\"document-title\" width: 100%; padding: 5px\"></div>",
                FooterHtml = "<div style=\"width: 100%; padding: 5px;\"><span class=\"pageNumber\" align=\"left\"></span>/<span class=\"totalPages\"></span></div>",
                MarginOptions = new MarginOptions(){Bottom = "30", Left = "30", Right = "30", Top = "30"}
            };
            // Създаваме конвертора
            var converter = new Markdown2PdfConverter(options);

            // Казваме на конвертора:
            // "Прочети от ТОЗИ .md файл и го запиши в ТОЗИ .pdf файл"
            await converter.Convert(tempMdPath, outputPdfPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving PDF analysis: {ex.Message}");
        }
        finally
        {
            // 3. (МНОГО ВАЖНО) Почистваме временния файл
            if (tempMdPath != null && File.Exists(tempMdPath))
            {
                File.Delete(tempMdPath);
            }
        }
    }

    // МЕТОД 3: Запазване на КОРИЦАТА (НОВ)
    private async void SaveCoverButton_Click(string size)
    {
        var vm = DataContext as SongDetailsViewModel;
        if (vm == null) return;

        string urlToDownload = size switch
        {
            "small" => vm.CoverUrlSmall,
            "medium" => vm.CoverUrlMedium,
            "big" => vm.CoverUrlBig,
            "xl" => vm.CoverUrlXl,
            _ => vm.CoverUrlBig
        };

        if (string.IsNullOrWhiteSpace(urlToDownload)) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var fileType = new FilePickerFileType("JPEG Image")
        {
            Patterns = new[] { "*.jpg" }
        };

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Запазване на корица",
            SuggestedFileName = $"{vm.ArtistName} - {vm.TrackTitle} (Корица).jpg",
            FileTypeChoices = new[] { fileType }
        });

        if (file != null)
        {
            try
            {
                var imageBytes = await _httpClient.GetByteArrayAsync(urlToDownload);
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