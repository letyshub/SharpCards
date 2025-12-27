namespace SharpCards;

public interface IStorage
{
    void Save(List<WordSet> sets);
    List<WordSet> Load();
}