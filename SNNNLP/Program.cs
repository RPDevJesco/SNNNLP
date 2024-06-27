using System.Text.Json;

namespace SNNNLP
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            // Download and parse JSON data
            var httpClient = new HttpClient();
            var jsonUrls = new[] {
                "https://raw.githubusercontent.com/Mirza-Glitch/simple-english-dictionary-api/main/meaningsJson/meanings1.json",
                "https://raw.githubusercontent.com/Mirza-Glitch/simple-english-dictionary-api/main/meaningsJson/meanings2.json"
            };

            var dictionaryData = new Dictionary<string, WordData>();

            foreach (var url in jsonUrls)
            {
                var json = await httpClient.GetStringAsync(url);
                var data = JsonSerializer.Deserialize<Data>(json);

                if (data?.Words != null)
                {
                    foreach (var entry in data.Words)
                    {
                        dictionaryData[entry.Key] = entry.Value;
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to deserialize data from URL: {url}");
                }
            }

            // Preprocess dictionary data
            var wordDictionary = PreprocessDictionaryData(dictionaryData);

            // Train the sentence construction model
            var trainingFiles = new[]
            {
                "books/book.txt",
                "books/pg11.txt",
                "books/pg55.txt",
                "books/pg100.txt",
                "books/pg11.txt",
                "books/pg514.txt",
                "books/pg844.txt",
                "books/pg1342.txt",
                "books/pg1513.txt",
                "books/pg1661.txt",
                "books/pg1727.txt",
                "books/pg2591.txt",
                "books/pg2701.txt",
                "books/pg8800.txt",
                "books/pg27558.txt",
                "books/pg64317.txt",
                "books/pg67098.txt",
                "books/pg73748.txt",
                "books/pg73753.txt",
                "books/pg73754.txt",
                "books/pg73755.txt",
                "books/pg73757.txt",
                "books/pg73758.txt",
                "books/pg73759.txt",
                "books/pg73760.txt",
                "books/pg73761.txt"
            };

            // Create the SNN
            var snn = new SpikingNeuralNetwork(0.05, 0.05, 20.0); // Fine-tuned values for potentiation, depression, and delta time

            var sentenceTrainer = new SentenceTrainer(trainingFiles, snn);
            sentenceTrainer.Train();
            var sentence = sentenceTrainer.GenerateSentence(100);

            // Implement sentence construction algorithm
            var naiveSentence = ConstructSentence(wordDictionary);

            // Implement sentence construction algorithm with SNN
            var snnSentence = ConstructSNNSentence(wordDictionary, snn, 100);

            // Perform sentiment analysis
            var sentimentScore = AnalyzeSentiment(sentence, wordDictionary);

            // Process sentences using SNN
            ProcessSentencesUsingSNN(sentence, naiveSentence, wordDictionary, snn);

            // Display results
            //Console.WriteLine("Generated Sentence: " + sentence);
            Console.WriteLine("Generated SNN Sentence: " + snnSentence);
            Console.WriteLine("Default Generated Sentence: " + naiveSentence);
            Console.WriteLine("Sentiment Score: " + sentimentScore);
        }

        static Dictionary<string, List<WordData>> PreprocessDictionaryData(Dictionary<string, WordData> dictionaryData)
        {
            var wordDictionary = new Dictionary<string, List<WordData>>();

            foreach (var entry in dictionaryData)
            {
                var word = entry.Key.ToLower();

                if (!wordDictionary.ContainsKey(word))
                {
                    wordDictionary[word] = new List<WordData>();
                }

                wordDictionary[word].Add(entry.Value);
            }

            return wordDictionary;
        }

        static string ConstructSentence(Dictionary<string, List<WordData>> wordDictionary)
        {
            var random = new Random();
            var sentence = string.Empty;

            // Define the sentence structure
            var sentenceStructure = new[] { "Noun", "Verb", "Adjective", "Noun" };

            foreach (var partOfSpeech in sentenceStructure)
            {
                // Get a random word with the specified part of speech
                var wordsWithPartOfSpeech = wordDictionary.Values
                    .SelectMany(entries => entries)
                    .SelectMany(entry => entry.Meanings)
                    .Where(meaning => meaning.PartsOfSpeech.Equals(partOfSpeech, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (wordsWithPartOfSpeech.Count > 0)
                {
                    var randomIndex = random.Next(0, wordsWithPartOfSpeech.Count);
                    var meaning = wordsWithPartOfSpeech[randomIndex];
                    var word = meaning.Definition ?? string.Empty;

                    // Add the word to the sentence
                    sentence += word + " ";
                }
            }

            // Capitalize the first letter and add a period at the end
            sentence = sentence.Trim();
            if (!string.IsNullOrEmpty(sentence))
            {
                sentence = char.ToUpper(sentence[0]) + sentence.Substring(1) + ".";
            }

            return sentence;
        }

        static string ConstructSNNSentence(Dictionary<string, List<WordData>> wordDictionary, SpikingNeuralNetwork snn, int maxLength = 10)
        {
            var random = new Random();
            var sentence = new List<string>();

            // Define the sentence structure
            var sentenceStructure = new[] { "Noun", "Verb", "Adjective", "Noun" };

            // Dictionary to map part of speech to neurons
            var posNeurons = new Dictionary<string, List<Neuron>>();

            foreach (var partOfSpeech in sentenceStructure)
            {
                posNeurons[partOfSpeech] = new List<Neuron>();

                // Get words with the specified part of speech
                var wordsWithPartOfSpeech = wordDictionary.Values
                    .SelectMany(entries => entries)
                    .SelectMany(entry => entry.Meanings)
                    .Where(meaning => meaning.PartsOfSpeech.Equals(partOfSpeech, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var meaning in wordsWithPartOfSpeech)
                {
                    if (!string.IsNullOrEmpty(meaning.Definition))
                    {
                        if (!snn.TryGetNeuron(meaning.Definition, out var neuron))
                        {
                            neuron = new Neuron(1.0, 0.99, 1.0, 100.0, meaning.Definition);
                            snn.AddNeuron(neuron);
                        }
                        posNeurons[partOfSpeech].Add(neuron);
                    }
                }
            }

            // Generate sentence using the SNN
            foreach (var partOfSpeech in sentenceStructure)
            {
                if (posNeurons[partOfSpeech].Count > 0)
                {
                    var selectedNeuron = posNeurons[partOfSpeech]
                        .OrderByDescending(n => n.MembranePotential)
                        .ThenBy(n => random.Next())
                        .FirstOrDefault();
                    if (selectedNeuron != null)
                    {
                        sentence.Add(selectedNeuron.Word);
                        selectedNeuron.MembranePotential = 0; // Reset the potential to avoid repetition
                    }
                    else
                    {
                        Console.WriteLine($"No neuron selected for part of speech '{partOfSpeech}'");
                    }
                }
                else
                {
                    Console.WriteLine($"No neurons found for part of speech '{partOfSpeech}'");
                }
            }

            // Capitalize the first letter and add a period at the end
            var sentenceStr = string.Join(" ", sentence).Trim();
            if (!string.IsNullOrEmpty(sentenceStr))
            {
                sentenceStr = char.ToUpper(sentenceStr[0]) + sentenceStr.Substring(1) + ".";
            }

            return sentenceStr;
        }

        static double AnalyzeSentiment(string sentence, Dictionary<string, List<WordData>> wordDictionary)
        {
            var words = sentence.Split(' ');
            var sentimentScore = 0.0;
            var sentimentWords = 0;

            foreach (var word in words)
            {
                var lowercaseWord = word.TrimEnd('.');

                if (wordDictionary.TryGetValue(lowercaseWord, out var entries))
                {
                    foreach (var entry in entries)
                    {
                        foreach (var meaning in entry.Meanings)
                        {
                            var definition = meaning.Definition?.ToLower() ?? string.Empty;
                            var score = 0.0;

                            if (definition.Contains("positive"))
                            {
                                score = 1.0;
                            }
                            else if (definition.Contains("negative"))
                            {
                                score = -1.0;
                            }

                            sentimentScore += score;
                            sentimentWords++;
                        }
                    }
                }
            }

            if (sentimentWords > 0)
            {
                return sentimentScore / sentimentWords;
            }
            else
            {
                return 0.0;
            }
        }

        static void ProcessSentencesUsingSNN(string sentence, string naiveSentence, Dictionary<string, List<WordData>> wordDictionary, SpikingNeuralNetwork snn)
        {
            var uniqueWords = sentence.Split(' ').Concat(naiveSentence.Split(' ')).Distinct().ToList();

            // Create input neurons for each word
            var inputNeurons = new Dictionary<string, Neuron>();
            foreach (var word in uniqueWords)
            {
                var neuron = new Neuron(1.0, 0.99, 1.0, 100.0, word); // Example values
                inputNeurons[word] = neuron;
                snn.AddNeuron(neuron);
            }

            // Simulate spikes for sentence
            var words = sentence.Split(' ');
            foreach (var word in words)
            {
                if (inputNeurons.TryGetValue(word, out var neuron))
                {
                    neuron.ReceiveInput(1.0);
                }
            }

            // Simulate spikes for naive sentence
            words = naiveSentence.Split(' ');
            foreach (var word in words)
            {
                if (inputNeurons.TryGetValue(word, out var neuron))
                {
                    neuron.ReceiveInput(1.0);
                }
            }

            snn.ApplySTDP();

            Console.WriteLine("Processing completed using SNN.");
        }
    }
}