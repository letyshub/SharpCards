namespace SharpCards;

public class Flashcard
{
    public string WordNative { get; set; } = string.Empty;
    public string WordForeign { get; set; } = string.Empty;
    public string Pronunciation { get; set; } = string.Empty;
    public string Example { get; set; } = string.Empty;

    // System Leitnera: 1 (codziennie), 2 (co 3 dni), 3 (tydzień), 4 (2 tyg), 5 (miesiąc)
    public LeitnerBox Box { get; set; } = LeitnerBox.Everyday;
    public DateTime LastReviewDate { get; set; } = DateTime.MinValue;

    public bool IsReadyToReview()
    {
        int daysToWait = Box switch
        {
            LeitnerBox.Everyday => 0,  // Codziennie
            LeitnerBox.ThreeDays => 3,  // Co 3 dni
            LeitnerBox.Week => 7,  // Tydzień
            LeitnerBox.TwoWeeks => 14, // 2 tygodnie
            LeitnerBox.Month => 30, // Miesiąc
            _ => 1
        };
        return (DateTime.Now - LastReviewDate).TotalDays >= daysToWait;
    }
}