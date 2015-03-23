

using System;
using System.IO.Ports;
using System.Text;

namespace SerialPortTranslator
{
    internal class CommunicationManager : IDisposable
    {
        public delegate void MyEventHandler(object sender, SerialPort1Capture serialPort1Capture);

        public enum TransmissionType
        {
            Text,
            Hex,
            
        }

        private static string _dataread = string.Empty;
        private readonly string _baudRate;
        private readonly string _dataBits;
        private readonly string _parity;
        private readonly string _portName;
        private readonly string _stopBits;

        private SerialPort _comPort = new SerialPort();

        static CommunicationManager()
        {
        }

        public CommunicationManager(string baud, string par, string sBits, string dBits, string name, bool dtr, bool rts)
        {
            _baudRate = baud;
            _parity = par;
            _stopBits = sBits;
            _dataBits = dBits;
            _portName = name;
            DtrEnable = dtr;
            RtsEnable = rts;
            CurrentTransmissionType = TransmissionType.Text;
            _comPort.DataReceived += ComPortDataReceived;
        }

        public TransmissionType CurrentTransmissionType { get; set; }

        public bool DtrEnable { get; set; }

        public bool RtsEnable { get; set; }

        public event EventHandler serialPort1Capture;


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || _comPort == null)
                return;
            _comPort.Dispose();
            _comPort = null;
        }

        public void WriteData(string msg)
        {
            switch (CurrentTransmissionType)
            {
                case TransmissionType.Text:
                    if (!_comPort.IsOpen)
                        _comPort.Open();
                    _comPort.Write(msg);
                    DisplayData(msg + "\n");
                    break;
                case TransmissionType.Hex:
                    try
                    {
                        byte[] numArray = HexToByte(msg);
                        _comPort.Write(numArray, 0, numArray.Length);
                        DisplayData(ByteToHex(numArray) + "\n");
                        break;
                    }
                    catch (FormatException ex)
                    {
                        DisplayData(ex.Message);
                        break;
                    }
                default:
                    if (!_comPort.IsOpen)
                        _comPort.Open();
                    _comPort.Write(msg);
                    DisplayData(msg + "\n");
                    break;
            }
        }

        private static byte[] HexToByte(string msg)
        {
            msg = msg.Replace(" ", "");
            var numArray = new byte[msg.Length/2];
            int startIndex = 0;
            while (startIndex < msg.Length)
            {
                numArray[startIndex/2] = Convert.ToByte(msg.Substring(startIndex, 2), 16);
                startIndex += 2;
            }
            return numArray;
        }

        [STAThread]
        private void DisplayData(string msg)
        {
            lock (_dataread)
            {
                _dataread = _dataread + msg;
                if (!_dataread.Contains("H") && !_dataread.Contains("B"))
                    return;
                var local0 = new SerialPort1Capture(_dataread);
                EventHandler sp1Capture = serialPort1Capture;
                if (sp1Capture != null)
                    sp1Capture(this, local0);
                _dataread = string.Empty;
            }
        }

        public bool ClosePort()
        {
            try
            {
                if (_comPort.IsOpen)
                    _comPort.Close();
                return true;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public bool OpenPort()
        {
            try
            {
                if (_comPort.IsOpen)
                    _comPort.Close();
                _comPort.BaudRate = int.Parse(_baudRate);
                _comPort.DataBits = int.Parse(_dataBits);
                _comPort.StopBits = (StopBits) Enum.Parse(typeof (StopBits), _stopBits);
                _comPort.Parity = (Parity) Enum.Parse(typeof (Parity), _parity);
                _comPort.PortName = _portName;
                _comPort.RtsEnable = RtsEnable;
                _comPort.DtrEnable = DtrEnable;
                _comPort.Open();
                Console.WriteLine("{0} Port opened at " + DateTime.Now + "\n", _comPort.PortName);
                return true;
            }
            catch (Exception ex)
            {
                DisplayData(ex.Message);
                return false;
            }
        }

        private void ComPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            switch (CurrentTransmissionType)
            {
                case TransmissionType.Text:
                    DisplayData(_comPort.ReadExisting());
                    break;
                case TransmissionType.Hex:
                    int bytesToRead = _comPort.BytesToRead;
                    var numArray = new byte[bytesToRead];
                    _comPort.Read(numArray, 0, bytesToRead);
                    DisplayData(ByteToHex(numArray) + "\n");
                    break;
                default:
                    DisplayData(_comPort.ReadExisting());
                    break;
            }
        }

        private static string ByteToHex(byte[] comByte)
        {
            if (comByte == null) throw new ArgumentNullException("comByte");
            var stringBuilder = new StringBuilder(comByte.Length*3);
            foreach (byte num in comByte)
                stringBuilder.Append(Convert.ToString(num, 16).PadLeft(2, '0').PadRight(3, ' '));
            return (stringBuilder).ToString().ToUpper();
        }
    }
}