// Файл: LyricsInsight.Tests/ApiKeyViewModelTests.cs

using Xunit; // Това е xUnit framework
using Moq; // Това е Moq
using System;
using LyricsInsight.Core.Services; // За SettingsService
using LyricsInsight.ViewModels; // За ApiKeyViewModel

namespace LyricsInsight.Tests;

public class ApiKeyViewModelTests
{
    // "Mock" (фалшив) SettingsService.
    // Ние не искаме тестът ни да записва истински файлове.
    private readonly Mock<SettingsService> _mockSettingsService;
    
    // "Mock" (фалшив) callback Action
    private readonly Mock<Action<string>> _mockOnKeySaved;

    public ApiKeyViewModelTests()
    {
        // Инициализираме фалшивите обекти преди всеки тест
        _mockSettingsService = new Mock<SettingsService>();
        _mockOnKeySaved = new Mock<Action<string>>();
    }

    // [Fact] е атрибут, който казва "това е тест"
    [Fact]
    public void SaveCommand_ShouldBeDisabled_WhenApiKeyIsEmpty()
    {
        // --- 1. Подготовка (Arrange) ---
        // Създаваме ViewModel-а, като му подаваме ФАЛШИВИТЕ зависимости
        var vm = new ApiKeyViewModel(
            _mockSettingsService.Object, 
            _mockOnKeySaved.Object
        );
        
        // Задаваме условието, което тестваме (празно поле)
        vm.ApiKey = "";

        // --- 2. Действие (Act) ---
        // Проверяваме дали командата може да се изпълни
        bool canExecute = vm.SaveCommand.CanExecute(null);

        // --- 3. Проверка (Assert) ---
        // Твърдим, че CanExecute ТРЯБВА да е 'false'
        Assert.False(canExecute);
    }

    [Fact]
    public void SaveCommand_ShouldBeEnabled_WhenApiKeyIsNotEmpty()
    {
        // --- 1. Подготовка (Arrange) ---
        var vm = new ApiKeyViewModel(
            _mockSettingsService.Object, 
            _mockOnKeySaved.Object
        );
        
        // Задаваме условието (НЕ е празно поле)
        vm.ApiKey = "my-test-key";

        // --- 2. Действие (Act) ---
        bool canExecute = vm.SaveCommand.CanExecute(null);

        // --- 3. Проверка (Assert) ---
        // Твърдим, че CanExecute ТРЯБВА да е 'true'
        Assert.True(canExecute);
    }
    
    // Можем да тестваме и с 'null' или 'whitespace'
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")] // само интервали
    public void SaveCommand_ShouldBeDisabled_WhenApiKeyIsInvalid(string invalidKey)
    {
        // Arrange
        var vm = new ApiKeyViewModel(
            _mockSettingsService.Object, 
            _mockOnKeySaved.Object
        );
        vm.ApiKey = invalidKey;
        
        // Act
        bool canExecute = vm.SaveCommand.CanExecute(null);
        
        // Assert
        Assert.False(canExecute);
    }
}