using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleForum.Models.Reply
{
    public class PostReplyModel
    {
        public int Id { get; set; }

        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public int AuthorRating { get; set; }
        public string AuthorImageUrl { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsMod { get; set; }
        public bool IsActive { get; set; }
        public bool CloseOrOpen { get; set; }//True la mo
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Nội dung bình luận phải có!")]
        [StringLength(10000, ErrorMessage = "{0} phải có ít nhất {2} kí tự và tối đa {1} kí tự.", MinimumLength = 10)]
        [Display(Name = "Nội dung")]
        public string ReplyContent { get; set; }

        public string Slug { get; set; }
        public int PostId { get; set; }
        public string PostTitle { get; set; }
        public string PostContent { get; set; }
        public int NumberOfReply { get; set; }
        public bool IsReview { get; set; }
        public string ForumName { get; set; }
        public string ForumImageUrl { get; set; }
        public int ForumId { get; set; }

    }
}
