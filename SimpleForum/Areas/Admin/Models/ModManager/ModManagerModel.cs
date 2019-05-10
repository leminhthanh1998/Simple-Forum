using SimpleForum.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleForum.Areas.Admin.Models.ModManager
{
    public class ModManagerModel
    {
        public string Email { get; set; }
        public IEnumerable<ApplicationUser> User { get; set; }
    }
}
