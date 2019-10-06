using System;
using System.Collections.Generic;

namespace VersionDoc.Models
{
    public partial class File
    {
        public File()
        {
            Log = new HashSet<Log>();
            SharedOwnership = new HashSet<SharedOwnership>();
        }

        public Guid FileId { get; set; }
        public int UserId { get; set; }
        public string FileName { get; set; }
        public string FileSize { get; set; }
        public DateTime FileUploadDate { get; set; }
        public int FilePermission { get; set; }
        public string FileDirectory { get; set; }
        public string FileType { get; set; }

        public UserDetail User { get; set; }
        public ICollection<Log> Log { get; set; }
        public ICollection<SharedOwnership> SharedOwnership { get; set; }
    }
}
