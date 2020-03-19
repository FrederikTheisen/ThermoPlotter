using System;
using System.Linq;

namespace ThermodynamicsPlotter
{
    public class LinearRegression
    {
        public bool UseMeanOrigin { get; set; }

        public string Name;
        public string XVar;
        public string YVar;

        bool IsFitted = false;
        double Slope;
        double Intercept;

        double SlopeError;
        double InterceptError;

        double[] XValues;
        double[] YValues;
        double[] Weights;

        public double OriginOffsetX { get; set; }
        public double OriginOffsetY { get; set; }

        public static ConfidenceLevel SelectedConfidenceLevel = LinearRegression.ConfidenceLevel.CL95;
        public static int DecimalDigits = 2;

        public double[] Residuals
        {
            get
            {
                if (!IsFitted) return new double[0];

                var l = XValues.Length;
                var res = new double[l];
                for (int i = 0; i < l; i++)
                {
                    var X = XValues[i];
                    res[i] = YValues[i] - (Slope * X + Intercept);
                }

                return res;
            }
        }

        public LinearRegression(double[] x, double[] y)
        {
            XValues = x;
            YValues = y;
        }

        public LinearRegression(double[] x, double[] y, string name)
        {
            XValues = x;
            YValues = y;

            Name = name;
        }

        public LinearRegression(double[] x, double[] y, double[] w, string name)
        {
            XValues = x;
            YValues = y;
            Weights = w;

            Name = name;
        }

        internal void SetHeaders(string v1, string v2)
        {
            XVar = v1;
            YVar = v2;
        }

        public void SetOrigin(int mode)
        {
            if (mode == 0)
            {
                OriginOffsetX = XValues.Average();

                XValues = XValues.Select(v => v - OriginOffsetX).ToArray();
            }
            else if (mode == 1)
            {
                OriginOffsetY = YValues.Average();

                YValues = YValues.Select(v => v - OriginOffsetY).ToArray();
            }
            else if (mode == 2)
            {
                SetOrigin(0);
                SetOrigin(1);
            }
        }

        public void Fit(bool weighted = false)
        {
            if (weighted) LinFitWeighted();
            else Fit();

            IsFitted = true;
        }

        private void Fit()
        {
            var (intercept, slope) = MathNet.Numerics.LinearRegression.SimpleRegression.Fit(XValues, YValues);

            Slope = slope;
            Intercept = intercept;
        }

        private void FitWeighted()
        {
            var parameters = MathNet.Numerics.Fit.PolynomialWeighted(XValues, YValues, Weights, 1);

            Slope = parameters[1];
            Intercept = parameters[0];
        }

        private void LinFitWeighted()
        {
            var x = XValues.Select(p => new[] { p }).ToArray();
            var y = YValues.Select(p => p).ToArray();
            var w = Weights.Select(p => Convert.ToDouble(p)).ToArray();

            var parameters = MathNet.Numerics.LinearRegression.WeightedRegression.Weighted(x, y, w, intercept: !UseMeanOrigin);

            Slope = parameters.Last();
            Intercept = parameters.Length > 1 ? parameters[0] : 0;
        }

        /// <summary>
        /// Get fit errors. Returns Tuple(slope error, intercept error).
        /// </summary>
        /// <returns> </returns>
        private Tuple<double, double> GetFittingErrors()
        {
            if (!IsFitted) return new Tuple<double, double>(0, 0);

            var res = Residuals;
            var sumofsquared = LRM.SumOfSquared(res);
            var ssd = LRM.ArraySumSquareDeviation(XValues);
            var oneoverxlength = 1f / (XValues.Length - 2);
            var se_slope = Math.Sqrt(oneoverxlength * sumofsquared / ssd);

            var se_intercept = se_slope * Math.Sqrt(1f / XValues.Length * LRM.SumOfSquared(XValues));

            return new Tuple<double, double>(se_slope, se_intercept);
        }

        public Tuple<double, double> GetStandardDeviations() => GetConfidenceInterval(ConfidenceLevel.StandardDeviation);
        public Tuple<double, double> GetConfidenceInterval(ConfidenceLevel level)
        {
            var errors = GetFittingErrors();
            var tstat = TStat(level);

            return new Tuple<double, double>(errors.Item1 * tstat, errors.Item2 * tstat);
        }

        public void ComputeConfidenceIntervals(ConfidenceLevel level)
        {
            var se = GetFittingErrors();
            var tstat = TStat(level);

            SlopeError = se.Item1 * tstat;
            InterceptError = se.Item2 * tstat;
        }

        private void NCSSLinRegSlopeDeviation()
        {
            //NCSS.com Confidence intervals for linear regression slope

            var res = Residuals;
            var sumsquared = LRM.SumOfSquared(res);
            var s = Math.Sqrt(sumsquared / (XValues.Length - 2));

            var se = s / Math.Sqrt(LRM.SumOfSquared(XValues));
        }

        public double TStat(ConfidenceLevel level) => MathNet.Numerics.Distributions.StudentT.InvCDF(0, 1, DegreesOfFreedom(), TQuantile(level));
        private int DegreesOfFreedom() => XValues.Length - 2;
        private double TQuantile(ConfidenceLevel level) => 1 - ConfidenceValue(level) / 2;

        #region ConfidenceLevel

        public enum ConfidenceLevel
        {
            CL99,
            CL95,
            StandardDeviation,
            CL50,
        }

        double ConfidenceValue(ConfidenceLevel level) => level switch
        {
            ConfidenceLevel.CL99 => 1 - 0.99,
            ConfidenceLevel.CL95 => 1 - 0.95,
            ConfidenceLevel.StandardDeviation => 1 - 0.683,
            ConfidenceLevel.CL50 => 1 - 0.5,
            _ => 1 - 0.95,
        };

        #endregion

        private static class LRM
        {
            internal static double SumOfSquared(double[] arr)
            {
                var ss = 0.0;
                foreach (var v in arr) ss += (v * v);
                return ss;
            }

            internal static double ArrayMean(double[] arr)
            {
                return arr.Average();
            }

            internal static double ArraySumSquareDeviation(double[] arr)
            {
                var mean = ArrayMean(arr);
                var ss = 0.0;
                foreach (var v in arr)
                {
                    var d = v - mean;
                    ss += (d * d);
                }
                return ss;
            }
        }

        public string MCErrorName()
        {
            var yvar = YVar?.ToLower() ?? "yvar";

            if (yvar.Contains("h")) return "dh";
            else if (yvar.Contains("s")) return "ds";
            else if (yvar.Contains("g")) return "dg";
            else return yvar;
        }

        public void Print()
        {
            if (!IsFitted) { Console.WriteLine("Function has not been fitted"); return; }

            if (true)
            {
                var digits = "####0." + new string('0', DecimalDigits);

                if (XVar == null)
                {
                    Console.WriteLine("Y = ("
                        + Slope.ToString(digits) + " ± "
                        + SlopeError.ToString(digits) + ") * X + ("
                        + Intercept.ToString(digits) + " ± "
                        + InterceptError.ToString(digits) + ")");
                }
                else
                {
                    Console.WriteLine(YVar.Replace(" ", "_") + " = ("
                        + Slope.ToString(digits) + " ± "
                        + SlopeError.ToString(digits) + ") * " + XVar.Replace(" ", "_") + " + ("
                        + Intercept.ToString(digits) + " ± "
                        + InterceptError.ToString(digits) + ")");
                }
            }
            else
            {
                Console.WriteLine(Name + "\t"
                    + MCErrorName() + "\t"
                    + Slope + "\t"
                    + SlopeError + "\t"
                    + Intercept + "\t"
                    + InterceptError + "\t"
                    + OriginOffsetX + "\t"
                    + OriginOffsetY);
            }
        }
    }
}
