using System;
using System.Collections.Generic;
using System.Linq;
using MathNet;

namespace ThermodynamicsPlotter
{
    public class DataSeries
    {
        public string Name { get; set; } = "";

        public List<DataPoint> DataPoints { get; set; } = new List<DataPoint>();

        public double MinimumX => DataPoints.Min(dp => dp.X);
        public double MinimumY => DataPoints.Min(dp => dp.Y);
        public double MaximumX => DataPoints.Max(dp => dp.X);
        public double MaximumY => DataPoints.Max(dp => dp.Y);

        public DataSeries()
        {
        }

        public void AddDataPoint(double x, double y) => DataPoints.Add(new DataPoint(x, y));
        public void AddDataPoint(DataPoint dp) => DataPoints.Add(dp);

        public void GroupDataPoints(double window)
        {
            var groups = DataPoints.GroupByProximity(window).ToList();

            var newdata = new List<DataPoint>();

            foreach (var g in groups)
            {
                var newx = g.Average(dp => dp.X);
                var sdx = 2*MathNet.Numerics.Statistics.Statistics.StandardDeviation(g.Select(dp => dp.X));

                var newy = g.Average(dp => dp.Y);
                var sdy = 2*MathNet.Numerics.Statistics.Statistics.StandardDeviation(g.Select(dp => dp.Y));

                var newdp = new DataPoint(newx, newy);
                newdp.SetErrorBars(sdx, sdy);

                newdata.Add(newdp);
            }

            DataPoints = newdata;
        }

        public void TranslatePoints(double value)
        {
            DataPoints.ForEach(dp => dp.X += value);
        }
    }

    public class DataPoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        /// <summary>
        /// Error on the X axis [negative, positive]
        /// </summary>
        public double[] ErrorX { get; set; } = new double[2];
        /// <summary>
        /// Error on the Y axis [negative, positive]
        /// </summary>
        public double[] ErrorY { get; set; } = new double[2];

        public DataPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Set error bars (assuming normal distribution)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetErrorBars(double x, double y)
        {
            x = Math.Abs(x);
            y = Math.Abs(y);

            ErrorX = new double[] { x, x };
            ErrorY = new double[] { y, y };
        }

        /// <summary>
        /// Set error bars with custom distribution
        /// </summary>
        /// <param name="x1">Negative X error</param> 
        /// <param name="x2">Positive X error</param>
        /// <param name="y1">Negative Y error</param>
        /// <param name="y2">Positive Y error</param>
        public void SetErrorBars(double x1, double x2, double y1, double y2)
        {
            ErrorX = new double[] { Math.Abs(x1), Math.Abs(x2) };
            ErrorY = new double[] { Math.Abs(y1), Math.Abs(y2) };
        }

        public bool HasErrors => ErrorY.Any(v => v > 0) || ErrorX.Any(v => v > 0);
        public bool HasVerticalError => ErrorY.Any(v => v > 0);
        public bool HasHorizontalError => ErrorX.Any(v => v > 0);

        public override string ToString()
        {
            string ex = "";
            if (ErrorX.Sum() > 0)
            {
                if (ErrorX[0] != ErrorX[1]) ex = " [" + ErrorX[0].ToString("###0.0") + "," + ErrorX[1].ToString("###0.0") + "]";
                else ex = " [" + ErrorX[0].ToString("###0.0") + "]";
            }

            string ey = "";
            if (ErrorY.Sum() > 0)
            {
                if (ErrorY[0] != ErrorY[1]) ey = " [" + ErrorY[0].ToString("###0.0") + "," + ErrorY[1].ToString("###0.0") + "]";
                else ey = " [" + ErrorY[0].ToString("###0.0") + "]";
            }


            return X.ToString("#####0.0") + ex + ", " + Y.ToString("#####0.0") + ey;
        }
    }
}
