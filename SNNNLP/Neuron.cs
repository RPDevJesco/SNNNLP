namespace SNNNLP
{
    public class Neuron
    {
        public string Word { get; private set; }
        public double MembranePotential { get; set; }
        public double Threshold { get; private set; }
        public double Decay { get; private set; }
        public double SpikeValue { get; private set; }
        public double RefractoryPeriod { get; private set; }
        public double LastSpikeTime { get; private set; }

        public Dictionary<Neuron, double> Synapses { get; private set; }

        public event Action<Neuron> Spike;

        public Neuron(double threshold, double decay, double spikeValue, double refractoryPeriod, string word)
        {
            Threshold = threshold;
            Decay = decay;
            SpikeValue = spikeValue;
            RefractoryPeriod = refractoryPeriod;
            MembranePotential = 0.0;
            Synapses = new Dictionary<Neuron, double>();
            LastSpikeTime = -1.0;
            Word = word;
        }

        public void AddSynapse(Neuron neuron, double weight)
        {
            Synapses[neuron] = weight;
        }

        public void ReceiveInput(double input)
        {
            if (LastSpikeTime < 0 || (DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond) - LastSpikeTime >= RefractoryPeriod)
            {
                MembranePotential += input;
                if (MembranePotential >= Threshold)
                {
                    Fire();
                }
            }
        }

        private void Fire()
        {
            MembranePotential = 0.0; // Reset after firing
            Spike?.Invoke(this);
            LastSpikeTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;

            foreach (var synapse in Synapses)
            {
                synapse.Key.ReceiveInput(synapse.Value * SpikeValue);
            }
        }

        public void Update()
        {
            MembranePotential *= Decay;
        }

        public void ApplySTDP(Neuron presynapticNeuron, double potentiationRate, double depressionRate, double deltaTime)
        {
            if (LastSpikeTime >= 0 && presynapticNeuron.LastSpikeTime >= 0)
            {
                double timeDifference = LastSpikeTime - presynapticNeuron.LastSpikeTime;

                if (timeDifference > 0)
                {
                    // Potentiation
                    if (Synapses.ContainsKey(presynapticNeuron))
                    {
                        double weight = Synapses[presynapticNeuron];
                        weight = Math.Min(weight + potentiationRate * Math.Exp(-timeDifference / deltaTime), 1.0);
                        Synapses[presynapticNeuron] = weight;
                    }
                }
                else if (timeDifference < 0)
                {
                    // Depression
                    if (Synapses.ContainsKey(presynapticNeuron))
                    {
                        double weight = Synapses[presynapticNeuron];
                        weight = Math.Max(weight - depressionRate * Math.Exp(timeDifference / deltaTime), 0.1); // Set minimum weight threshold to 0.1
                        Synapses[presynapticNeuron] = weight;
                    }
                }
            }
        }
    }
}