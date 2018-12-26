using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllianceManager
{
    public class ActivityAttendInfo
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string ActivityName { get; set; }
        public int UserId { get; set; }
    }
}
