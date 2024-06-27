namespace SNNNLP
{
    public class SpikingNeuralNetwork
    {
        public List<Neuron> Neurons { get; private set; }
        public double PotentiationRate { get; private set; }
        public double DepressionRate { get; private set; }
        public double DeltaTime { get; private set; }

        private Dictionary<string, Neuron> wordNeuronMap;

        public SpikingNeuralNetwork(double potentiationRate, double depressionRate, double deltaTime)
        {
            Neurons = new List<Neuron>();
            PotentiationRate = potentiationRate;
            DepressionRate = depressionRate;
            DeltaTime = deltaTime;
            wordNeuronMap = new Dictionary<string, Neuron>();
        }

        public void AddNeuron(Neuron neuron)
        {
            Neurons.Add(neuron);
            wordNeuronMap[neuron.Word] = neuron;
        }

        public bool TryGetNeuron(string word, out Neuron neuron)
        {
            return wordNeuronMap.TryGetValue(word, out neuron);
        }

        public void Update()
        {
            foreach (var neuron in Neurons)
            {
                neuron.Update();
            }
        }

        public void Input(double[] inputs)
        {
            for (int i = 0; i < inputs.Length; i++)
            {
                Neurons[i].ReceiveInput(inputs[i]);
            }
        }

        public void ApplySTDP()
        {
            foreach (var neuron in Neurons)
            {
                foreach (var synapse in neuron.Synapses)
                {
                    neuron.ApplySTDP(synapse.Key, PotentiationRate, DepressionRate, DeltaTime);
                }
            }
        }
    }
}