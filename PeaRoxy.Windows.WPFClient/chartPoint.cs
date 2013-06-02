using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PeaRoxy.Windows.WPFClient
{
    public class chartPoint
    {
        public TimeSpan Time { get; set; }

        public double Data { get; set; }

        public chartPoint(double data)
        {
            Data = data;
            Time = new TimeSpan(Environment.TickCount);
        }
    }
}
