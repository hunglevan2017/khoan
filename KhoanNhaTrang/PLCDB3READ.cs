using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KhoanNhaTrang
{
    public class PLCDB3READ
    {
        protected PLCDB3READ()
        {

        }
        private static PLCDB3READ _instance;
        public static PLCDB3READ Instance()
        {

            if (_instance == null)
            {
                _instance = new PLCDB3READ();

            }
            return _instance;

        }
        // AI 01
        public double data1 { get; set; }
        public double data2 { get; set; }
        public double data3 { get; set; }
        public double data4 { get; set; }
        public double data5 { get; set; }
        public double Raw_low_1 { get; set; }
        public double Raw_high_1 { get; set; }
        public double Scale_value_low_1 { get; set; }
        public double Scale_value_high_1 { get; set; }
        // AI 02
        public double Raw_low_2 { get; set; }
        public double Raw_high_2 { get; set; }
        public double Scale_value_low_2 { get; set; }
        public double Scale_value_high_2 { get; set; }
        // AI 03
        public double Raw_low_3 { get; set; }
        public double Raw_high_3 { get; set; }
        public double Scale_value_low_3 { get; set; }
        public double Scale_value_high_3 { get; set; }
        public double total_1 { get; set; }
        public double total_2 { get; set; }
        public double total_3 { get; set; }
        public double total_4 { get; set; }
        public double total_5 { get; set; }
        public double total_6 { get; set; }
        public double total_7 { get; set; }
        public double totalxm_1 { get; set; }
        public double totalxm_2 { get; set; }
        public double totalxm_3 { get; set; }
        public double totalxm_4 { get; set; }
        public double totalxm_5 { get; set; }
        public double totalxm_6 { get; set; }
        public double tytrong { get; set; }
        public double tytrongloadcell { get; set; }
        public short temp { get; set; }
    }
}
