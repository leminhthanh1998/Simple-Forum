using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleForum.Models.Reply
{
    public class DeleteReplyModel
    {
        public int ReplyId { get; set; }
        public bool IsActive { get; set; }
        public int PostID { get; set; }
    }
}
