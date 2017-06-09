using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OxyPlot;
using System.ComponentModel;

namespace lab7
{
    /// <summary>
    /// Logika interakcji dla klasy WindowDistribution.xaml
    /// </summary>
    public partial class WindowDistribution : Window
    {
        private OxyPlotModel oxyPlotModel1;

        public WindowDistribution(double[] listY, double[] listXcumultative)
        {
            InitializeComponent();
            oxyPlotModel1 = new OxyPlotModel();
            this.DataContext = oxyPlotModel1;
            oxyPlotModel1.plot(listY, listXcumultative);
            OxyPlot.Wpf.PngExporter.Export(oxyPlotModel1.PlotModel, $@"\drawings\cumultative{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day} {DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}.png", 1600, 1600, OxyPlot.OxyColors.White, 96);
        }
    }
}
