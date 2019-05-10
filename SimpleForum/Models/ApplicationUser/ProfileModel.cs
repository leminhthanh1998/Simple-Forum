using Microsoft.AspNetCore.Http;
using System;

namespace SimpleForum.Models.ApplicationUser
{
    public class ProfileModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string UserRating { get; set; }
        public string ProfileImageUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsComfirmEmail { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsMod { get; set; }
        public int PostCount { get; set; }


        public DateTime DateJoined { get; set; }
        public IFormFile ImageUpload { get; set; }
    }
}