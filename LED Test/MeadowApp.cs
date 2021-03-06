﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Hardware;

namespace LED_Test
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        //   RgbPwmLed onboardLed;
        IDigitalOutputPort DO00;
        IDigitalOutputPort DO01;
        IDigitalOutputPort DO02;
        IDigitalOutputPort DO03;
        IDigitalOutputPort DO04;
        IDigitalOutputPort DO05;

        IDigitalInputPort DI00;

        IAnalogInputPort analogIn;

        float analogValue = 3.3f;

        ConcurrentQueue<string> consoleStatements = new ConcurrentQueue<string>();


        enum Mode
        {
            CountUp,
            Walk
        };

        enum WalkDirection
        {
            Up,
            Down
        };

        Mode mode;
        WalkDirection walkDirection;


        public MeadowApp()
        {
            Initialize();

            MainLoop(100);
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");
            DO00 = Device.CreateDigitalOutputPort(Device.Pins.D00, false);
            DO01 = Device.CreateDigitalOutputPort(Device.Pins.D01, false);
            DO02 = Device.CreateDigitalOutputPort(Device.Pins.D02, false);
            DO03 = Device.CreateDigitalOutputPort(Device.Pins.D03, false);
            DO04 = Device.CreateDigitalOutputPort(Device.Pins.D04, false);
            DO05 = Device.CreateDigitalOutputPort(Device.Pins.D05, false);

            DI00 = Device.CreateDigitalInputPort(Device.Pins.D10);

            mode = Mode.CountUp;
            walkDirection = WalkDirection.Up;
            Console.WriteLine($"Mode: {mode}");

            analogIn = Device.CreateAnalogInputPort(Device.Pins.A02);
            analogIn.StartSampling();

        }
        void MainLoop(int maxLoopDurationMillis)
        {
            byte value = 1;

            bool previousSwitchState = false;

           // float analogValue;
            float speedPercentage;

            Task.Run(ReadAI);
          //  Task.Run(ConsoleOut);

            while (true)
            {
               // analogValue = ReadAI();
                speedPercentage = analogValue / 3.3f * 100f;
             //   consoleStatements.Enqueue($"Analogue value: {analogValue} V\tSpeed: {speedPercentage} %");
                
                // Console.WriteLine($"Analogue value: {analogValue} V\tSpeed: {speedPercentage} %");

                if (SwitchRisingEdge(previousSwitchState))
                {
                    Console.WriteLine($"Rising edge on switch");
                    previousSwitchState = true;
                    if (mode == Mode.CountUp)
                    {
                        mode = Mode.Walk;
                    }
                    else
                    {
                        mode = Mode.CountUp;
                    }
                    value = 1;
                    Console.WriteLine($"Setting mode to: {mode}");
                }

                if (SwitchFallingEdge(previousSwitchState))
                {
                    Console.WriteLine($"Falling edge on switch");
                    previousSwitchState = false;
                }


                switch (mode)
                {
                    case Mode.Walk:
                        Walk(ref value);
                        break;

                    case Mode.CountUp:
                        CountUp(ref value);
                        break;

                    default:
                        break;
                }

                var sleepDuration = maxLoopDurationMillis * speedPercentage / 100f;
                Thread.Sleep((int)sleepDuration);
            }
        }

        private async Task ConsoleOut()
        {
            string consoleStatement;
            while (true)
            {
         //       if (consoleStatements.TryDequeue(out consoleStatement))
         //       {
          //          Console.WriteLine(consoleStatement);
          //      }
            }
        }

        private async Task ReadAI()
        {
            while (true)
            {
                await analogIn.Read(1,1);
                analogValue = analogIn.Voltage;
            }

            // return analogIn.Voltage;
        }

        private bool SwitchRisingEdge(bool previousState)
        {
            bool newState = DI00.State;

            if (newState && !previousState)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool SwitchFallingEdge(bool previousState)
        {
            bool newState = DI00.State;

            if (!newState && previousState)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private bool BitMask(byte value, byte bit)
        {
            return (value & bit) > 0;
        }

        private void CountUp(ref byte value)
        {
            DO00.State = BitMask(value, 1);
            DO01.State = BitMask(value, 2);
            DO02.State = BitMask(value, 4);
            DO03.State = BitMask(value, 8);
            DO04.State = BitMask(value, 16);
            DO05.State = BitMask(value, 32);

            value++;

            if (value >= 64)
            {
                value = 0;
            }
        }


        private void Walk(ref byte value)
        {
            DO00.State = BitMask(value, 1);
            DO01.State = BitMask(value, 2);
            DO02.State = BitMask(value, 4);
            DO03.State = BitMask(value, 8);
            DO04.State = BitMask(value, 16);
            DO05.State = BitMask(value, 32);

            int valueint = value;

            switch (walkDirection)
            {
                case WalkDirection.Up:
                    valueint <<= 1;
                    if (valueint >= 32)
                    {
                        walkDirection = WalkDirection.Down;
                    }
                    break;

                case WalkDirection.Down:
                    valueint >>= 1;
                    if (valueint <= 1)
                    {
                        walkDirection = WalkDirection.Up;
                    }
                    break;

                default:
                    break;
            }

            value = (byte)valueint;
        }

    }
}
