using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KhoanNhaTrang
{
    public class PLCDB1Read
    {
        protected PLCDB1Read()
        {

        }
        private static PLCDB1Read _instance;
        public static PLCDB1Read Instance()
        {

            if (_instance == null)
            {
                _instance = new PLCDB1Read();

            }
            return _instance;
        }
        public short RawAI1 { get; set; }
        public short RawAI2 { get; set; }
        public short RawAI3 { get; set; }



        public double flow_rate { get; set; }
        public double pressure { get; set; }
        public double wc { get; set; }
        public double fluid { get; set; }
        public bool Outstart { get; set; }
        public short wc_1 { get; set; }
        public double cement_total { get; set; }
        public short temp1 { get; set; }

    }
}
