using System;
using System.Windows.Input;
using LyricsInsight.Core.Services;
using ReactiveUI;

namespace LyricsInsight.ViewModels;

public class ApiKeyViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;
    private readonly Action<string> _onKeySaved;

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
        _onKeySaved = onKeySaved;
        
        var canSave = this.WhenAnyValue(
            vm => vm.ApiKey,
            (key) => !string.IsNullOrWhiteSpace(key)
        );
        
        SaveCommand = ReactiveCommand.CreateFromTask(
            async () =>
            {
                await _settingsService.SaveKeyAsync(ApiKey);
                _onKeySaved(ApiKey);
            },
            canSave
        );
    }
}
