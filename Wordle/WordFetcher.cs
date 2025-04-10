using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Wordle
{
    public class WordFetcher
    {
        private readonly HttpClient _http = new();

        public async Task<(List<string> allWords, List<string> commonWords)> FetchFiveLetterWordsAsync()
        {
            var allWords = new HashSet<string>();
            var commonWords = new List<string>();

            int batchSize = 1000;
            for (char start = 'a'; start <= 'z'; start ++)
            {
                var response = await _http.GetStringAsync($"https://api.datamuse.com/words?sp={start}?????&max={batchSize}&md=f");
                var words = JsonSerializer.Deserialize<List<WordResult>>(response);

                foreach (var word in words)
                {
                    if (word.word.Length == 5 && word.word.All(char.IsLetter))
                    {
                        allWords.Add(word.word.ToLower());

                        if (word.tags !=null && word.tags.Any(testc => testc.StartsWith("f:")))
                        {
                            var freq = double.Parse(word.tags.First(t => t.StartsWith("f:")).Substring(2));
                            if (freq >= 10.0)
                                commonWords.Add(word.word.ToLower());
                        }
                    }
                }
            }
            return (allWords.ToList(), commonWords.Distinct().ToList());
        }
    }
    public class WordResult
    {
        public string word {  get; set; }   
        public List<string>  tags { get; set; }
    }
}
