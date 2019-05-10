using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SimpleForum.Areas.Admin.Models.ModManager;
using SimpleForum.Data;
using SimpleForum.Data.Models;

namespace SimpleForum.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ModManagerController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationUser _userService;

        public ModManagerController(UserManager<ApplicationUser> userManager, IApplicationUser applicationUser)
        {
            _userManager = userManager;
            _userService = applicationUser;
        }

        public IActionResult Index()
        {
            var mods = _userService.GetAll()
                .Where(u =>  _userManager.GetRolesAsync(u).Result.Contains("Mod"))
                .OrderByDescending(u=>u.UserName);

            var model = new ModManagerModel
            {
                User = mods
            };
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddMod(string email)
        {
            var user = _userService.GetByEmail(email);

            if(user!=null)
                await _userManager.AddToRoleAsync(user, "Mod");

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> RemoveMod(string id)
        {
            var user = _userService.GetById(id);

            if (user != null)
                await _userManager.RemoveFromRoleAsync(user, "Mod");

            return RedirectToAction("Index");
        }
    }
}