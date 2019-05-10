using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleForum.Data.Models
{
    public class Forum
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public DateTime Created { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }

        public virtual IEnumerable<Post> Posts { get; set; }
    }
}
