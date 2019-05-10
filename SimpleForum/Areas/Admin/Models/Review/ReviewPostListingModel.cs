using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleForum.Areas.Admin.Models.Review
{
    public class ReviewPostListingModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public DateTime Created { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string UserImgUrl { get; set; }
    }
}
