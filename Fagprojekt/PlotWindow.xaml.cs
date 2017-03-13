using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Fagprojekt
{
    /// <summary>
    /// Interaction logic for PotWindow.xaml
    /// </summary>
    public partial class PlotWindow : Window
    {
        ViewModels.MainWindowModel viewModel;

        int key;

        LineSeries series2 = new LineSeries
        {
            MarkerType = MarkerType.Circle,
            MarkerSize = 4,
            MarkerStroke = OxyColors.White
        };

        public PlotWindow(int key)
        {
            this.key = key;
            viewModel = new ViewModels.MainWindowModel();
            DataContext = viewModel;

            CompositionTarget.Rendering += CompositionTargetRendering;

            InitializeComponent();




            Plot1.Model = viewModel.PlotModel;
        }
        private void CompositionTargetRendering(object sender, EventArgs e)
        {
            Plot1.InvalidatePlot(true);
        }
        public void UpdatePlotModel(Datapoint data)
        {
            viewModel.UpdateModel(data);
        }
    }
}
