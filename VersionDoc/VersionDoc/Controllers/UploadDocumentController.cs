using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VersionDoc.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

namespace VersionDoc.Controllers
{
    public class UploadDocumentController : Controller
    {
        private versionDocContext _vdContext;
        private IHostingEnvironment _hostingEnvironment;
        private UserManager<ApplicationUser> _userManager;
        private DateTime date = DateTime.Now;
        private Guid fileReUploadID;
        private bool reuploadIsShared;
        private int reUploadoriginalUser;
        private int reUploadOriginalpermission;
        private string fileType = "";
        private string error_msg = "";

        public UploadDocumentController(versionDocContext versionDocContext, IHostingEnvironment hosting, UserManager<ApplicationUser> userManager)
        {
            _vdContext = versionDocContext;
            _hostingEnvironment = hosting;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        /*This is the upload method, it uses the actual file and the model uploadInfo as parameters
         * The method gets the current user, finds the user id, checks for errors and then try and upload the document
         * In the try catch, the document is searched to see if it is reuploaded or new upload, paths are created for local reposirtoy
         * The file information is uploaded to the database by the use of the uploadInfoToDatabase and reUpload_File_Into_DB methods
         * In the end the file is placed in the repository with filestream 
         */
        [HttpPost]
        public async Task<IActionResult> Upload(int id, IFormFile document, [Bind(include: "Pub, Priv, shared, UserEmail")] UploadInfo uploadInfo, UserDetail userDetail)
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            string currentUserEmail = currentUser.Email;

            var user = await _vdContext.UserDetail.FirstOrDefaultAsync(i => i.UserEmail == currentUserEmail);
            int userID = user.UserId;

            if (document == null || document.Length == 0) {
                error_msg = "The file that you are trying to upload does not exists or nothing is in the file(0KB)";
                return RedirectToAction("ErrorUploading", new { msg = error_msg });
            }
            if (getFileTypeExtension(document.FileName) == false) { 
                error_msg = "The file type" + fileType + " is not supported";
                return RedirectToAction("ErrorUploading", new { msg = error_msg });
            }

            if (checkSelected(uploadInfo.Pub,uploadInfo.Priv) == false)
                return RedirectToAction("ErrorUploading", new { msg = "Please select either public or private" });

            string filename = document.FileName;
            string path = " ";
            string pathHosting = _hostingEnvironment.WebRootPath;

            try
            {
                if (check_File_ReUpload_Exists(filename))
                {
                    await reUpload_File_Into_DB(userID, filename, Convert.ToInt32(document.Length), date, currentUserEmail);
                    var findoriganluser = _vdContext.UserDetail.Find(reUploadoriginalUser);
                    string ogUser = findoriganluser.UserEmail;

                    if (reuploadIsShared)
                        path = createFolderPath(filename, ogUser, "Private");
                    else
                    {
                        if (reUploadOriginalpermission == 1)
                            path = createFolderPath(filename, ogUser, "Private");
                        else
                            path = createFolderPath(filename, ogUser, "Public");
                    }
                    pathHosting += path;
                }
                else
                { 
                    if (uploadInfo.Priv)
                    {
                        path = createFolderPath(filename, currentUserEmail, "Private");
                        pathHosting += path;
                        await uploadInfoToDatabase(userID, filename, Convert.ToInt32(document.Length), date, 1, path, fileType, currentUserEmail);
                    }
                    else
                        if (uploadInfo.Pub)
                        {
                            path = createFolderPath(filename, currentUserEmail, "Public");
                            pathHosting += path;
                            await uploadInfoToDatabase(userID, filename, Convert.ToInt32(document.Length), date, 0, path, fileType, currentUserEmail);
                        }
                }
            }
            catch (DataException){
                error_msg = "The file could not be saved, please retry or contact the site administrator";
                return RedirectToAction("ErrorUploading", new { msg = error_msg });
            }

            using (var stream = new FileStream(pathHosting, FileMode.Create))
            {
                await document.CopyToAsync(stream);
            }
            
            if (uploadInfo.shared)
                await shared(uploadInfo.UserEmail, filename, currentUserEmail, document);

            return RedirectToAction("filesucsessfulUploadPage");
            return View();
        }

        private bool checkSelected(bool pub, bool priv)
        {
            bool check = true;
            if ((pub == false) && (priv == false))
                check = false;

            return check;
        }

        //Method is used to find out if the file is of correct type
        private bool getFileTypeExtension(string fileName)
        {
            int index = fileName.IndexOf('.');
            fileType = fileName.Substring(index);

            bool fileExists = false;
            switch (fileType)
            {
                case ".docx":
                    fileExists = true;
                    break;
                case ".doc":
                    fileExists = true;
                    break;
                case ".xlsx":
                    fileExists = true;
                    break;
                case ".xlx":
                    fileExists = true;
                    break;
                case ".txt":
                    fileExists = true;
                    break;
            }

            return fileExists;
        }

        //This method is used to create the local folder paths by using the user email and what type (private or public)
        private string createFolderPath(string file_name, string email, string permission)
        {
            string sub_folder_name = "";
            if (permission.Equals("Public"))
            {
                sub_folder_name = "Public_Documents";
            }
            else
                if (permission.Equals("Private"))
            {
                sub_folder_name = "Private_Documents";
            }

            string path = "\\Documents\\" + email + "\\" + sub_folder_name + "\\";
            path.Replace(" ", "");
            return path + file_name;
        }

        //This method is used to insert the file info into the database, by use of the parameters
        private async Task uploadInfoToDatabase(int userID, string fileName, int size, DateTime uploadDate, int permission, string directory, string typeOfFile, string currUserEmail)
        {
            VersionDoc.Models.File myFile = new Models.File();
            myFile.FileId = Guid.NewGuid();
            myFile.UserId = userID;
            myFile.FileName = fileName;
            myFile.FileSize = size.ToString();
            myFile.FileUploadDate = uploadDate;
            myFile.FilePermission = permission;
            myFile.FileDirectory = directory;
            myFile.FileType = typeOfFile;

            _vdContext.Add(myFile);
            _vdContext.SaveChanges();

            Log myLog = new Log();

            var file_uploaded = await _vdContext.File.FirstOrDefaultAsync(i => i.FileName == fileName);
            Guid fileID = file_uploaded.FileId;

            myLog.FileId = fileID;
            myLog.LogUploader = currUserEmail;
            myLog.LogDateTime = uploadDate;
            myLog.LogSize = size.ToString();

            _vdContext.Add(myLog);
            _vdContext.SaveChanges();
        }

        /*The shared method is where the shared path to the user it is shared with is made
         * The file is then stored in that users shared folder 
         * After the file is in the local files it is uploaded to the databse with the upload_Shared_To_DB method
         */
        private async Task shared(string shared_User_Name, string file_name, string email, IFormFile document)
        {
            string path = _hostingEnvironment.WebRootPath + "\\Documents\\" + shared_User_Name
                        + "\\Shared_Documents\\" + file_name;
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await document.CopyToAsync(stream);
            }

            var shared_user = await _vdContext.UserDetail.FirstOrDefaultAsync(i => i.UserEmail == shared_User_Name);
            int shared_userID = shared_user.UserId;

            try
            {
                await upload_Shared_To_DB(shared_userID, email, file_name);
            }
            catch (DataException)
            {
                ModelState.AddModelError("", "Could not share the file");
            }
        }

        private async Task upload_Shared_To_DB(int userID, string email, string fileName)
        {
            var file_uploaded = await _vdContext.File.FirstOrDefaultAsync(i => i.FileName == fileName);
            Guid fileID = file_uploaded.FileId;

            SharedOwnership sharedOwnership = new SharedOwnership();
            sharedOwnership.FilesId = fileID;
            sharedOwnership.UsersId = userID;
            sharedOwnership.SharedBy = email;

            _vdContext.Add(sharedOwnership);
            _vdContext.SaveChanges();
        }

        //This method checks that the file that is reupload exists and return a booleon if it is found
        private bool check_File_ReUpload_Exists(string fileName)
        {
            bool check = false;
            if (_vdContext.File.Any(item => item.FileName == fileName))
            {
                check = true;
                var fileInfo = _vdContext.File.FirstOrDefault(item => item.FileName == fileName);
                reUploadoriginalUser = fileInfo.UserId;

                if (_vdContext.SharedOwnership.Any(i => i.Files.FileName == fileName))
                {
                    reuploadIsShared = true;
                }
            }
            return check;
        }

        //This method takes the parameters and place the reuploaded file information in the database, the file table is updated and the log file is added with a new entry
        private async Task reUpload_File_Into_DB(int userID, string fileName, int size, DateTime date, string email)
        {
            var file_uploaded = await _vdContext.File.FirstOrDefaultAsync(i => i.FileName == fileName);
            fileReUploadID = file_uploaded.FileId;

            Models.File myFile = _vdContext.File.Find(fileReUploadID);
            Log myLog = new Log();

            if (reuploadIsShared)
            {
                myFile.FileName = fileName;
                myFile.FileSize = size.ToString();
                myFile.FileUploadDate = date;

                _vdContext.Update(myFile);
                _vdContext.SaveChanges();

                myLog.FileId = fileReUploadID;
                myLog.LogUploader = email;
                myLog.LogDateTime = date;
                myLog.LogSize = size.ToString();

                _vdContext.Add(myLog);
                _vdContext.SaveChanges();

                System.IO.File.Delete(_hostingEnvironment.WebRootPath + myFile.FileDirectory);
                var currentUser = await _userManager.GetUserAsync(HttpContext.User);
                string currentUserEmail = currentUser.Email; 
                System.IO.File.Delete(_hostingEnvironment.WebRootPath + "\\Documents\\" + currentUserEmail
                        + "\\Shared_Documents\\" + fileName);
            }
            else
            {
                reUploadOriginalpermission = myFile.FilePermission;
                myFile.FileName = fileName;
                myFile.FileSize = size.ToString();
                myFile.FileUploadDate = date;

                _vdContext.Update(myFile);
                _vdContext.SaveChanges();

                myLog.FileId = fileReUploadID;
                myLog.LogUploader = email;
                myLog.LogDateTime = date;
                myLog.LogSize = size.ToString();

                _vdContext.Add(myLog);
                _vdContext.SaveChanges();

                System.IO.File.Delete(_hostingEnvironment.WebRootPath + myFile.FileDirectory);
            }
        }

        /*
         * The following four methods are used for dowloading the file
         * dowloadFile is the main methods 
         * findfilePath is used to find the actual local file directory 
         * GetContentType and GetMimeTypes are used to check for the file extenstion when dowloading the file 
         */
        public async Task<IActionResult> downloadFile(Guid? id, string type, string email ,string fileName) //string fileName
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            string currentUserEmail = currentUser.Email;

            var findFileName = _vdContext.File.Find(id);
            string filename = findFileName.FileName;

            string path = _hostingEnvironment.WebRootPath + findFilePath(filename, email, type);

            var memory = new MemoryStream();

            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            
            return File(memory, GetContentType(path), Path.GetFileName(path));
        }

        private string findFilePath(string file_name, string email, string folderSubName)
        {
            string sub_folder_name = "";
            if (folderSubName.Equals("Public"))
            {
                sub_folder_name = "Public_Documents";
            }
            else
                if (folderSubName.Equals("Private"))
                {
                    sub_folder_name = "Private_Documents";
                }
                else
                {
                    sub_folder_name = "Shared_Documents";
                }

            string path = "\\Documents\\" + email + "\\" + sub_folder_name + "\\";
            path.Replace(" ", "");

            return path += file_name;
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet"},
                {".csv", "text/csv"}
            };
        }

        //This is a method to return a custom error page
        public ActionResult ErrorUploading(string msg)
        {
            ViewBag.error = msg;
            return View();
        }

        public ActionResult filesucsessfulUploadPage()
        {
            return View();
        }
    }
}