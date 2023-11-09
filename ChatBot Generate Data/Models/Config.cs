using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot_Generate_Data.Models
{
    public class Config
    {
        public int NumberTasks { get; set; }
        public int NumberErrorVariations { get; set; }
        public int TurnAgain {get; set; }
        public int TimeWaitLong { get; set; }
        public int TimeWaitShort { get; set; }
        public string Url { get; set; }
    }
}
