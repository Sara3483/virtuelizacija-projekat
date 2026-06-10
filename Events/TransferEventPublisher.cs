using BaterijaContract.Events;
namespace VirtuelizacijaProjekat.Events
{
    public class TransferEventPublisher
    {
        public event TransferEventHandler OnTransferStarted;
        public event TransferEventHandler OnSampleReceived;
        public event TransferEventHandler OnTransferCompleted;
        public event TransferEventHandler OnWarningRaised;

        public void RaiseTransferStarted(string message)
        {
            if(OnTransferStarted != null)
            {
                OnTransferStarted(this, new CustomEventArgs(message));
            }
        }

        public void RaiseSampleReceived(string message, int rowIndex)
        {
            if(OnSampleReceived != null)
            {
                OnSampleReceived(this, new CustomEventArgs(message, rowIndex));
            }
        }

        public void RaiseTransferCompleted(string message)
        {
            if(OnTransferCompleted != null)
            {
                OnTransferCompleted(this, new CustomEventArgs(message));
            }
        }

        public void RaiseWarning(string message, int rowIndex)
        {
            if(OnWarningRaised != null)
            {
                OnWarningRaised(this, new CustomEventArgs(message, rowIndex));
            }
        }
    }
}
