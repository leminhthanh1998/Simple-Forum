using SimpleForum.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleForum.Data
{
    public interface IPostReply
    {
        PostReply GetById(int id);
        Task Edit(int id, string message);
        Task Delete(int id);
    }
}
