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
            RenderInLegend = false,
            MarkerType = MarkerType.Circle,
            MarkerSize = 4,
            MarkerStroke = OxyColors.White
        };

        public PlotWindow(int key)
        {

            this.key = key;
            this.Title = "Plot for ID: " + Convert.ToString(key, 16);
            viewModel = new ViewModels.MainWindowModel();
            DataContext = viewModel;

  
            InitializeComponent();




            Plot1.Model = viewModel.PlotModel;
        }

        public void UpdatePlotModel(Datapoint data)
        {
            viewModel.UpdateModel(data);
            textBlock.Dispatcher.Invoke(() => textBlock.Text = ShowBinary(data));
        }

        private string ShowBinary(Datapoint data)
        {
            StringBuilder builder = new StringBuilder();

            bool i = false;
            int j = 0;

            foreach (byte datapoint in data.data)
            {
                if (i)
                {
                    builder.AppendFormat("Byte: {0}: {1}\n", j, Convert.ToString(datapoint, 2).PadLeft(8, '0'));
                    i = !i;
                }
                else
                {
                    builder.AppendFormat("Byte: {0}:  {1},  ", j, Convert.ToString(datapoint, 2).PadLeft(8, '0'));
                    i = !i;
                }
                j++;
            }
            

            return builder.ToString();
        }

        public void Refresh()
        {
            Plot1.InvalidatePlot(true);
        }
    }
}
