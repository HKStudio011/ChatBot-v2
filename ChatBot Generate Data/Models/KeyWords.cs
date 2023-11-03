using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot_Generate_Data.Models
{
    public class KeyWords
    {
        public int IdKeyWord { get; set; }
        public string KeyWord { get; set; }
        public string KeyWordNotToneMarks { get; set; }
        public SplitContents SplitContents { get; set; }
        public List<ErrorVariations> ErrorVariations { get; set; }
    }
}
