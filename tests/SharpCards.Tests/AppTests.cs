using Moq;
using Spectre.Console;
using Spectre.Console.Testing;
using Xunit;

namespace SharpCards.Tests;

public class AppTests : IDisposable
{
    private readonly Mock<IStorage> _mockStorage;
    private readonly TestConsole _console;
    private readonly App _app;

    public AppTests()
    {
        _mockStorage = new Mock<IStorage>();
        _console = new TestConsole();
        _console.Profile.Capabilities.Interactive = true;
        AnsiConsole.Console = _console;
        _app = new App(_mockStorage.Object);
    }

    public void Dispose()
    {
        AnsiConsole.Console = AnsiConsole.Create(new AnsiConsoleSettings());
    }

    [Fact]
    public void Run_WhenNoSets_CreatesDefaultSetAndSavesOnExit()
    {
        // Arrange
        _mockStorage.Setup(s => s.Load()).Returns(new List<WordSet>());

        // Simulate user input:
        // Navigate to "Save and Exit" (6th option)
        // Press Down 5 times and Enter
        for (int i = 0; i < 5; i++) _console.Input.PushKey(ConsoleKey.DownArrow);
        _console.Input.PushKey(ConsoleKey.Enter);

        // Act
        _app.Run();

        // Assert
        _mockStorage.Verify(s => s.Load(), Times.Once);
        _mockStorage.Verify(s => s.Save(It.Is<List<WordSet>>(sets =>
            sets.Count == 1 && sets[0].Name == "Angielski - Ogólny")), Times.Once);
    }

    [Fact]
    public void Run_AddWord_AddsWordToCurrentSet()
    {
        // Arrange
        var set = new WordSet { Name = "TestSet" };
        _mockStorage.Setup(s => s.Load()).Returns(new List<WordSet> { set });

        // 1. Select "Add word" (2nd option)
        _console.Input.PushKey(ConsoleKey.DownArrow);
        _console.Input.PushKey(ConsoleKey.Enter);

        // 2. Enter Native
        _console.Input.PushText("Cat");
        _console.Input.PushKey(ConsoleKey.Enter);

        // 3. Enter Foreign
        _console.Input.PushText("Kot");
        _console.Input.PushKey(ConsoleKey.Enter);

        // 4. Enter Pronunciation
        _console.Input.PushText("Kat");
        _console.Input.PushKey(ConsoleKey.Enter);

        // 5. Enter Example
        _console.Input.PushText("It is a cat");
        _console.Input.PushKey(ConsoleKey.Enter);

        // 6. Select "Save and Exit" (6th option)
        for (int i = 0; i < 5; i++) _console.Input.PushKey(ConsoleKey.DownArrow);
        _console.Input.PushKey(ConsoleKey.Enter);

        // Act
        _app.Run();

        // Assert
        _mockStorage.Verify(s => s.Save(It.Is<List<WordSet>>(sets =>
            sets.First().Flashcards.Count == 1 &&
            sets.First().Flashcards.First().WordNative == "Cat"
        )), Times.Once);
    }
}