using SimpleForum.Models.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleForum.Models.ApplicationUser
{
    public class FindAllPostModel
    {
        public string UserName { get; set; }
        public string Id { get; set; }
        public IEnumerable<PostListingModel> Posts { get; set; }
    }
}
