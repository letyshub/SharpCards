namespace SharpCards;

class Program
{
    static void Main(string[] args)
    {
        var storage = new Storage();
        var app = new App(storage);
        app.Run();
    }
}