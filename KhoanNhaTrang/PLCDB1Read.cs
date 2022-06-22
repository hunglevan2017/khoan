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

        public float flow_rate { get; set; }
        public float fluid { get; set; }
        public float pressure { get; set; }
        public float wc { get; set; }

    }
}
