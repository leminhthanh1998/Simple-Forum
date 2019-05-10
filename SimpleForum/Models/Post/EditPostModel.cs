using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleForum.Models.Post
{
    public class EditPostModel
    {
        [Required(ErrorMessage = "Nội dung tiêu đề phải có!")]
        [StringLength(200, ErrorMessage = "{0} phải có ít nhất {2} kí tự và tối đa {1} kí tự.", MinimumLength = 3)]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Nội dung bài viết phải có!")]
        [StringLength(50000, ErrorMessage = "{0} phải có ít nhất {2} kí tự và tối đa {1} kí tự.", MinimumLength = 10)]
        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        public string PostID { get; set; }
        public bool IsActive { get; set; }
    }
}
