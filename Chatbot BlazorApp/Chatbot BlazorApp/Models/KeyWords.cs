using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatbot_BlazorApp.Models
{
    [Index(nameof(Keyword), nameof(KeywordNotToneMarks))]
    public class Keywords
    {
        [Key]
        public int KeywordID { get; set; }
        [StringLength(50, MinimumLength = 2, ErrorMessage = "{0} phải dài từ {2} đến {1} kí tự.")]
        [Column(TypeName = "nvarchar")]
        [Required(ErrorMessage = "{0} phải nhập.")]
        [DisplayName("Từ khoá")]
        public string Keyword { get; set; }
        [StringLength(50, MinimumLength = 2, ErrorMessage = "{0} phải dài từ {2} đến {1} kí tự.")]
        [Column(TypeName = "nvarchar")]
        [Required(ErrorMessage = "{0} phải nhập.")]
        [DisplayName("Từ khoá không dấu")]
        public string KeywordNotToneMarks { get; set; }
        public int SplitContentID { get; set; }
        [ForeignKey("SplitContentID")]
        [Required(ErrorMessage = "{0} phải nhập.")]
        public SplitContents SplitContents { get; set; }
    }
}
