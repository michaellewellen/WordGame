using System.Net.Http;
using System.Text.Json;
class Program
{
    static async Task Main()
    {
        using var client = new HttpClient();
        var allWords = new HashSet<string>();
        char[] letters = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

        foreach (var letter in letters)
        {
            string url = $"https://api.datamuse.com/words?sp={letter}*&max=1000";
            string response = await client.GetStringAsync(url);
            var results = JsonSerializer.Deserialize<List<WordEntry>>(response);

            foreach (var word in results!)
            {
                if (!string.IsNullOrEmpty(word.Word) && word.Word.All(char.IsLetter)) // no hypens, digits, etc.
                    allWords.Add(word.Word.ToLower());
            }
            Console.WriteLine($"Fetched {letter}...");
            await Task.Delay(1000); /// be nice to the API
        }

        File.WriteAllLines($"valid_words.txt", allWords.OrderBy(w => w));
        Console.WriteLine("Done!");
    }

    public class WordEntry
    {
        public string Word { get; set; }
    }
}