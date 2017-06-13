using System;
using System.Windows;


namespace PoissonGen
{
    /// <summary>
    /// Uniform distribution numbers from 0 to 1.
    /// </summary>
    public class GenerateRandom
    {
        /// <summary>
        /// Creates an array of numbers in Poisson's distribution using previously generated numbers by class GenerateRandom.
        /// </summary>
        public class PoissonDistribution
        {
            private double[] poissonArray;
            private GenerateRandom generateRandom;

            public double this[int index]
            {
                get { return poissonArray[index]; }
                set { poissonArray[index] = value; }
            }

            // https://brain.fuw.edu.pl/edu/index.php/WnioskowanieStatystyczne/Zmienne_losowe_i_generatory_liczb_pseudolosowych#Rozk.C5.82ad_Poissona
            private void generatePoisson(GenerateRandom generateRandom, int lambda, int NT) // lambda - średnia ilość zdarzeń
            {                                                                               // NT - liczba losowanych liczb z rozkładu jednostajnego
                int probability = lambda / NT;                                              // NT < N, lambda <= NT

                for (int i = 0; i < generateRandom.randomIntArray.Length; i++)
                {
                    generateRandom.generateRandom();
                    int poissonValue = 0;

                    for (int j = 0; j < NT; j++) 
                        {
                            if (generateRandom.randomIntArray[i] <= probability)
                            {
                                poissonValue++;
                            }
                    }
                    poissonArray[i] = poissonValue;
                }
            }

            public PoissonDistribution(GenerateRandom generateRandom, int NT, int mi = 0)
            {
                generateRandom = new GenerateRandom(generateRandom.randomIntArray.Length);
                generatePoisson(generateRandom, mi, NT);
            }
        }


        private double[] randomIntArray;

        public double this[int index]
        {
            get { return randomIntArray[index]; }
            set { randomIntArray[index] = value; }
        }

        private void generateRandom()
        {
            int highestInt = (int)Math.Pow(2, 21);
            double modulo = Math.Pow(10, 6);
            double div = Math.Pow(10, 6);
            double auxiliary;
            Random number = new Random();

            for (int i = 0; i < randomIntArray.Length; i++)
            {
                auxiliary = ((number.Next(0, highestInt) % modulo) / div);
                randomIntArray[i] = auxiliary;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arraySize"></param>
        public GenerateRandom(int arraySize)
        {
            this.randomIntArray = new double[arraySize];
            this.generateRandom();
        }
    }   

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
