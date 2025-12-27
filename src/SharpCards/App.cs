using System.Text;
using Spectre.Console;

namespace SharpCards;

public class App
{
    private List<WordSet> _allSets = new List<WordSet>();
    private WordSet? _currentSet;
    private readonly IStorage _storage;

    public App(IStorage storage)
    {
        _storage = storage;
    }

    public void Run()
    {
        _allSets = _storage.Load();

        if (_allSets.Count == 0)
        {
            _allSets.Add(new WordSet { Name = "Angielski - Ogólny" });
        }

        _currentSet = _allSets.First();

        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Sharp Cards").Color(Color.Teal));
            AnsiConsole.MarkupLine($"[grey]Aktywny zestaw:[/] [bold cyan]{_currentSet.Name}[/]");

            var readyCount = _currentSet.Flashcards.Count(x => x.IsReadyToReview());
            AnsiConsole.MarkupLine($"[grey]Słówka w zestawie:[/] [green]{_currentSet.Flashcards.Count}[/] | [grey]Do powtórki:[/] [yellow]{readyCount}[/]");
            AnsiConsole.WriteLine();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Co chcesz zrobić?")
                    .PageSize(10)
                    .AddChoices(new[] {
                            "🧠 Nauka (System Leitnera)",
                            "➕ Dodaj słówko",
                            "📚 Zmień / Stwórz zestaw",
                            "📂 Importuj z CSV",
                            "📋 Lista słówek",
                            "💾 Zapisz i Wyjdź"
                    }));

            switch (choice)
            {
                case "🧠 Nauka (System Leitnera)": StudyLeitnerMode(); break;
                case "➕ Dodaj słówko": AddWord(); break;
                case "📚 Zmień / Stwórz zestaw": ManageSets(); break;
                case "📂 Importuj z CSV": ImportCsv(); break;
                case "📋 Lista słówek": ShowAllWords(); break;
                case "💾 Zapisz i Wyjdź": _storage.Save(_allSets); return;
            }
        }
    }

    void ManageSets()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold blue]Zarządzanie Zestawami[/]");

            // Tworzymy listę opcji wyboru zestawu
            var choices = _allSets.Select(s => s.Name).ToList();
            choices.Add("[green]+ Stwórz nowy zestaw[/]");
            choices.Add("[red]« Powrót[/]");

            var selectedName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Wybierz zestaw (Obecny: [cyan]{_currentSet?.Name}[/])")
                    .AddChoices(choices));

            if (selectedName == "[red]« Powrót[/]")
            {
                break;
            }
            else if (selectedName == "[green]+ Stwórz nowy zestaw[/]")
            {
                var newName = AnsiConsole.Ask<string>("Podaj nazwę nowego zestawu:");
                if (_allSets.Any(s => s.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                {
                    AnsiConsole.MarkupLine("[red]Taki zestaw już istnieje![/]");
                    Thread.Sleep(1500);
                }
                else
                {
                    var newSet = new WordSet { Name = newName };
                    _allSets.Add(newSet);
                    _currentSet = newSet; // Przełącz na nowy
                    AnsiConsole.MarkupLine($"[green]Stworzono zestaw '{newName}' i przełączono na niego.[/]");
                    Thread.Sleep(1000);
                    break; // Wróć do menu głównego
                }
            }
            else
            {
                // Wybrano istniejący zestaw
                _currentSet = _allSets.First(s => s.Name == selectedName);
                AnsiConsole.MarkupLine($"[green]Przełączono na zestaw: {selectedName}[/]");
                Thread.Sleep(800);
                break;
            }
        }
    }

    void StudyLeitnerMode()
    {
        var sessionWords = _currentSet?.Flashcards.Where(x => x.IsReadyToReview()).ToList();

        if (sessionWords == null || !sessionWords.Any())
        {
            AnsiConsole.MarkupLine("[green]W tym zestawie wszystko powtórzone! 🎉[/]");
            WaitForInput();
            return;
        }

        foreach (var card in sessionWords)
        {
            var panelContent = new StringBuilder();
            panelContent.AppendLine($"[bold font=20]{card.WordNative}[/]");
            panelContent.AppendLine($"\n[grey]Pudełko: {(int)card.Box}/5[/]");

            AnsiConsole.Write(new Panel(panelContent.ToString()).Header("Przetłumacz").Border(BoxBorder.Rounded));

            var answer = AnsiConsole.Ask<string>("Tłumaczenie: ");

            if (answer.Trim().Equals(card.WordForeign, StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine("[green bold]DOBRZE![/]");
                if (card.Box < LeitnerBox.Month) card.Box++;
            }
            else
            {
                AnsiConsole.MarkupLine($"[red bold]ŹLE![/] Poprawna: [yellow]{card.WordForeign}[/]");
                card.Box = LeitnerBox.Everyday;
            }

            if (!string.IsNullOrWhiteSpace(card.Pronunciation))
                AnsiConsole.MarkupLine($"[grey]Wymowa:[/] {card.Pronunciation}");
            if (!string.IsNullOrWhiteSpace(card.Example))
                AnsiConsole.MarkupLine($"[grey]Przykład:[/] {card.Example}");

            card.LastReviewDate = DateTime.Now;
            AnsiConsole.WriteLine();
            Thread.Sleep(1500);
            AnsiConsole.Clear();
        }
        _storage.Save(_allSets);
    }

    void AddWord()
    {
        if (_currentSet == null)
        {
            AnsiConsole.MarkupLine("[red]Nie wybrano zestawu![/]");
            WaitForInput();
            return;
        }

        AnsiConsole.MarkupLine($"[blue]Dodawanie do zestawu: [bold]{_currentSet?.Name}[/][/]");
        var native = AnsiConsole.Ask<string>("Słowo [green]Native[/]:");
        var foreign = AnsiConsole.Ask<string>("Tłumaczenie [yellow]Foreign[/]:");
        var pronunciation = AnsiConsole.Prompt(new TextPrompt<string>("Wymowa [grey](opcjonalnie)[/]:").AllowEmpty());
        var example = AnsiConsole.Prompt(new TextPrompt<string>("Przykład [grey](opcjonalnie)[/]:").AllowEmpty());

        _currentSet.Flashcards.Add(new Flashcard { WordNative = native, WordForeign = foreign, Pronunciation = pronunciation, Example = example });
        AnsiConsole.MarkupLine("[green]Dodano![/]");
        Thread.Sleep(800);
    }

    void ImportCsv()
    {
        AnsiConsole.MarkupLine($"[yellow]Import do zestawu: {_currentSet?.Name}[/]");
        var path = AnsiConsole.Ask<string>("Podaj ścieżkę do CSV:");

        if (File.Exists(path))
        {
            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var parts = line.Split(';');
                if (parts.Length >= 2)
                {
                    _currentSet.Flashcards.Add(new Flashcard
                    {
                        WordNative = parts[0].Trim(),
                        WordForeign = parts[1].Trim(),
                        Pronunciation = parts.Length > 2 ? parts[2].Trim() : string.Empty,
                        Example = parts.Length > 3 ? parts[3].Trim() : string.Empty
                    });
                }
            }
            AnsiConsole.MarkupLine("[green]Zaimportowano![/]");
            _storage.Save(_allSets);
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Brak pliku[/]");
        }
        WaitForInput();
    }

    void ShowAllWords()
    {
        var table = new Table();
        table.Title($"Zestaw: [cyan]{_currentSet.Name}[/]");
        table.AddColumn("Native");
        table.AddColumn("Foreign");
        table.AddColumn("Pronunciation");
        table.AddColumn("Example");
        table.AddColumn("Box");

        foreach (var item in _currentSet.Flashcards)
        {
            string color = item.Box switch { LeitnerBox.Month => "green", LeitnerBox.Everyday => "red", _ => "yellow" };
            table.AddRow(item.WordNative, item.WordForeign, item.Pronunciation, item.Example, $"[{color}]{(int)item.Box}[/]");
        }

        AnsiConsole.Write(table);
        WaitForInput();
    }

    void WaitForInput()
    {
        AnsiConsole.MarkupLine("\nNaciśnij dowolny klawisz...");
        AnsiConsole.Console.Input.ReadKey(true);
    }
}