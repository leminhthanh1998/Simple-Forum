using Microsoft.AspNetCore.Http;
using SimpleForum.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleForum.Data
{
    public interface IForum
    {
        Forum GetByID(int id);
        IEnumerable<Forum> GetAll();
        IEnumerable<ApplicationUser> GetAllActiveUser();

        Task Create(string Title, string Description, IFormFile file);
        Task Delete(int forumID);
        Task UpdateForum(int id, string Title, string Description, IFormFile file);
    }
}
