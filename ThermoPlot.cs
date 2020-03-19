using System;
using System.Collections.Generic;
using System.Linq;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace ThermodynamicsPlotter
{
    public class ThermoPlot
    {
        public List<DataSeries> DataSeries { get; set; } = new List<DataSeries>();
        public List<Axis> Axes { get; set; } = new List<Axis>();

        public ThermoPlot()
        {
        }

        public List<Axis> GetAxes(bool onlyvisible = false)
        {
            var axes = new List<Axis>();

            foreach (var axis in Axes.Where(a => onlyvisible ? a.IsVisible : true))
            {
                axes.Add(axis);

                if (axis.ShowOnOppositeSide)
                {
                    axes.Add(axis.GetOpposingAxis());
                }
            }

            //Create missing axes
            if (!axes.Exists(a => a.IsHorizontal))
            {
                var axis = new Axis();
                axis.Minimum = DataSeries.Min(ds => ds.MinimumX);
                axis.Maximum = DataSeries.Max(ds => ds.MaximumX);
                axis.Position = AxisPosition.Bottom;

                axes.Add(axis);
            }
            if (!axes.Exists(a => a.IsVertical))
            {
                var axis = new Axis();
                axis.Minimum = DataSeries.Min(ds => ds.MinimumY);
                axis.Maximum = DataSeries.Max(ds => ds.MaximumY);
                axis.Position = AxisPosition.Left;

                axes.Add(axis);
            }

            return axes;
        }

        public PlotModel GetOxyplotModel()
        {
            var model = new PlotModel();

            foreach (var ds in DataSeries)
            {
                var series = new ScatterErrorSeries();

                foreach (var dp in ds.DataPoints)
                {
                    if (dp.HasErrors) series.Points.Add(new ScatterErrorPoint(dp.X, dp.Y, dp.ErrorX.Average(), dp.ErrorY.Average()));
                    else series.Points.Add(new ScatterErrorPoint(dp.X, dp.Y, 0, 0));
                }

                model.Series.Add(series);
            }

            foreach (var axis in GetAxes())
            {
                model.Axes.Add(axis.GetOxyPlotAxis());
            }

            return model;
        }
    }

    public class Axis
    {
        public string Legend { get; set; } = "";
        public string Unit { get; set; } = "";

        public double Minimum { get; set; } = 0;
        public double Maximum { get; set; } = 100;

        public AxisPosition Position { get; set; } = AxisPosition.Left;
        public bool ShowOnOppositeSide { get; set; } = false;
        public bool OpposingSideMinimal { get; set; } = true;

        public bool IsVisible { get; set; } = true;
        public bool ShowLegend { get; set; } = true;
        public bool ShowUnit { get; set; } = true;

        public bool GetShowLegend() => ShowLegend && !string.IsNullOrEmpty(Legend);
        public bool GetShowUnit() => ShowUnit && !string.IsNullOrEmpty(Unit);

        public bool IsHorizontal => Position == AxisPosition.Bottom || Position == AxisPosition.Top;
        public bool IsVertical => Position == AxisPosition.Left || Position == AxisPosition.Right;

        public AxisTickInfo MajorTickOptions { get; set; }
        public AxisTickInfo MinorTickOptions { get; set; }

        public Axis GetOpposingAxis()
        {
            var axis = new Axis()
            {
                Minimum = this.Minimum,
                Maximum = this.Maximum,
                Legend = this.Legend,
                Unit = this.Unit,
                IsVisible = this.IsVisible,
                ShowLegend = this.ShowLegend,
                ShowUnit = this.ShowUnit,
                MajorTickOptions = this.MajorTickOptions,
                MinorTickOptions = this.MinorTickOptions
            };

            if (OpposingSideMinimal)
            {
                axis.ShowLegend = false;
                axis.ShowUnit = false;
            }

            switch (Position)
            {
                case AxisPosition.Bottom: axis.Position = AxisPosition.Top; break;
                case AxisPosition.Left: axis.Position = AxisPosition.Right; break;
                case AxisPosition.Right: axis.Position = AxisPosition.Left; break;
                case AxisPosition.Top: axis.Position = AxisPosition.Bottom; break;
                case AxisPosition.None: axis.Position = AxisPosition.Top; break;
            }

            return axis;
        }

        public OxyPlot.Axes.LinearAxis GetOxyPlotAxis()
        {
            var axis = new LinearAxis();
            axis.Minimum = Minimum;
            axis.Maximum = Maximum;
            if (GetShowLegend()) axis.Title = Legend;
            if (GetShowUnit()) axis.Unit = Unit;
            axis.IsAxisVisible = IsVisible;
            axis.Position = Position;
            if (!ShowLegend) axis.LabelFormatter = NoLabelFormatter;
            if (MajorTickOptions != null)
            {
                axis.MajorStep = MajorTickOptions.StepSize;
                axis.TickStyle = MajorTickOptions.Style;
            }
            if (MinorTickOptions != null)
            {
                axis.MinorStep = MinorTickOptions.StepSize;
            }

            return axis;
        }

        string NoLabelFormatter(double v) => "";
    }

    public class AxisTickInfo
    {
        public double StepSize { get; set; } = double.NaN;
        public double TickAnchor { get; set; } = double.NaN;
        public bool ShowExtremes { get; set; } = false;
        public bool ShowInside { get => Style == TickStyle.Inside; set { var v = value; if (v) Style = TickStyle.Inside; else Style = TickStyle.Outside; } }
        public TickStyle Style { get; set; }

        public bool IsVisible { get; set; } = true;

        public bool UseCustomTickValues => !double.IsNaN(TickAnchor);

        public double[] GetCustomTickValues(double min, double max)
        {
            return new double[0];
        }
    }
}
