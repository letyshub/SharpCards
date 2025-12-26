namespace SharpCards;

public class Flashcard
{
    public string WordNative { get; set; } = string.Empty;
    public string WordForeign { get; set; } = string.Empty;
    
    // System Leitnera: 1 (codziennie), 2 (co 3 dni), 3 (tydzień), 4 (2 tyg), 5 (miesiąc)
    public int Box { get; set; } = 1; 
    public DateTime LastReviewDate { get; set; } = DateTime.MinValue;

    // Czy słówko jest gotowe do powtórki na podstawie daty i pudełka?
    public bool IsReadyToReview()
    {
        int daysToWait = Box switch
        {
            1 => 0,  // Codziennie
            2 => 3,  // Co 3 dni
            3 => 7,  // Tydzień
            4 => 14, // 2 tygodnie
            5 => 30, // Miesiąc
            _ => 1
        };
        return (DateTime.Now - LastReviewDate).TotalDays >= daysToWait;
    }
}