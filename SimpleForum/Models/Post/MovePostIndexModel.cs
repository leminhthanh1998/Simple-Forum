using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleForum.Models.Post
{
    public class MovePostIndexModel
    {
        public string PostId { get; set; }
        public int NewForumId { get; set; }
        public int OldForumId { get; set; }
        public IEnumerable<Data.Models.Forum> Forums { get; set; }

    }
}
