using System;
using BaterijaContract.Events;
using System.IO;

namespace VirtuelizacijaProjekat.Subscribers
{
    public class FileTransferSubscriber
    {
        private readonly string logPath;

        public FileTransferSubscriber(string logPath)
        {
            this.logPath = logPath;
        }

        public void TransferStartedHandler(object sender, CustomEventArgs e)
        {
            WriteToLog("[START] " + e.Message);
        }

        public void SampleReceivedHandler(object sender, CustomEventArgs e)
        {
            WriteToLog("[SAMPLE] " + e.Message + " RowIndex: " + e.RowIndex);
        }

        public void TransferCompletedHandler(object sender, CustomEventArgs e)
        {
            WriteToLog("[COMPLETED] " + e.Message);
        }

        public void WarningRaisedHandler(object sender, CustomEventArgs e)
        {
            WriteToLog("[WARNING] " + e.Message + " RowIndex: " + e.RowIndex);
        }

        private void WriteToLog(string text)
        {
            File.AppendAllText(logPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + text + Environment.NewLine);
        }
    }
}
