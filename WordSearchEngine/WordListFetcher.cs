using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace WordSearchEngine
{
    public class WordListFetcher
    {
        private readonly HttpClient _http = new();

        public async Task<List<string>> GetPhrasesForTopicAsync(string topic, int maxFetch = 100, int finalCount = 30)
        {
            string url = $"https://api.datamuse.com/words?ml={topic}&max={maxFetch}";
            string response = await _http.GetStringAsync(url);
            var results = JsonSerializer.Deserialize<List<WordResult>>(response);

            var cleaned = results
                .Select(w => FormatPhrase(w.word))
                .Where(w => IsCleanPhrase(w))
                .Distinct()                
                .ToList();

            return cleaned
                .OrderBy(_ => Guid.NewGuid())
                .Take(finalCount)
                .ToList();
        }

        private static string FormatPhrase(string phrase) =>
            CultureInfo.CurrentCulture.TextInfo.ToTitleCase(phrase.ToLower());

        private static bool IsCleanPhrase(string w)
        {
            int wordCount = w.Count(c => c == ' ') + 1;
            return wordCount <= 3 && w.Length <= 20 && !w.Contains('-');
        }

        public class WordResult
        {
            public string word { get; set; }    
        }
    }
}
