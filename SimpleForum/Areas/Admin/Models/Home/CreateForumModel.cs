﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleForum.Areas.Admin.Models.Home
{
    public class CreateForumModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile File { get; set; }
    }
}
