using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace UniversityCorrespondencePortal.Controllers
{
    public class HelpController : Controller
    {
        // GET: Help
        [HttpPost]
        public JsonResult GeneratePasswordHash(string password)
        {
            if (string.IsNullOrEmpty(password))
                return Json(new { hash = "" });

            var hash = PasswordHelper.HashPassword(password);
            return Json(new { hash = hash });
        }
    }
}