using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wordle
{
    public class WordListService
    {
        private readonly string _validPath = "Resources/wordlists/valid_words.txt";
        private readonly string _answerPath = "Resources/wordlists/answer_words.txt";
        private readonly string _usedPath = "Resources/wordlists/used_words.txt";

        private List<string> _validWords;
        private List<string> _answerWords;
        private HashSet<string> _usedAnswers;

        public WordListService()
        {
            _validWords = File.ReadAllLines(_validPath).Select(w => w.ToLower()).ToList();
            _answerWords = File.ReadAllLines(_answerPath).Select(w  => w.ToLower()).ToList();

            if (!File.Exists(_usedPath))
                File.WriteAllText(_usedPath, "");

            _usedAnswers = File.ReadAllLines(_usedPath).Select(w => w.ToLower()).ToHashSet();
        }

        public bool IsValidWord(string word) => _validWords.Contains(word.ToLower());

        public string GetRandomUnusedAnswerWord()
        {
            var unused = _answerWords.Except(_usedAnswers).ToList();
            if(unused.Count == 0)
            {
                ResetUsedWords();
            }

            var selected = unused[new Random().Next(unused.Count)];
            _usedAnswers.Add(selected);
            File.AppendAllText(_usedPath, selected + Environment.NewLine);
            return selected;
        }

        public void ResetUsedWords()
        {
            File.WriteAllText(_usedPath, "");
            _usedAnswers.Clear();
        }
    }
}
