using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity; // За RoutedEventArgs
using System; // За Exception
using System.IO; // За File
using System.Net.Http; // За HttpClient
using LyricsInsight.ViewModels; // За да "виждаме" ViewModel-a

namespace LyricsInsight.Views;

public partial class SongDetailsView : UserControl
{
    public SongDetailsView()
    {
        InitializeComponent();
        SaveLyricsButton.Click += SaveLyricsButton_Click;
        SaveAnalysisButton.Click += SaveAnalysisButton_Click;
        SaveCoverSmallButton.Click += (s, e) => 
            SaveCoverAsync(vm => vm.AlbumCoverSmallUrl, "(Small)");

        SaveCoverMediumButton.Click += (s, e) => 
            SaveCoverAsync(vm => vm.AlbumCoverMediumUrl, "(Medium)");

        SaveCoverBigButton.Click += (s, e) => 
            SaveCoverAsync(vm => vm.AlbumCoverBigUrl, "(Large)");
    }
    
    // (Този метод идва след конструктора)

// Приема функция, която взема URL-а от ViewModel-a, и суфикс за името
    private async void SaveCoverAsync(Func<SongDetailsViewModel, string> getUrlFunc, string sizeSuffix)
    {
        var vm = this.DataContext as SongDetailsViewModel;
        var url = getUrlFunc(vm); // Изпълнява функцията, за да вземе правилния URL

        if (vm == null || string.IsNullOrWhiteSpace(url))
        {
            Console.WriteLine("Грешка: Няма URL за запазване.");
            return; // Не прави нищо, ако няма URL
        }

        var saveDialog = new SaveFileDialog
        {
            Title = $"Запазване на Корица {sizeSuffix}",
            InitialFileName = $"{vm.Artist} - {vm.Album} {sizeSuffix}.jpg",
            Filters = { new FileDialogFilter { Name = "JPEG Image", Extensions = { "jpg", "jpeg" } } }
        };

        var window = (this.VisualRoot as Window);
        var path = await saveDialog.ShowAsync(window);

        if (!string.IsNullOrWhiteSpace(path))
        {
            try
            {
                var imageBytes = await _httpClient.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(path, imageBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving cover: {ex.Message}");
            }
        }
    }
    
    // СТАТИЧЕН HttpClient за изтегляне на корицата
    private static readonly HttpClient _httpClient = new HttpClient();

    // МЕТОД 1: Запазване на ТЕКСТА
    private async void SaveLyricsButton_Click(object sender, RoutedEventArgs e)
    {
        // 1. Вземи ViewModel-a
        var vm = this.DataContext as SongDetailsViewModel;
        if (vm == null || string.IsNullOrWhiteSpace(vm.LyricsText)) return;

        // 2. Отвори диалога за запазване
        var saveDialog = new SaveFileDialog
        {
            Title = "Запазване на Текст",
            InitialFileName = $"{vm.Artist} - {vm.Title}.txt",
            Filters = { new FileDialogFilter { Name = "Text File", Extensions = { "txt" } } }
        };

        var window = (this.VisualRoot as Window); // Вземи прозореца
        var path = await saveDialog.ShowAsync(window); // Подай прозореца

        // 3. Запиши файла
        if (!string.IsNullOrWhiteSpace(path))
        {
            try
            {
                await File.WriteAllTextAsync(path, vm.LyricsText);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving lyrics: {ex.Message}");
            }
        }
    }
    
    // МЕТОD 2: Запазване на АНАЛИЗА
    private async void SaveAnalysisButton_Click(object sender, RoutedEventArgs e)
    {
        var vm = this.DataContext as SongDetailsViewModel;
        if (vm == null || string.IsNullOrWhiteSpace(vm.AiAnalysisText)) return;

        var saveDialog = new SaveFileDialog
        {
            Title = "Запазване на Анализ",
            InitialFileName = $"{vm.Artist} - {vm.Title} (Анализ).md",
            Filters = { new FileDialogFilter { Name = "Markdown File", Extensions = { "md" } } }
        };

        var window = (this.VisualRoot as Window); // Вземи прозореца
        var path = await saveDialog.ShowAsync(window); // Подай прозореца

        if (!string.IsNullOrWhiteSpace(path))
        {
            try
            {
                await File.WriteAllTextAsync(path, vm.AiAnalysisText);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving analysis: {ex.Message}");
            }
        }
    }

// МЕТОД 3: Запазване на КОРИЦАТА
    private async void SaveCoverButton_Click(object sender, RoutedEventArgs e)
    {
        var vm = this.DataContext as SongDetailsViewModel;
        if (vm == null || string.IsNullOrWhiteSpace(vm.AlbumCoverUrl)) return;

        var saveDialog = new SaveFileDialog
        {
            Title = "Запазване на Корица",
            InitialFileName = $"{vm.Artist} - {vm.Album}.jpg", // По подразбиране .jpg
            Filters = { new FileDialogFilter { Name = "JPEG Image", Extensions = { "jpg", "jpeg" } } }
        };

        var window = (this.VisualRoot as Window); // Вземи прозореца
        var path = await saveDialog.ShowAsync(window); // Подай прозореца

        if (!string.IsNullOrWhiteSpace(path))
        {
            try
            {
                // 1. Изтегли байтовете от URL-а
                var imageBytes = await _httpClient.GetByteArrayAsync(vm.AlbumCoverUrl);
                // 2. Запиши ги във файла
                await File.WriteAllBytesAsync(path, imageBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving cover: {ex.Message}");
            }
        }
    }
}