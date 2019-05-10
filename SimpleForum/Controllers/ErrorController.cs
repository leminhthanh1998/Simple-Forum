using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SimpleForum.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Route("Error/{statusCode}")]
        public IActionResult HandleErrorCode(int statusCode)
        {
            string statusmessage = "";
            switch (statusCode)
            {
                case 400:
                    statusmessage = "Bad request: Dường như bạn đã nhập sai cú pháp đường dẫn";
                    break;
                case 403:
                    statusmessage = "Truy cập bị cấm";
                    break;
                case 404:
                    statusmessage = "Không tìm thấy trang";
                    break;
                case 408:
                    statusmessage = "Máy chủ đã hết thời gian chờ cho yêu cầu này";
                    break;
                case 500:
                    statusmessage = "Internal Server Error - Server có vấn đề cmn rồi";
                    break;
                case 406://Loi nay tui tu che :v
                    statusmessage = "Hình bạn upload không đúng!";
                    break;
                default:
                    statusmessage = "Éc éc, có lỗi gì đó rồi, chắc do bạn xui đấy :))";
                    break;
            }

            ViewBag.ErrorCode = statusCode;
            ViewBag.ErrorMessage = statusmessage;

            return View();
        }
    }
}
