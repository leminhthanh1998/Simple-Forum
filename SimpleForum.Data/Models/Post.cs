using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SimpleForum.Data.Models
{
    public class Post
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public DateTime Created { get; set; }
        public bool CloseOrOpen { get; set; } = true;//True tuc la bai viet duoc mo
        public bool IsReview { get; set; } = true; //True tuc la se kiem duyet
        public string Slug { get; set; }

        [ForeignKey("ForumID")]
        public virtual Forum Forum { get; set; }
        public int ForumID { get; set; }

        public virtual ApplicationUser User { get; set; }


        public virtual IEnumerable<PostReply> Replies { get; set; }
    }
}
