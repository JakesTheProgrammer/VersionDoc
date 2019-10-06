using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using VersionDoc.Models.AccountViewModels;
using VersionDoc.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace VersionDoc.Controllers
{
    public class FinalizeUserController : Controller
    {
        private readonly versionDocContext _vdContext;
        private UserManager<ApplicationUser> _userManager;
        private IHostingEnvironment _hostingEnvironment;

        public FinalizeUserController(versionDocContext vdContext, UserManager<ApplicationUser> userManager, IHostingEnvironment hosting)
        {
            _vdContext = vdContext;
            _userManager = userManager;
            _hostingEnvironment = hosting;
        }

        [HttpGet]
        public IActionResult FinalizeReg()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FinalizeReg([Bind (include: "UserFirstName, UserLastName")]UserDetail userDetail)
        {
            try
            {
                if (userDetail.UserFirstName == null)
                    return RedirectToAction("ErrorUploading", "MyDocuments", new { msg = "Fill in the First name" });

                if (userDetail.UserLastName == null)
                    return RedirectToAction("ErrorUploading", "MyDocuments", new { msg = "Fill in the Last name" });

                if (ModelState.IsValid)
                {
                    var search_User = await _userManager.GetUserAsync(HttpContext.User);
                    string active_user_ID = search_User.Id;
                    string email = search_User.Email;
                    int last_Primary_Key = _vdContext.UserDetail.Max(item => item.UserId) + 1;

                    userDetail.UserId = last_Primary_Key;
                    userDetail.LoginId = active_user_ID;
                    userDetail.UserEmail = email;

                    _vdContext.UserDetail.Add(userDetail);
                    _vdContext.SaveChanges();

                    createDirectoryForNewUser(email);

                    return RedirectToAction("Index", "Home");
                }
            }
            catch(DataException)
            {
                ModelState.AddModelError("", "Could not finalize the registration information, try again");
            }
            return View();
        }

        private void createDirectoryForNewUser(string email)
        {
            string path_Main_Folder = _hostingEnvironment.WebRootPath;
            path_Main_Folder += "\\Documents\\" + email + "\\";

            string path_Public_Folder = path_Main_Folder + "Public_Documents\\";

            string path_Private_Folder = path_Main_Folder + "Private_Documents\\";

            string path_Shared_Folder = path_Main_Folder + "Shared_Documents\\";

            Directory.CreateDirectory(path_Main_Folder);
            Directory.CreateDirectory(path_Private_Folder);
            Directory.CreateDirectory(path_Public_Folder);
            Directory.CreateDirectory(path_Shared_Folder);
        }

        public ActionResult ErrorPageFinalize(string msg)
        {
            ViewBag.error = msg;
            return View();
        }
    }
}