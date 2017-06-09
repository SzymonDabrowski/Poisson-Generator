using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;// Binding
using ZXing;// QR
using System.Drawing;// Bitmap
using Microsoft.Win32;

//zxing.net license:
//http://www.apache.org/licenses/LICENSE-2.0

namespace lab7
{
    public partial class MainWindow : Window
    {
        private OxyPlotModel oxyPlotModel;
        
        public MainWindow() 
        {
            InitializeComponent();
            oxyPlotModel = new OxyPlotModel();
            this.DataContext = oxyPlotModel;
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            //generatePoisson construtor parameters
            //HARD CODED!
            int arraySize = 100000;
            int mi = 500;
            int NT = 10000;

            GeneratePoisson generatePoisson = new GeneratePoisson(arraySize, mi, NT); // hard coded data; change in code only
            var listXdensity = generatePoisson.density; // density array
            double[] listY = new double[generatePoisson.density.Length]; // filling listY with consicutive numbers (starting from 0)
            for (int i = 0; i < generatePoisson.density.Length; i++)
                listY[i] = i;
            double[] listXcumultative = generatePoisson.cumultative; // cumultative array

            #region save and plot

            if (!System.IO.Directory.Exists(@"\drawings"))            
                System.IO.Directory.CreateDirectory(@"\drawings");
              
            System.IO.StreamWriter file = new StreamWriter($@"\drawings\{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} {DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}.txt");
            file.WriteLine("index\t\tdensity\t\tcumultative");
            for (int i = 0; i < listXdensity.Length; i++)
            {
                string density = listXdensity[i].ToString();
                string cumultative = listXcumultative[i].ToString();
                file.WriteLine($"{i}:\t\t{density};\t\t{cumultative};");
            }
            file.Close();

            oxyPlotModel.plot(listY, listXdensity);
            OxyPlot.Wpf.PngExporter.Export(oxyPlotModel.PlotModel, $@"\drawings\density{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} {DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}.png", 1600, 1600, OxyPlot.OxyColors.White, 96);
            #endregion

            //opening second window with cumultative plot
            WindowDistribution newWindow = new WindowDistribution(listY, listXcumultative);
            newWindow.Show();  
        }
        
        private void buttonQR_Click(object sender, RoutedEventArgs e)
        {
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.DefaultExt = "jpg";
                openFileDialog.Filter = "JPEG (*.jpg)|*.jpg|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == true)
                {
                    IBarcodeReader reader = new ZXing.BarcodeReader();
                    string sFileName = openFileDialog.FileName;
                    var barcodeBitmap = (Bitmap)System.Drawing.Image.FromFile(sFileName);
                    var result = reader.Decode(barcodeBitmap);
                    QRPoisson.Text = result.Text.ToString();
                    double lambda = double.Parse(QRPoisson.Text.ToString(), System.Globalization.CultureInfo.InvariantCulture); // res is lambda
                    if (lambda <= 0.0 || lambda > 1.0)
                        lambda = 1;

                    //getting k parameter from textbox kParameter
                    //if no action occured, k == 3
                    int k; //3 by default
                    if (string.IsNullOrEmpty(kParameter.Text) || string.IsNullOrWhiteSpace(kParameter.Text))
                    {
                        kParameter.Text = "3";
                        k = 3;
                    }
                    else
                    {
                        k = int.Parse(kParameter.Text, System.Globalization.CultureInfo.InvariantCulture);
                    }

                    PoissonFromQR.Text = poissonQR(lambda, k).ToString();
                }
            }
        }

        #region functions made for buttonQR_Click
        private static double factorial(int k) // recursive is slower!
        {
            int result = 1;
            if (k == 0 || k == 1)
                return result;
            else
            {
                for (int i = 2; i <= k; i++)
                    result *= i;
            }
            return result;
        }

        private static double poissonQR(double lambda, int k)
        {
            double result = (Math.Pow(lambda,k) * Math.Exp(-lambda)) / (factorial(k));
            return result; // lambda and k int QR CODE, so QR code with 0-1 value and int >= 0
        }
        #endregion
    }

    #region interfaces
    public interface IGenerateRandom
    {
        void generateRandom();
    }
        
    public interface IGeneratePoisson
    {
        void generatePoisson(double mi, int NT);
        void countDensity();
        void countCumultative();
    }
    #endregion

    #region generator classes
    public class GenerateRandom : IGenerateRandom
    {
        public double[] randomIntArray; // not encapsulated yet, use method, not indexer!

        public void generateRandom() 
        {
            int highestInt = (int)Math.Pow(2, 21); // may be bigger as well
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

    public class GeneratePoisson : IGeneratePoisson
    {
        private double[] poissonArray;
        public double[] density; // private...
        public double[] cumultative;

        public double this[int index]
        {
            get { return poissonArray[index]; }
            set { poissonArray[index] = value; }
        }

        public void generatePoisson(double mi, int NT)
        {
            GenerateRandom auxRand = new GenerateRandom(NT);
            double probability = mi / NT;

            for (int i = 0; i < poissonArray.Length; i++)
            {
                auxRand = new GenerateRandom(NT);
                for (int j = 0; j < auxRand.randomIntArray.Length; j++)
                    if (auxRand.randomIntArray[j] <= probability)
                        poissonArray[j]++;
            }
        }

        public void countDensity()
        {
            for (int i = 0; i <= (int)poissonArray.Max(); i++)
            {
                for (int j = 0; j < poissonArray.Length; j++)
                    if (poissonArray[j] == i)
                        ++density[i];
            }
            int length = poissonArray.Length;

            for (int i = 0; i < density.Length; i++)
                density[i] /= length;
        }

        public void countCumultative()
        {
            cumultative[0] = density[0];
            for (int i = 1; i < density.Length; i++)
                cumultative[i] = cumultative[i - 1] + density[i];
        }

        public GeneratePoisson(int arraySize, int mi, int NT)
        {
            poissonArray = new double[NT];
            generatePoisson(mi, NT);
            int densitySize = (int)poissonArray.Max() + 1;
            density = new double[densitySize];
            cumultative = new double[densitySize];
            countDensity();
            countCumultative();
        }
    }
    #endregion

    #region oxyPlot
    public class OxyPlotModel : INotifyPropertyChanged
    {
        private OxyPlot.PlotModel plotModel;
        public OxyPlot.PlotModel PlotModel { get { return plotModel; } set { plotModel = value; OnPropertyChanged("PlotModel"); } }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void SetUpLegend()
        {
            plotModel.LegendTitle = "Legenda"; //Tytuł legendy 
            plotModel.LegendOrientation = OxyPlot.LegendOrientation.Horizontal; 
            plotModel.LegendPlacement = OxyPlot.LegendPlacement.Outside; 
            plotModel.LegendPosition = OxyPlot.LegendPosition.TopRight; 
            plotModel.LegendBackground = OxyPlot.OxyColor.FromAColor(200, OxyPlot.OxyColors.White);
            plotModel.LegendBorder = OxyPlot.OxyColors.Black; //Ramka
        }

        OxyPlot.Series.LineSeries seriesPoints;

        public IList<OxyPlot.DataPoint> Points { get; private set; }
        
        private readonly List<OxyPlot.OxyColor> plotColour = new List<OxyPlot.OxyColor>
        {
            OxyPlot.OxyColors.Green,
            OxyPlot.OxyColors.IndianRed,
            OxyPlot.OxyColors.Coral,
            OxyPlot.OxyColors.Chartreuse,
            OxyPlot.OxyColors.Peru
        };
        private readonly List<OxyPlot.MarkerType> plotPoints = new List<OxyPlot.MarkerType>
        {
            OxyPlot.MarkerType.Plus,
            OxyPlot.MarkerType.Star,
            OxyPlot.MarkerType.Cross,
            OxyPlot.MarkerType.Custom,
            OxyPlot.MarkerType.Square
        };

        public void plot(double[] X, double[] Y)
        {
            this.PlotModel = new OxyPlot.PlotModel();  // reset previously set parameters
            plotModel.Series = new System.Collections.ObjectModel.Collection<OxyPlot.Series.Series> { };
            plotModel.Axes = new System.Collections.ObjectModel.Collection<OxyPlot.Axes.Axis> { }; //graphical setting of plot
            seriesPoints = new OxyPlot.Series.LineSeries
            {
                MarkerType = plotPoints[2],
                MarkerSize = 4,
                MarkerStroke = plotColour[2], 
                Title = "PoissonValues" //series title
            };

            // data complementing
            for (var itX = 0; itX < X.Length; itX++)
                     seriesPoints.Points.Add(new OxyPlot.DataPoint(X[itX], Y[itX]));

            plotModel.Series.Add(seriesPoints);
            
            var xAxis = new OxyPlot.Axes.LinearAxis(OxyPlot.Axes.AxisPosition.Bottom, "X")
            {
                MajorGridlineStyle = OxyPlot.LineStyle.Solid,
                MinorGridlineStyle = OxyPlot.LineStyle.Dot
            };
            plotModel.Axes.Add(xAxis);
            var yAxis = new OxyPlot.Axes.LinearAxis(OxyPlot.Axes.AxisPosition.Left, "Y")
            {
                MajorGridlineStyle = OxyPlot.LineStyle.Solid,
                MinorGridlineStyle = OxyPlot.LineStyle.Dot
            };
            plotModel.Axes.Add(yAxis);
        }
    }
    #endregion
}
