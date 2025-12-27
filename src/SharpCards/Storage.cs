using System.Text.Json;

namespace SharpCards;

public class Storage : IStorage
{
    private readonly string _filePath;

    public Storage(string filePath = "database.json")
    {
        _filePath = filePath;
    }

    public void Save(List<WordSet> sets)
    {
        string jsonString = JsonSerializer.Serialize(sets, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, jsonString);
    }

    public List<WordSet> Load()
    {
        if (File.Exists(_filePath))
        {
            try
            {
                string jsonString = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<WordSet>>(jsonString) ?? new List<WordSet>();
            }
            catch
            {
                return new List<WordSet>();
            }
        }
        return new List<WordSet>();
    }
}