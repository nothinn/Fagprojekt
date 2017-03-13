using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
    /// Interaction logic for HeatMap.xaml
    /// </summary>
    public partial class HeatMap : Window
    {
        int[,] dataArray;
        int[,] colorArray;
        int columnlength;

        Dictionary<int, Datapoint> data;

        public HeatMap(Dictionary<int, Datapoint> data)
        {
            this.data = data;

            int[,] array = new int[,] {};

            InitializeComponent();

            this.SizeToContent = SizeToContent.WidthAndHeight;

            lst.ItemsSource = ArrayToList(array);

            //this.LayoutUpdated += HeatMap_ContentRendered;

        }

        private void HeatMap_ContentRendered(object sender, EventArgs e)
        {

            foreach (Button button in FindVisualChildren<Button>(this))
            {
                int key = (int)button.Content;

                if (key != 0)
                {

                    button.Background = getColor(key);
                }
                else
                {
                    button.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    button.IsEnabled = false;

                }
            }

        }

        private SolidColorBrush getColor(int key)
        {
            DateTime time = data[key].time;

            byte seconds = (byte)((DateTime.UtcNow - time).TotalSeconds);

            return new SolidColorBrush(Color.FromRgb(seconds, 255, 0));
        }

        public void UpdateHeatmap()
        {
            double temp = Math.Sqrt(data.Values.Count);

            int length = (int)temp;

            if (temp - length > 0)
            {
                length++;
            }
            columnlength = length;

            dataArray = new int[length, length];
            colorArray = new int[length, length];


            int i = 0;
            int j = 0;

            foreach (Datapoint data in data.Values)
            {
                dataArray[i, j] = data.key;
                colorArray[i, j] = (DateTime.UtcNow - data.time).Milliseconds;
                i++;
                if (i >= length)
                {
                    i = 0;
                    j++;
                }
            }
            lst.ItemsSource = ArrayToList(dataArray);

            
            
        }

        private List<List<int>> ArrayToList(int[,] array)
        {
            List<List<int>> lsts = new List<List<int>>();

            for (int i = 0; i < array.GetLength(0); i++)
            {
                lsts.Add(new List<int>());

                for (int j = 0; j < array.GetLength(1); j++)
                {
                    lsts[i].Add(array[i, j]);
                }
            }

            return lsts;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Button button = (Button)sender;


            int key = (int)button.Content;


            PlotWindow plotwindow;
            Timer plottimer;


            button.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));

            plotwindow = new PlotWindow(key);
            plotwindow.Show();

            plottimer = new Timer(200);
            plottimer.Elapsed += (s, se) => plotwindow.UpdatePlotModel(data[key]);
            plottimer.AutoReset = true;
            plottimer.Start();



        }



        private void Button_Mouse_Over(object sender, MouseEventArgs e)
        {

        }


        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

    }


}
