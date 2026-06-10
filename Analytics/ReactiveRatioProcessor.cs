using Common;
using System;
using System.Configuration;
using System.Globalization;
using VirtuelizacijaProjekat.Events;

namespace VirtuelizacijaProjekat.Analytics
{
    public class ReactiveRatioProcessor
    {
        private readonly ReactiveRatioEventPublisher reactiveRatioEventPublisher;
        private double qSum;
        private int qCount;

        public ReactiveRatioProcessor(ReactiveRatioEventPublisher reactiveRatioEventPublisher)
        {
            this.reactiveRatioEventPublisher = reactiveRatioEventPublisher;
            qSum = 0;
            qCount = 0;
        }

        public void Reset()
        {
            qSum = 0;
            qCount = 0;
        }

        public void ProcessSample(EisSample sample, EisMeta meta, Action<string> writeReject)
        {
            if (sample.R_ohm == 0)
            {
                throw new InvalidOperationException("R_ohm value is zero.");
            }

            double q = Math.Abs(sample.X_ohm) / sample.R_ohm;

            double qMin = double.Parse(ConfigurationManager.AppSettings["QMin"], CultureInfo.InvariantCulture);
            double qMax = double.Parse(ConfigurationManager.AppSettings["QMax"], CultureInfo.InvariantCulture);
            double qDev = double.Parse(ConfigurationManager.AppSettings["QDev"], CultureInfo.InvariantCulture);

            qSum += q;
            qCount++;

            double qMean = qSum / qCount;

            if (q < qMin)
            {
                writeReject("Reactive ratio out of bounds: " + q);

                string message = ConfigurationManager.AppSettings["ReactiveRatioOutOfBoundsMessage"];

                reactiveRatioEventPublisher.RaiseReactiveRatioOutOfBounds(message, sample.RowIndex, meta.BatteryId, meta.SoCPercentage, sample.FrequencyHz, q, qMean, "Below treshold!");
            }
            else if (q > qMax)
            {
                writeReject("Reactive ratio out of bounds: " + q);

                string message = ConfigurationManager.AppSettings["ReactiveRatioOutOfBoundsMessage"];

                reactiveRatioEventPublisher.RaiseReactiveRatioOutOfBounds(message, sample.RowIndex, meta.BatteryId, meta.SoCPercentage, sample.FrequencyHz, q, qMean, "Above treshold!");
            }

            if (q < (1 - qDev) * qMean)
            {
                string message = ConfigurationManager.AppSettings["ReactiveRatioWarningMessage"];

                reactiveRatioEventPublisher.RaiseReactiveRatioWarning(message, sample.RowIndex, meta.BatteryId, meta.SoCPercentage, sample.FrequencyHz, q, qMean, "Below expected!");
            }
            else if (q > (1 + qDev) * qMean)
            {
                string message = ConfigurationManager.AppSettings["ReactiveRatioWarningMessage"];

                reactiveRatioEventPublisher.RaiseReactiveRatioWarning(message, sample.RowIndex, meta.BatteryId, meta.SoCPercentage, sample.FrequencyHz, q, qMean, "Above expected!");
            }
        }
    }
}