
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace SerialPortTranslator
{
    internal class Program
    {
        private static readonly CommunicationManager Cm1 = new CommunicationManager("9600", "None", "1", "8", "COM1",
                                                                                    true, true);

        private static readonly CommunicationManager Cm2 = new CommunicationManager("9600", "None", "1", "8", "COM2",
                                                                                    true, true);

        private static bool _manuflag;

      

        private static void MessageTranslator(object sender, EventArgs e)
        {
            if (!(e is SerialPort1Capture)) return;
            var E = (SerialPort1Capture) e;
            var strArray = Regex.Split(E.Holder, "([0-9][0-9][0-9]F)|(H)|(B)");
            _manuflag = false;
            int length = strArray.Length;
            for (int index = 0; index < length - 1; ++index)
            {
                if (strArray[index + 1] != "H") continue;
                strArray[index] = strArray[index] + "H";
                strArray[index + 1] = string.Empty;
            }
            for (int index = 0; index < length; ++index)
            {
                strArray[index] = strArray[index].Replace(" ", "");
                Console.WriteLine("{0} ", strArray[index]);
                switch (strArray[index])
                {
                    case "388F":
                        _manuflag = true;
                        Cm2.WriteData(strArray[index]);
                        strArray[index] = string.Empty;
                        Thread.Sleep(150);
                        Cm2.WriteData("C");
                        Console.WriteLine("C");
                        Thread.Sleep(100);
                        Cm2.WriteData("G");
                        Console.WriteLine("G");
                        Thread.Sleep(100);
                        Cm2.WriteData("G");
                        Console.WriteLine("G");
                        Thread.Sleep(100);
                        Cm2.WriteData("7");
                        Console.WriteLine("7");
                        Thread.Sleep(100);
                        Cm2.WriteData("G");
                        Console.WriteLine("G");
                        Thread.Sleep(100);
                        Cm2.WriteData("C");
                        Console.WriteLine("C");
                        Thread.Sleep(100);
                        break;
                    default:
                        if (Regex.Match(strArray[index], "[0-9][0-9][0-9]F", RegexOptions.IgnoreCase).Success ||
                            strArray[index] == "B")
                        {
                            _manuflag = false;
                            Console.WriteLine(strArray[index]);
                            Cm2.OpenPort();
                            Cm2.WriteData(strArray[index]);
                            Thread.Sleep(200);
                            strArray[index] = string.Empty;
                            Cm2.ClosePort();
                        }
                        else if (_manuflag)
                        {
                            strArray[index] = strArray[index].Replace('0', 'A');
                            if (!string.IsNullOrEmpty(strArray[index]))
                            {
                                Cm2.OpenPort();
                                Cm2.WriteData(strArray[index]);
                                strArray[index] = string.Empty;
                                Thread.Sleep(100);
                                Cm2.ClosePort();
                            }
                        }
                        break;
                }
            }
        }

        private static void Main()
        {
            Cm1.OpenPort();
            Cm1.serialPort1Capture += MessageTranslator;
           
            var flag = false;
            while (!flag)
            {
                if (!Console.KeyAvailable || Console.ReadLine() != "Quit") continue;
                flag = true;
                Cm1.ClosePort();
                Cm1.Dispose();
                Cm2.Dispose();
            }
        }
    }
}