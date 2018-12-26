using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllianceManager
{
    public class UserInfo
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string UserId { get; set; }
        public int Sex { get; set; }
        public int Career { get; set; }
        public int Duty { get; set; }
        public string Pinyin { get; set; }
        public string ShortName { get; set; }
        public bool IsRemoved { get; set; }
        public string Description { get; set; }
    }
}
