using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KhoanNhaTrang
{
    public class PLCDB2Write
    {
        protected PLCDB2Write()
        {

        }
        private static PLCDB2Write _instance;
        public static PLCDB2Write Instance()
        {

            if (_instance == null)
            {
                _instance = new PLCDB2Write();

            }
            return _instance;

        }
        // AI 01
        public int Raw_low_1 { get; set; }
        public int Raw_high_1 { get; set; }
        public int Scale_value_low_1 { get; set; }
        public int Scale_value_high_1 { get; set; }
        // AI 02
        public int Raw_low_2 { get; set; }
        public int Raw_high_2 { get; set; }
        public int Scale_value_low_2 { get; set; }
        public int Scale_value_high_2 { get; set; }
        // AI 03
        public int Raw_low_3 { get; set; }
        public int Raw_high_3 { get; set; }
        public int Scale_value_low_3 { get; set; }
        public int Scale_value_high_3 { get; set; }
        // AI 04
      
        public bool Start { get; set; }
        public bool Stop { get; set; }
        public bool Reset { get; set; }
        public bool test { get; set; }
        public short temp { get; set; }
    }
}
