using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

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

        CanHandler can;

        Dictionary<int, PlotWindow> plotWindows;

        public HeatMap(CanHandler can)
        {
            this.can = can;
            plotWindows = new Dictionary<int, PlotWindow>();
            Dictionary<int, Datapoint> data = can.Dict;

            this.data = data;

            int[,] array = new int[,] {};

            InitializeComponent();

            this.SizeToContent = SizeToContent.WidthAndHeight;

            lst.ItemsSource = ArrayToList(array);


            can.ValueAdded += Can_ValueAdded;
            //this.LayoutUpdated += HeatMap_ContentRendered;

            can.ValueChanged += Can_ValueChanged;

        }

        private void Can_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (plotWindows.ContainsKey(e.Data.key))
            {
                plotWindows[e.Data.key].UpdatePlotModel(e.Data);
            }
        }

        private void Can_ValueAdded(object sender, ValueChangedEventArgs e)
        {
            UpdateHeatmap();
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
            double temp = Math.Sqrt(can.Dict.Values.Count);

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

            foreach (Datapoint data in can.Dict.Values)
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
            this.Dispatcher.Invoke(()=> lst.ItemsSource = ArrayToList(dataArray));

            
            
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

        public void UpdatePlots()
        {
            foreach(PlotWindow plotwindow in plotWindows.Values)
            {
                plotwindow.Refresh();
            }
        }




        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Button button = (Button)sender;


            int key = (int)button.Content;

            if (!plotWindows.ContainsKey(key))
            {

                PlotWindow plotwindow;
                button.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));

                Thread newWindowThread = new Thread(new ThreadStart(() => {
                    plotwindow = new PlotWindow(key);
                    plotwindow.Show();

                    plotwindow.Closed += ((s, se) => {
                        plotWindows.Remove(key);
                        Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                    });

                    plotWindows.Add(key, plotwindow);

                    System.Windows.Threading.Dispatcher.Run();

                } ));
                

                newWindowThread.SetApartmentState(ApartmentState.STA);
                newWindowThread.IsBackground = true;
                newWindowThread.Start();

            }
            else
            {
                plotWindows[key].Close();
                plotWindows.Remove(key);
            }


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


        private void align_button_Click(object sender, RoutedEventArgs e)
        {
            double width = System.Windows.SystemParameters.PrimaryScreenWidth;
            double height = System.Windows.SystemParameters.PrimaryScreenHeight;

            double tempHeight = 0;
            double tempWidt = 0;

            foreach (PlotWindow window in plotWindows.Values)
            {

                window.Dispatcher.Invoke(() =>
                {



                    if (tempWidt + window.Width > width)
                    {
                        tempWidt = 0;
                        tempHeight += window.ActualHeight;
                    }
                    window.Left = tempWidt;
                    window.Top = tempHeight;

                    tempWidt += window.Width;
                    window.Activate();

                });
            }

        }
    }
}
