using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ThermodynamicsPlotter
{
    public static class DataReader
    {
        public static string[] Headers;

        public static List<double[]> ReadFile(string path, Delimiter delimiter = Delimiter.Unknown)
        {
            List<double[]> data = new List<double[]>();

            try
            {
                using var stream = File.Open(path, FileMode.Open);
                using var reader = new StreamReader(stream);
                string line;
                bool isfirstline = true;

                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line.Trim())) continue;

                    if (delimiter == Delimiter.Unknown) delimiter = GetDelimiter(line);
                    if (delimiter == Delimiter.Unknown) return null;

                    var columns = line.Split(DelimiterString(@delimiter), StringSplitOptions.RemoveEmptyEntries);

                    var dat = new double[columns.Length];
                    bool add = false;

                    for (int i = 0; i < dat.Length; i++)
                    {
                        if (double.TryParse(columns[i].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                        {
                            dat[i] = v;

                            add = true;
                        }
                        else if (isfirstline)
                        {
                            Headers = columns;

                            break;
                        }
                    }

                    if (add) data.Add(dat);

                    isfirstline = false;
                }

                return data;
            }
            catch { return null; }
        }

        public static List<DataSeries> ReadDataSeries(string path, Delimiter delimiter = Delimiter.Unknown)
        {
            var data = ReadFile(path, delimiter);

            var dataseries = (from __ in data[0].Skip(1)
                              select new DataSeries()).ToList();

            for (int i = 0; i < data.Count; i++)
            {
                for (int n = 1; n < data[0].Length; n++)
                {
                    dataseries[n-1].AddDataPoint(data[i][0], data[i][n]);
                }
            }

            return dataseries;
        }

        static Delimiter GetDelimiter(string line)
        {
            if (line.Contains("\t")) return Delimiter.Tab;
            if (line.Contains(" ")) return Delimiter.Space;
            if (line.Contains(".") && line.Contains(",")) return Delimiter.Comma;
            else return Delimiter.Unknown;
        }

        public enum Delimiter
        {
            Tab,
            Space,
            Comma,
            Unknown
        }

        private static string DelimiterString(Delimiter delimiter)
        {
            return delimiter switch
            {
                Delimiter.Tab => "\t",
                Delimiter.Space => " ",
                Delimiter.Comma => ",",
                _ => DelimiterString(Delimiter.Tab),
            };
        }

        private static double[] GetColumn(this List<double[]> data, int column)
        {
            var c = new double[data.Count];

            for (int i = 0; i < c.Length; i++)
            {
                c[i] = data[i][column];
            }

            return c;
        }

        public static double[][] ToMatrix(this List<double[]> data)
        {
            int columns = data[0].Length;
            double[][] matrix = new double[columns][];

            for (int i = 0; i < columns; i++)
            {
                matrix[i] = data.GetColumn(i);
            }

            return matrix;
        }
    }
}
