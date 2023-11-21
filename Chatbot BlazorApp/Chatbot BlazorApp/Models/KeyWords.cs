using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot_Generate_Data.Models
{
    public class Keywords
    {
        public int IdKeyword { get; set; }
        public string Keyword { get; set; }
        public string KeywordNotToneMarks { get; set; }
        public SplitContents SplitContents { get; set; }
    }
}
