using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleForum.Areas.Admin.Models.Home
{
    public class HomeIndexModel
    {
        public int NumberOfPostNeedReview { get; set; }
        public IEnumerable<HomeListingModel> HomeList { get; set; }
    }
}
