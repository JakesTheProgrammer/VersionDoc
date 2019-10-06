using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace VersionDoc.Models
{
    public class UploadFile
    {
        public IFormFile Document { get; set; }
        public File File { get; set; }
    }
}
