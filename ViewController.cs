using System;

using AppKit;
using Foundation;

namespace ThermodynamicsPlotter
{
    public partial class ViewController : NSViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Do any additional setup after loading the view.
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }

        partial void OpenFileButtonAction(NSObject sender)
        {
            var rawpath = PathInputField.StringValue;

            var path = MacPath.MacPath.ToPath(rawpath);

            var data = DataReader.ReadDataSeries(path);

            data.ForEach(ds => ds.TranslatePoints(-273.15));

            var plot = new ThermoPlot();
            plot.DataSeries = data;
            plot.Axes.Add(new Axis()
            {
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                Minimum = 5,
                Maximum = 35,
                Legend = "Temperature",
                Unit = "K",
                MajorTickOptions = new AxisTickInfo
                {
                    ShowInside = true,
                    StepSize = 5,
                }
            });

            plot.Axes.Add(new Axis()
            {
                Position = OxyPlot.Axes.AxisPosition.Left,
                ShowOnOppositeSide = true,
                Minimum = -100,
                Maximum = 50,
                Legend = "Energy",
                Unit = "kJ/mol",
                OpposingSideMinimal = true,
                MajorTickOptions = new AxisTickInfo
                {
                    ShowInside = true,
                    StepSize = 25
                }
            });

            foreach (var ds in plot.DataSeries) ds.GroupDataPoints(1);

            var model = plot.GetOxyplotModel();

            PlotPreviewView.Document = model.GetPDF(300, 300);
        }

        partial void RedrawPlotAction(NSObject sender)
        {
            throw new NotImplementedException();
        }
    }
}
