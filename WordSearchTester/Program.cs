using WordSearchEngine;

namespace WordSearchTester
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var fetcher = new WordSearchEngine.WordListFetcher();
            var words = await fetcher.GetPhrasesForTopicAsync("outer space");

            var generator = new WordSearchGenerator();
            generator.Generate(words);
            generator.PrintToConsole();  // <- method you added earlier
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}