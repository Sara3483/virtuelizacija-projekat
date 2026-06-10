using BaterijaContract.Events;

namespace VirtuelizacijaProjekat.Events
{
    public class PhaseAngleEventPublisher
    {
        public event PhaseAngleEventHandler OnPhaseAngleShift;

        public void RaisePhaseAngleShift(string message, int rowIndex, string batteryId, double soc, double frequencyHz, double phi, double deltaPhi, string shiftDirection)
        {
            if (OnPhaseAngleShift != null)
            {
                OnPhaseAngleShift(this, new PhaseAngleEventArgs(message, rowIndex, batteryId, soc, frequencyHz, phi, deltaPhi, shiftDirection));
            }
        }
    }
}