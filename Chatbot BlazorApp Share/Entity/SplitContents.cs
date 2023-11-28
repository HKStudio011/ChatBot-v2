using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Chatbot_BlazorApp_Share.Entity
{
    public class SplitContents
    {
        [Key]
        public int SplitContentID { get; set; }
        [StringLength(1000, MinimumLength = 2, ErrorMessage = "{0} phải dài từ {2} đến {1} kí tự.")]
        [Column(TypeName = "nvarchar")]
        [Required(ErrorMessage = "{0} phải nhập.")]
        [DisplayName("Nội dung đã tách")]
        public string SplitContent { get; set; }
        public int ContentID { get; set; }
        [ForeignKey("ContentID")]
        [Required(ErrorMessage = "{0} phải nhập.")]
        public Contents Content { get; set; }
        public List<Keywords>? KeyWords { get; set; }
    }
}
