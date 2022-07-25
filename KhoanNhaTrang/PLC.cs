using S7.Net;
namespace KhoanNhaTrang
{
    public class PLC
    {
        Plc _plc = new Plc(CpuType.S71200, Properties.Settings.Default.IPPLC, 0, 1);
        private static PLC _instance;
        public bool PLC_connected = false;

        protected PLC()
        {

        }

        public static PLC Instance()
        {
            if (_instance == null)
            {
                _instance = new PLC();
            }
            return _instance;
        }
         public bool Open()
        {
            if (_plc.IsConnected)
                return true;
            if (_plc.Open() == ErrorCode.NoError)
            {
                PLC_connected = true;
                return true;

            }
            else 
            {
                PLC_connected = false;
                return false;
            }
        }
        
        public void Close()
        {
            PLC_connected = false;
            _plc.Close();
        }

        public void SetBit (string Address)
        {
            if (PLC_connected)
            {
                _plc.Write(Address, 1);
            }
        }
        public void ResetBit(string Address)
        {
            if (PLC_connected)
            {
                _plc.Write(Address, 0);
            }
        }

        public void WriteInt(string Address, short Value)
        {
            if (PLC_connected)
            {
                _plc.Write(Address, Value);
            }
        }
        public void WriteDInt(string Address, int Value)
        {
            if (PLC_connected)
            {
                _plc.Write(Address, Value);
            }
        }
        public void WriteTimer(string Address, int Value)
        {
            if (PLC_connected)
            {
                _plc.Write(Address, Value);
            }
        }
        public void WriteReal(string Address, double Value)
        {
            if (PLC_connected)
            {
                _plc.Write(Address, Value);
            }
        }
        public void ReadBit(string Address)
        {
            if (PLC_connected)
            {
                _plc.Read(Address);
            }
        }
        public void ReadClass (object cl, int DB)
        {
            if (PLC_connected)
            {
                _plc.ReadClass(cl, DB);
            }
        }
        public void WriteClass(object cl, int DB)
        {
            if (PLC_connected)
            {
                _plc.WriteClass(cl, DB);
            }
        }
    }

}
