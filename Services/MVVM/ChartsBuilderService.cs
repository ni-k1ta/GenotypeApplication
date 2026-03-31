using GenotypeApplication.Models.Structure_Harvester;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace GenotypeApplication.Services.MVVM
{
    public class ChartsBuilderService
    {
        private static readonly OxyColor MarkerColor = OxyColors.SteelBlue;
        private static readonly OxyColor ErrorBarColor = OxyColors.Black;

        public (PlotModel MeanLnPK, PlotModel LnPrimeK, PlotModel LnDoublePrimeK, PlotModel DeltaK)
            BuildCharts(List<EvannoParametersModel> data)
        {
            return (
                BuildMeanLnPKPlot(data),
                BuildLnPrimeKPlot(data),
                BuildLnDoublePrimeKPlot(data),
                BuildDeltaKPlot(data)
            );
        }

        private PlotModel BuildMeanLnPKPlot(List<EvannoParametersModel> data)
        {
            var model = CreateBasePlotModel("K", "Mean LnP(K)");

            // Вертикальные error bars реализуются через ScatterErrorSeries.
            // Каждая точка: X = K, Y = Mean, ErrorY = Stdev.
            var errorSeries = new ScatterErrorSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerFill = MarkerColor,
                ErrorBarColor = ErrorBarColor,
                ErrorBarStrokeThickness = 0.8,
                ErrorBarStopWidth = 2,
                TrackerFormatString = "K: {2:0}\nMean LnP(K): {4:0.####}\nSD: {ErrorY:0.####}"
            };

            foreach (var row in data)
            {
                // errorX = 0 (без горизонтальных error bars), errorY = stdev
                errorSeries.Points.Add(new ScatterErrorPoint(row.K, row.MeanLnPK, 0, row.StdevLnPK));
            }

            model.Series.Add(errorSeries);
            //ConfigureKAxis(model, data);

            return model;
        }

        private PlotModel BuildLnPrimeKPlot(List<EvannoParametersModel> data)
        {
            var model = CreateBasePlotModel("K", "Ln'(K)");

            var series = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerFill = MarkerColor
            };

            foreach (var row in data)
            {
                if (row.LnPrimeK is not null)
                    series.Points.Add(new ScatterPoint(row.K, row.LnPrimeK.Value));
            }

            model.Series.Add(series);
            //ConfigureKAxis(model, data);

            return model;
        }

        private PlotModel BuildLnDoublePrimeKPlot(List<EvannoParametersModel> data)
        {
            var model = CreateBasePlotModel("K", "|Ln''(K)|");

            var series = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerFill = MarkerColor
            };

            foreach (var row in data)
            {
                if (row.LnDoublePrimeK is not null)
                    series.Points.Add(new ScatterPoint(row.K, row.LnDoublePrimeK.Value));
            }

            model.Series.Add(series);
            //ConfigureKAxis(model, data);

            return model;
        }

        private PlotModel BuildDeltaKPlot(List<EvannoParametersModel> data)
        {
            var model = CreateBasePlotModel("K", "Delta K");

            var series = new LineSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerFill = MarkerColor,
                Color = MarkerColor,
                StrokeThickness = 0.8
            };

            foreach (var row in data)
            {
                if (row.DeltaK is not null)
                    series.Points.Add(new DataPoint(row.K, row.DeltaK.Value));
            }

            model.Series.Add(series);
            //ConfigureKAxis(model, data);

            return model;
        }

        private static PlotModel CreateBasePlotModel(string xTitle, string yTitle)
        {
            var model = new PlotModel
            {
                // отступы для подписей осей
                Padding = new OxyThickness(10, 10, 20, 10)
            };

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = xTitle,
                MajorStep = 1,
                MinorStep = 1,
                MinimumMajorStep = 1,
                StringFormat = "0",
                MinimumPadding = 0.05,
                MaximumPadding = 0.05
            });

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = yTitle,
                MinimumPadding = 0.05,
                MaximumPadding = 0.05
            });

            return model;
        }

        //private static void ConfigureKAxis(PlotModel model, List<EvannoParametersModel> data)
        //{
        //    if (data.Count == 0) return;

        //    var xAxis = model.Axes.First(a => a.Position == AxisPosition.Bottom);
        //    xAxis.Minimum = data.Min(r => r.K) - 0.5;
        //    xAxis.Maximum = data.Max(r => r.K) + 0.5;
        //}
    }
}
