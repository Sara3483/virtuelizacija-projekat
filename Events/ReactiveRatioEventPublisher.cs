using BaterijaContract.Events;

namespace VirtuelizacijaProjekat.Events
{
    public class ReactiveRatioEventPublisher
    {
        public event ReactiveRatioEventHandler OnReactiveRatioOutOfBounds;
        public event ReactiveRatioEventHandler OnReactiveRatioWarning;

        public void RaiseReactiveRatioOutOfBounds(string message, int rowIndex, string batteryId, double soc, double frequencyHz, double q, double qMean, string direction)
        {
            if (OnReactiveRatioOutOfBounds != null)
            {
                OnReactiveRatioOutOfBounds(this, new ReactiveRatioEventArgs(message, rowIndex, batteryId, soc, frequencyHz, q, qMean, direction));
            }
        }

        public void RaiseReactiveRatioWarning(string message, int rowIndex, string batteryId, double soc, double frequencyHz, double q, double qMean, string direction)
        {
            if (OnReactiveRatioWarning != null)
            {
                OnReactiveRatioWarning(this, new ReactiveRatioEventArgs(message, rowIndex, batteryId, soc, frequencyHz, q, qMean, direction));
            }
        }
    }
}