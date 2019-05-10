using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleForum.Hubs
{
    public class UserHub : Hub
    {
        private static int countOnline = 0;

        public override Task OnConnectedAsync()
        {
            countOnline++;
            SendInfoOnline(countOnline).Wait();
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            countOnline--;
            SendInfoOnline(countOnline).Wait();
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendInfo(string location, string ip)
        {
            string name;
            string userID="";
            var user = Context.User;

            if (user.Identity.IsAuthenticated)
            {
                name = user.Identity.Name;
            }
            else
            {
                name = "Guest";
            }

            var claimsIdentity = Context.User.Identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                var userIdClaim = claimsIdentity.Claims
                    .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

                if (userIdClaim != null)
                {
                    userID = userIdClaim.Value;
                }
            }

            await SendInfoAdmin(name, userID, location, ip);
        }

        public async Task SendInfoAdmin(string username, string userid, string location, string ip)
        {            
            string linkProfile = $"https://simpleforum.azurewebsites.net/Profile/Detail/{userid}";//Tam thoi set cung ten mien nhu vay
            if (userid == "")
                linkProfile = "https://simpleforum.azurewebsites.net/Profile";
            string linkLocation = $"https://simpleforum.azurewebsites.net{location}";

            var htmldata = $"<tr class=\"userRow\">\r\n  <td>\r\n  <a href=\"{linkProfile}\" target=\"_blank\">{username}</a>  </td>\r\n   <td>\r\n  <a href=\"http://ipinfolookup.com/{ip}\" target=\"_blank\">{ip}</a>  </td>\r\n <td>\r\n  <a href=\"{linkLocation}\" target=\"_blank\">{location}</a>      </td>\r\n          </tr>";

            await Clients.All.SendAsync("ReceiveInfoUser", htmldata);
        }

        public async Task SendInfoOnline(int count)
        {           
            await Clients.All.SendAsync("ReceiveInfoOnline", count);
        }
    }
}
