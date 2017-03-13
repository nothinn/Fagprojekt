using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using OxyPlot.Annotations;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Wpf;

namespace Fagprojekt.ViewModels
{
    public class MainWindowModel : INotifyPropertyChanged
    {

        private PlotModel plotModel;
        private DateTime lastUpdate;

        public PlotModel PlotModel
        {
            get { return plotModel; }
            set { plotModel = value; OnPropertyChanged("PlotModel"); }
        }

        public MainWindowModel()
        {
            PlotModel = new PlotModel();

            SetUpModel();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void UpdateModel(Datapoint data)
        {

            var lineSerie = plotModel.Series[plotModel.Series.Count-1] as OxyPlot.Series.LineSeries;
            if (lineSerie != null)
            {
                int sum = 0;
                foreach (byte temp in data.data)
                {
                    sum += temp;
                }
                lineSerie.Points.Add(new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(DateTime.Now), sum));
            }



            lastUpdate = DateTime.Now;
        }

        private void SetUpModel()
        {


            PlotModel.LegendTitle = "Legend";
            PlotModel.LegendOrientation = LegendOrientation.Horizontal;
            PlotModel.LegendPlacement = LegendPlacement.Outside;
            PlotModel.LegendPosition = LegendPosition.TopRight;
            PlotModel.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
            PlotModel.LegendBorder = OxyColors.Black;
            
            var dateAxis = new OxyPlot.Axes.DateTimeAxis() { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, IntervalLength = 80 };
            PlotModel.Axes.Add(dateAxis);
            var valueAxis = new OxyPlot.Axes.LinearAxis() { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Dot, Title = "Value"};
            PlotModel.Axes.Add(valueAxis);


            var lineSerie = new OxyPlot.Series.LineSeries
            {
                StrokeThickness = 2,
                MarkerSize = 3,
                CanTrackerInterpolatePoints = false,
                Title = string.Format("Detector {0}", 0),
                Smooth = false,
            };

            lineSerie.Points.Add(new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(DateTime.Now), 1));
            PlotModel.Series.Add(lineSerie);
            

        }
    //[NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
