using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleForum.Helper
{
    public interface IFriendlyUrl
    {
        string GetFriendlyTitle(string Title, bool remap=true, int number=80);
    }
}
