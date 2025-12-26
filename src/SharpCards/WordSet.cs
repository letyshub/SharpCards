using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharpCards;

public class WordSet
{
    public string Name { get; set; } = "Domyślny";
    public List<Flashcard> Flashcards { get; set; } = new();
}