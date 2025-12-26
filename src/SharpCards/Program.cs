using System.Text;
using System.Text.Json;
using Spectre.Console;

namespace SharpCards;

class Program
{
    // Teraz przechowujemy listę zestawów, a nie same słówka
    private static List<WordSet> _allSets = new List<WordSet>();
    // Aktualnie wybrany zestaw, na którym pracujemy
    private static WordSet? _currentSet; 

    private static string _dbPath = "database.json";

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        LoadData();

        // Jeśli nie ma żadnych zestawów, stwórz domyślny
        if (_allSets.Count == 0)
        {
            _allSets.Add(new WordSet { Name = "Angielski - Ogólny" });
        }

        // Domyślnie wybierz pierwszy zestaw
        _currentSet = _allSets.First();

        while (true)
        {
            Console.Clear();
            AnsiConsole.Write(new FigletText("Sharp Cards").Color(Color.Teal));

            // Pasek statusu z informacją o wybranym zestawie
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
                case "💾 Zapisz i Wyjdź": SaveData(); return;
            }
        }
    }

    static void ManageSets()
    {
        while (true)
        {
            Console.Clear();
            AnsiConsole.MarkupLine("[bold blue]Zarządzanie Zestawami[/]");

            // Tworzymy listę opcji wyboru zestawu
            var choices = _allSets.Select(s => s.Name).ToList();
            choices.Add("[green]+ Stwórz nowy zestaw[/]");
            choices.Add("[red]« Powrót[/]");

            var selectedName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Wybierz zestaw (Obecny: [cyan]{_currentSet.Name}[/])")
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

    static void StudyLeitnerMode()
    {
        var sessionWords = _currentSet.Flashcards.Where(x => x.IsReadyToReview()).ToList();

        if (sessionWords.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]W tym zestawie wszystko powtórzone! 🎉[/]");
            WaitForInput();
            return;
        }

        foreach (var card in sessionWords)
        {
            var panelContent = new StringBuilder();
            panelContent.AppendLine($"[bold font=20]{card.WordNative}[/]");
            panelContent.AppendLine($"\n[grey]Pudełko: {card.Box}/5[/]");

            AnsiConsole.Write(new Panel(panelContent.ToString()).Header("Przetłumacz").Border(BoxBorder.Rounded));

            var answer = AnsiConsole.Ask<string>("Tłumaczenie: ");

            if (answer.Trim().Equals(card.WordForeign, StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine("[green bold]DOBRZE![/]");
                if (card.Box < 5) card.Box++;
            }
            else
            {
                AnsiConsole.MarkupLine($"[red bold]ŹLE![/] Poprawna: [yellow]{card.WordForeign}[/]");
                card.Box = 1;
            }

            card.LastReviewDate = DateTime.Now;
            AnsiConsole.WriteLine();
            Thread.Sleep(1500);
            Console.Clear();
        }
        SaveData();
    }

    static void AddWord()
    {
        AnsiConsole.MarkupLine($"[blue]Dodawanie do zestawu: [bold]{_currentSet.Name}[/][/]");
        var native = AnsiConsole.Ask<string>("Słowo [green]Native[/]:");
        var foreign = AnsiConsole.Ask<string>("Tłumaczenie [yellow]Foreign[/]:");

        _currentSet.Flashcards.Add(new Flashcard { WordNative = native, WordForeign = foreign });
        AnsiConsole.MarkupLine("[green]Dodano![/]");
        Thread.Sleep(800);
    }

    static void ImportCsv()
    {
        AnsiConsole.MarkupLine($"[yellow]Import do zestawu: {_currentSet.Name}[/]");
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
                        WordForeign = parts[1].Trim()
                    });
                }
            }
            AnsiConsole.MarkupLine("[green]Zaimportowano![/]");
            SaveData(); // Zapisz od razu po imporcie
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Brak pliku[/]");
        }
        WaitForInput();
    }

    static void ShowAllWords()
    {
        var table = new Table();
        table.Title($"Zestaw: [cyan]{_currentSet.Name}[/]");
        table.AddColumn("Native");
        table.AddColumn("Foreign");
        table.AddColumn("Box");

        foreach (var item in _currentSet.Flashcards)
        {
            string color = item.Box switch { 5 => "green", 1 => "red", _ => "yellow" };
            table.AddRow(item.WordNative, item.WordForeign, $"[{color}]{item.Box}[/]");
        }

        AnsiConsole.Write(table);
        WaitForInput();
    }

    static void SaveData()
    {
        // Zapisujemy całą listę zestawów (_allSets)
        string jsonString = JsonSerializer.Serialize(_allSets, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_dbPath, jsonString);
    }

    static void LoadData()
    {
        if (File.Exists(_dbPath))
        {
            try
            {
                string jsonString = File.ReadAllText(_dbPath);
                _allSets = JsonSerializer.Deserialize<List<WordSet>>(jsonString) ?? new List<WordSet>();
            }
            catch
            {
                // Jeśli stary format pliku powoduje błąd, startujemy z pustą listą
                _allSets = new List<WordSet>();
            }
        }
    }

    static void WaitForInput()
    {
        AnsiConsole.MarkupLine("\nNaciśnij dowolny klawisz...");
        Console.ReadKey();
    }
}