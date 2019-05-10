using SimpleForum.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleForum.Data
{
    public interface IApplicationUser
    {
        ApplicationUser GetById(string id);
        ApplicationUser GetByEmail(string email);
        IEnumerable<ApplicationUser> GetAll();
        Task IncrementRating(string id);
        Task Add(ApplicationUser user);
        Task BanOrUnban(ApplicationUser user);
        Task SetProfileImage(string id, string uri);
        Task BumpRating(string userId, Type type);
    }
}
