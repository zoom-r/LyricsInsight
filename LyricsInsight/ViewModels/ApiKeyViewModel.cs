using System;
using System.Reactive; // За Unit
using System.Reactive.Linq; // За IObservable
using System.Windows.Input;
using LyricsInsight.Core.Services; // Нашият SettingsService
using ReactiveUI;

namespace LyricsInsight.ViewModels;

public class ApiKeyViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;
    private readonly Action<string> _onKeySaved; // "Callback"

    private string _apiKey;
    public string ApiKey
    {
        get => _apiKey;
        set => this.RaiseAndSetIfChanged(ref _apiKey, value);
    }

    public ICommand SaveCommand { get; }

    public ApiKeyViewModel(SettingsService settingsService, Action<string> onKeySaved)
    {
        _settingsService = settingsService;
        _onKeySaved = onKeySaved; // Запазваме "callback"-а

        // Определяме кога бутонът "Запази" е активен
        // (само когато текстът в ApiKey не е празен)
        var canSave = this.WhenAnyValue(
            vm => vm.ApiKey,
            (key) => !string.IsNullOrWhiteSpace(key)
        );

        // Създаваме командата
        // ReactiveCommand.CreateFromTask е за async команди
        SaveCommand = ReactiveCommand.CreateFromTask(
            async () =>
            {
                // 1. Запазваме ключа във файла
                await _settingsService.SaveKeyAsync(ApiKey);
                
                // 2. Извикваме "callback"-а, за да кажем на MainViewModel,
                // че сме готови и му подаваме новия ключ
                _onKeySaved(ApiKey);
            },
            canSave // Активираме бутона само ако canSave е 'true'
        );
    }
}
