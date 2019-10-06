using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VersionDoc.Models;
using Microsoft.AspNetCore.Mvc;

namespace VersionDoc.Controllers
{
    public class PublicDocumentsController : Controller
    {
        private versionDocContext _vdContext;
        private UserManager<ApplicationUser> _userManager;
        private ApplicationUser user;

        public PublicDocumentsController(versionDocContext context, UserManager<ApplicationUser> userManager)
        {
            _vdContext = context;
            _userManager = userManager;
        }

        /* This controller is used to display all public files
         * It returns to view methods, one for all the files and one for the log for each of the file
         */ 
        [HttpGet]
        public async Task<IActionResult> PublicDocuments()
        {
            var document = _vdContext.File.Include(c => c.User);
            return View(await document.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Log(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var log = _vdContext.Log.Include(c => c.File);
            ViewBag.id = id;
            return View(await log.ToListAsync());
        }

        [HttpPost]
        public IActionResult Log()
        {
            return View();
        }
    }
}