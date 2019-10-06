using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using VersionDoc.Models;
using Microsoft.AspNetCore.Mvc;

namespace VersionDoc.Controllers
{
    public class MyDocumentsController : Controller
    {
        private versionDocContext _vdContext;
        private UserManager<ApplicationUser> _userManager;
        private IHostingEnvironment _hostingEnvironment;
        private ApplicationUser user;
        private string errorMsg = " ";

        public MyDocumentsController(versionDocContext context, UserManager<ApplicationUser> userManager, IHostingEnvironment hosting)
        {
            _vdContext = context;
            _userManager = userManager;
            _hostingEnvironment = hosting;
        }

        /* The following 5 view methods are used to display the information in table format
         * This also included log views for each file
         * The methods returns async lists of either the files or the logs 
         */ 

        [HttpGet]
        public async Task<IActionResult> MyDocuments()
        {
            var document = _vdContext.File.Include(c => c.User);
            return View(await document.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> MyPrivateDocuments()
        {
            var document = _vdContext.File.Include(c => c.User);
            return View(await document.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> SharedWithMe()
        {
            var shared_document = _vdContext.SharedOwnership.Include(c => c.Files);
            return View(await shared_document.Include(i => i.Users).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> PublicLog(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var log = _vdContext.Log.Include(c => c.File);
            ViewBag.id = id;
            return View(await log.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> PrivateLog(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var log = _vdContext.Log.Include(c => c.File);
            ViewBag.id = id;
            return View(await log.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> SharedLog(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var log = _vdContext.Log.Include(c => c.File);
            ViewBag.id = id;
            return View(await log.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Remove(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var getFile = await _vdContext.File.FindAsync(id);
            return View(getFile);
        }

        /* The remove method finds the path for the document, check if the file is shared, private or public
         * When the file is found it is deleted out of the database and out of the file directory
         */ 

        [HttpPost]
        public async Task<IActionResult> Remove(Guid? id, [Bind(include:"FileName, Filesize, FileUploadDate, FileType")] File files)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                string currentUserEmail = currentUser.Email;

                var user = await _vdContext.UserDetail.FirstOrDefaultAsync(i => i.UserEmail == currentUserEmail);
                int userID = user.UserId;

                var findPath = await _vdContext.File.FirstOrDefaultAsync(c => c.FileId == id);
                string path = findPath.FileDirectory;

                SharedOwnership shared = _vdContext.SharedOwnership.Find(userID, id);
                if (shared != null)
                {
                    _vdContext.SharedOwnership.Remove(shared);
                    _vdContext.SaveChanges();
                    deleteFileFromDirectory(path);
                }

                var log = await _vdContext.Log.FirstOrDefaultAsync(i => i.LogUploader == currentUserEmail);
                int logID = log.LogId;

                Log myLog = _vdContext.Log.Find(logID);
                if (myLog != null)
                {
                    _vdContext.Log.Remove(myLog);
                    _vdContext.SaveChanges();
                }

                File myFile = _vdContext.File.Find(id);
                if (myFile != null)
                {
                    _vdContext.File.Remove(myFile);
                    _vdContext.SaveChanges();
                    deleteFileFromDirectory(path);
                }

                return RedirectToAction("MyDocuments");
            }
            catch (Exception e)
            {
                errorMsg = "The file could not be deleted \n For Administrator: " + e.ToString();
                RedirectToAction("ErrorUploading", new { msg = errorMsg });
            }
            return View();
        }

        private void deleteFileFromDirectory(string pathToDelete)
        {
            string path = _hostingEnvironment.WebRootPath + pathToDelete;
            System.IO.File.Delete(path);
        }

        [HttpGet]
        public IActionResult EditPrivateFile(Guid? id)
        {
            return View();
        }

        /* The edit file method is used to add more users to a shared file, only one user can be added at a time
         * the method finds the file and the path
         * The file info is added to the shared database table and locally placed in the shared users directory
         */ 

        [HttpPost] 
        public async Task<IActionResult> EditPrivateFile(Guid id, [Bind(include: "UserEmail")] UserDetail userDetail)
        {
            var user = await _vdContext.UserDetail.FirstOrDefaultAsync(i => i.UserEmail == userDetail.UserEmail);
            if (user == null)
            {
                errorMsg = "The user: " + userDetail.UserEmail + " could not be found! \n Make sure your spelling is correct and retry";
                return RedirectToAction("ErrorUploading", new { msg = errorMsg });
            }
            int userID = user.UserId;
        
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            string currentUserEmail = currentUser.Email;

            var findFile = await _vdContext.File.FindAsync(id);
            string filename = findFile.FileName;

            string sourceFile = _hostingEnvironment.WebRootPath + findFile.FileDirectory;

            string destFile = _hostingEnvironment.WebRootPath + "\\Documents\\" + userDetail.UserEmail
                        + "\\Shared_Documents\\" + filename;
            try
            {
                if (System.IO.Directory.Exists(destFile))
                {
                    errorMsg = "The file does not exists, please first upload the document";
                    return RedirectToAction("ErrorUploading", new { msg = errorMsg });
                } 
                else
                    System.IO.File.Copy(sourceFile, destFile);

                upload_Shared_To_DB(userID, currentUser.Email, id);
                
            }
            catch (Exception e)
            {
                errorMsg = "The file could not be shared \n For Administrator: " + e.ToString();
                return RedirectToAction("ErrorUploading", new { msg = errorMsg });
            }
            return View();
        }

        private void upload_Shared_To_DB(int userID, string email, Guid fileID)
        {
            SharedOwnership sharedOwnership = new SharedOwnership();
            sharedOwnership.FilesId = fileID;
            sharedOwnership.UsersId = userID;
            sharedOwnership.SharedBy = email;

            _vdContext.Add(sharedOwnership);
            _vdContext.SaveChanges();
        }


        //This is a custom error page
        public ActionResult ErrorPageMyDocs(string msg)
        {
            ViewBag.error = msg;
            return View();
        }

        public ActionResult succsesfullSharedAdded()
        {
            return View();
        }
    }
}