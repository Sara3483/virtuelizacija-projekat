using Common;
using System;
using System.Configuration;
using System.Globalization;
using VirtuelizacijaProjekat.Events;

namespace VirtuelizacijaProjekat.Analytics
{
    public class PhaseAngleProcessor
    {
        private readonly PhaseAngleEventPublisher phaseAngleEventPublisher;
        private double? previousPhi;

        public PhaseAngleProcessor(PhaseAngleEventPublisher phaseAngleEventPublisher)
        {
            this.phaseAngleEventPublisher = phaseAngleEventPublisher;
            previousPhi = null;
        }

        public void Reset()
        {
            previousPhi = null;
        }

        public void ProcessSample(EisSample sample, EisMeta meta)
        {
            double phi = Math.Atan2(sample.X_ohm, sample.R_ohm);

            if (previousPhi.HasValue)
            {
                double deltaPhi = phi - previousPhi.Value;

                double phiThreshold = double.Parse(
                    ConfigurationManager.AppSettings["PhiThreshold"],
                    CultureInfo.InvariantCulture);

                if (Math.Abs(deltaPhi) > phiThreshold)
                {
                    string direction = GetDirection(deltaPhi);

                    string message = ConfigurationManager.AppSettings["PhaseAngleShiftMessage"];

                    phaseAngleEventPublisher.RaisePhaseAngleShift(message, sample.RowIndex, meta.BatteryId, meta.SoCPercentage,
                        sample.FrequencyHz, phi, deltaPhi, direction);
                }
            }

            previousPhi = phi;
        }

        private string GetDirection(double deltaPhi)
        {
            if (deltaPhi > 0)
            {
                return "Moving toward inductive!";
            }

            return "Moving toward capacitive!";
        }
    }
}