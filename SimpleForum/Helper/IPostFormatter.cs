using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleForum.Helper
{
    public interface IPostFormatter
    {
        string Prettify(string postContent);
    }
}
