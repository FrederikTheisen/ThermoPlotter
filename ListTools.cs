using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Foundation;
using PdfKit;

namespace ThermodynamicsPlotter
{
    public static class ListTools
    {
        public static IEnumerable<IEnumerable<DataPoint>> GroupByProximity(this List<DataPoint> source, double threshold)
        {
            var g = new List<DataPoint>();
            foreach (var x in source.OrderBy(dp => dp.X))
            {
                if ((g.Count != 0) && (x.X > g.Average(dp => dp.X) + threshold))
                {
                    yield return g;
                    g = new List<DataPoint>();
                }
                g.Add(x);
            }
            yield return g;
        }
    }

    public static class OxyTools
    {
        public static PdfDocument GetPDF(this OxyPlot.PlotModel model, float width, float height)
        {
            if (model is null) return null;

            NSData dat = null;

            model.DefaultFont = OxyPlot.StandardFonts.Helvetica.RegularFont.FontName;

            using (var stream = new MemoryStream())
            {
                OxyPlot.PdfExporter.Export(model, stream, width, height);

                dat = NSData.FromArray(stream.GetBuffer());
            }

            return new PdfDocument(dat);
        }
    }
}
