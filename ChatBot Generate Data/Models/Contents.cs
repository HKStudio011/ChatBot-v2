using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot_Generate_Data.Models
{
    public class Contents
    {
        public int IdContent { get; set; }
        public string Content { get; set; }
        public List<SplitContents> SplitContents { get; set; }
    }
}
