using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot_Generate_Data.Models
{
    public class SplitContents
    {
        public int IdSplitContent { get; set; }
        public string SplitContent { get; set; }
        public Contents Content { get; set; }
        public List<Keywords> KeyWords { get; set; }
    }
}
