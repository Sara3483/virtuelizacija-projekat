using System;
using System.ServiceModel;
using VirtuelizacijaProjekat.Events;
using VirtuelizacijaProjekat.Subscribers;
using VirtuelizacijaProjekat.Analytics;

namespace VirtuelizacijaProjekat
{
    class Program
    {
        static void Main(string[] args)
        {
            TransferEventPublisher transferEvent = new TransferEventPublisher();

            ConsoleTransferSubscriber consoleSubscriber = new ConsoleTransferSubscriber();
            FileTransferSubscriber fileSubscriber = new FileTransferSubscriber("transfer_events.log");

            transferEvent.OnTransferStarted += consoleSubscriber.TransferStartedHandler;
            transferEvent.OnSampleReceived += consoleSubscriber.SampleReceivedHandler;
            transferEvent.OnTransferCompleted += consoleSubscriber.TransferCompletedHandler;
            transferEvent.OnWarningRaised += consoleSubscriber.WarningRaisedHandler;

            transferEvent.OnTransferStarted += fileSubscriber.TransferStartedHandler;
            transferEvent.OnSampleReceived += fileSubscriber.SampleReceivedHandler;
            transferEvent.OnTransferCompleted += fileSubscriber.TransferCompletedHandler;
            transferEvent.OnWarningRaised += fileSubscriber.WarningRaisedHandler;

            PhaseAngleEventPublisher phaseAngleEvent = new PhaseAngleEventPublisher();
            ConsolePhaseAngleSubscriber consolePhaseSubscriber = new ConsolePhaseAngleSubscriber();
            FilePhaseAngleSubscriber filePhaseSubscriber = new FilePhaseAngleSubscriber("phase_angle.log");
            phaseAngleEvent.OnPhaseAngleShift += consolePhaseSubscriber.PhaseAngleShiftHandler;
            phaseAngleEvent.OnPhaseAngleShift += filePhaseSubscriber.PhaseAngleShiftHandler;
            PhaseAngleProcessor phaseAngleProcessor = new PhaseAngleProcessor(phaseAngleEvent);

            ReactiveRatioEventPublisher reactiveRatioEvent = new ReactiveRatioEventPublisher();
            ConsoleReactiveRatioSubscriber consoleReactiveRatioSubscriber = new ConsoleReactiveRatioSubscriber();
            FileReactiveRatioSubscriber fileReactiveRatioSubscriber = new FileReactiveRatioSubscriber("reactive_ratio.log");
            reactiveRatioEvent.OnReactiveRatioOutOfBounds += consoleReactiveRatioSubscriber.ReactiveRatioOutOfBoundsHandler;
            reactiveRatioEvent.OnReactiveRatioWarning += consoleReactiveRatioSubscriber.ReactiveRatioWarningHandler;
            reactiveRatioEvent.OnReactiveRatioOutOfBounds += fileReactiveRatioSubscriber.ReactiveRatioOutOfBoundsHandler;
            reactiveRatioEvent.OnReactiveRatioWarning += fileReactiveRatioSubscriber.ReactiveRatioWarningHandler;
            ReactiveRatioProcessor reactiveRatioProcessor = new ReactiveRatioProcessor(reactiveRatioEvent);

            EisService service = new EisService(transferEvent, phaseAngleProcessor, reactiveRatioProcessor);

            ServiceHost host = new ServiceHost(service);

            try
            {
                host.Open();
                Console.WriteLine("WCF service running...");
                Console.ReadLine();
                host.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                host.Abort();
            }
        }
    }
}
