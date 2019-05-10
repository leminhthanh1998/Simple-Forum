using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleForum.Models.Reply
{
    public class EditReplyModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Nội dung bình luận phải có!")]
        [StringLength(10000, ErrorMessage = "{0} phải có ít nhất {2} kí tự và tối đa {1} kí tự.", MinimumLength = 10)]
        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        public bool IsActive { get; set; }
    }
}
