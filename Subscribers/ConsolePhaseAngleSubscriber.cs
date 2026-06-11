using System;
using BaterijaContract.Events;

namespace VirtuelizacijaProjekat.Subscribers
{
    public class ConsolePhaseAngleSubscriber
    {
        public void PhaseAngleShiftHandler(object sender, PhaseAngleEventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("----- PHASE ANGLE SHIFT -----");
            Console.WriteLine("Phi: " + e.Phi);
            Console.WriteLine("DeltaPhi: " + e.DeltaPhi);
            Console.WriteLine("FrequencyHz: " + e.FrequencyHz);
            Console.WriteLine("SoC: " + e.SoC);
            Console.WriteLine("Direction: " + e.ShiftDirection);
            Console.WriteLine("-----------------------------");
        }
    }
}
