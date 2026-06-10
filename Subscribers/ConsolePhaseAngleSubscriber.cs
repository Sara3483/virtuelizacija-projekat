using System;
using BaterijaContract.Events;

namespace VirtuelizacijaProjekat.Subscribers
{
    public class ConsolePhaseAngleSubscriber
    {
        public void PhaseAngleShiftHandler(object sender, PhaseAngleEventArgs e)
        {
            Console.Write("[PHASE ANGLE SHIFT] " + e.Message +
                        "| Direction: " + e.ShiftDirection +
                        "| RowIndex: " + e.RowIndex +
                        "| BatteryId: " + e.BatteryId +
                        "| SoC: " + e.SoC +
                        "| FrequencyHz: " + e.FrequencyHz +
                        "| Phi: " + e.Phi +
                        "| DeltaPhi: " + e.DeltaPhi);
        }
    }
}
