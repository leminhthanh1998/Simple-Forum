using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleForum.Models.ApplicationUser
{
    public class ApplicationUserModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string ProfileImageUrl { get; set; }
        public int Rating { get; set; }
        public bool IsActive { get; set; }
    }
}
