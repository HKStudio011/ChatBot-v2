using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatbot_BlazorApp_Share.Entity
{
    public class Contents
    {
        [Key]
        public int ContentID { get; set; }
        [StringLength(10000,MinimumLength =2,ErrorMessage ="{0} phải dài từ {2} đến {1} kí tự.")]
        [Column(TypeName = "nvarchar(max)")]
        [Required(ErrorMessage ="{0} phải nhập.")]
        [DisplayName("Nội dung")]
        public string Content { get; set; }
        public List<SplitContents> SplitContents { get; set; }
    }
}
