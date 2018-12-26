using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AllianceManager
{
    public class CareerInfo
    {
        public int Career { get; set; }
        public PositionTypeEnum Position { get; set; }

        public Brushes Color { get; set; }


    }

    public enum PositionTypeEnum
    {
        Tank, DPS, Milker
    }
}
