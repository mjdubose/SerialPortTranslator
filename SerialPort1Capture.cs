

using System;

namespace SerialPortTranslator
{
  internal class SerialPort1Capture : EventArgs
  {
    public string Holder { get; set; }

    public SerialPort1Capture(string x)
    {
      Holder = x;
    }
  }
}
