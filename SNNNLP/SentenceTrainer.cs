using System.Text.RegularExpressions;

namespace SNNNLP
{
    class SentenceTrainer
    {
        private readonly string[] _trainingFiles;
        private readonly SpikingNeuralNetwork _snn;
        private readonly Dictionary<string, Neuron> _wordNeurons;
        private readonly Random _random;

        public SentenceTrainer(string[] trainingFiles, SpikingNeuralNetwork snn)
        {
            _trainingFiles = trainingFiles;
            _snn = snn;
            _wordNeurons = new Dictionary<string, Neuron>();
            _random = new Random();
        }

        public void Train()
        {
            foreach (var file in _trainingFiles)
            {
                var text = File.ReadAllText(file);
                var sentences = SplitIntoSentences(text);

                foreach (var sentence in sentences)
                {
                    var words = Tokenize(sentence);
                    UpdateSNN(words);
                }
            }
        }

        private string[] SplitIntoSentences(string text)
        {
            // Use regular expressions to split the text into sentences
            var sentences = Regex.Split(text, @"(?<=[.!?])\s+");

            // Remove any empty sentences
            sentences = sentences.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            return sentences;
        }

        private string[] Tokenize(string sentence)
        {
            // Tokenize the sentence into words
            var words = Regex.Split(sentence, @"\W+")
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Select(w => w.ToLower())
                .ToArray();

            return words;
        }

        private void UpdateSNN(string[] words)
        {
            Neuron prevNeuron = null;

            foreach (var word in words)
            {
                try
                {
                    if (!_wordNeurons.ContainsKey(word))
                    {
                        var neuron = new Neuron(1.0, 0.99, 1.0, 100.0, word);
                        _wordNeurons[word] = neuron;
                        _snn.AddNeuron(neuron);
                    }

                    var wordNeuron = _wordNeurons[word];
                    wordNeuron.ReceiveInput(1.0);

                    if (prevNeuron != null && !prevNeuron.Synapses.ContainsKey(wordNeuron))
                    {
                        prevNeuron.AddSynapse(wordNeuron, _random.NextDouble());
                    }

                    prevNeuron = wordNeuron;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing word '{word}': {ex.Message}");
                }
            }
        }

        public string GenerateSentence(int maxLength)
        {
            var generatedWords = new List<string>();
            var currentWord = _wordNeurons.Keys.ElementAt(_random.Next(_wordNeurons.Count));
            generatedWords.Add(currentWord);

            while (generatedWords.Count < maxLength)
            {
                if (!_wordNeurons.ContainsKey(currentWord))
                {
                    break;
                }

                var wordNeuron = _wordNeurons[currentWord];
                if (wordNeuron.Synapses.Count == 0)
                {
                    break;
                }

                var nextWordNeuron = wordNeuron.Synapses.OrderByDescending(s => s.Value).First().Key;
                currentWord = _wordNeurons.FirstOrDefault(x => x.Value == nextWordNeuron).Key;
                if (currentWord != null)
                {
                    generatedWords.Add(currentWord);
                }
            }

            return string.Join(" ", generatedWords);
        }
    }
}