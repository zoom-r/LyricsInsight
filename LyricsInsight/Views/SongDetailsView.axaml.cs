using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.IO;
using System.Net.Http;
using LyricsInsight.ViewModels;
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
    
    private static readonly HttpClient HttpClient = new HttpClient();

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

    private async void SaveAnalysisButton_Click(object sender, RoutedEventArgs e)
    {
        var vm = this.DataContext as SongDetailsViewModel;
        if (vm == null || string.IsNullOrWhiteSpace(vm.AiAnalysisText)) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var pdfFileType = new FilePickerFileType("PDF Document")
        {
            Patterns = new[] { "*.pdf" }
        };

        var filePickerOptions = new FilePickerSaveOptions
        {
            Title = "Запазване на Анализ",
            SuggestedFileName = $"{vm.ArtistName} - {vm.TrackTitle} (Анализ).pdf",
            FileTypeChoices = new[] { pdfFileType }
        };

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(filePickerOptions);
        if(file == null ) return;
        string tempMdPath = null;
    
        try
        {
            var markdown = vm.AiAnalysisText;
        
            var outputPdfPath = file.Path.LocalPath;

            tempMdPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.md");

            await File.WriteAllTextAsync(tempMdPath, markdown, Encoding.UTF8);
            
            var options = new Markdown2PdfOptions {
                HeaderHtml = "<div class=\"document-title\" width: 100%; padding: 5px\"></div>",
                FooterHtml = "<div style=\"width: 100%; padding: 5px;\"><span class=\"pageNumber\" align=\"left\"></span>/<span class=\"totalPages\"></span></div>",
                MarginOptions = new MarginOptions(){Bottom = "30", Left = "30", Right = "30", Top = "30"}
            };
            var converter = new Markdown2PdfConverter(options);

            await converter.Convert(tempMdPath, outputPdfPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving PDF analysis: {ex.Message}");
        }
        finally
        {
            if (tempMdPath != null && File.Exists(tempMdPath))
            {
                File.Delete(tempMdPath);
            }
        }
    }

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
                var imageBytes = await HttpClient.GetByteArrayAsync(urlToDownload);
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