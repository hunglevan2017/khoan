﻿using System;
using System.Collections.Generic;
using System.Text;

namespace KhoanNhaTrang.Model
{
    class Data
    {
        public long Id { get; set; }
        public double flow_rate { get; set; }
        public double fluid { get; set; }
        public double pressure { get; set; }
        public double wc { get; set; }

        public DateTime insert_date { get; set; }

        public long management_id { get; set; }

    }
}
