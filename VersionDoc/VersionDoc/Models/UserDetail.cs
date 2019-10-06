using System;
using System.Collections.Generic;

namespace VersionDoc.Models
{
    public partial class UserDetail
    {
        public UserDetail()
        {
            File = new HashSet<File>();
            SharedOwnership = new HashSet<SharedOwnership>();
        }

        public int UserId { get; set; }
        public string UserFirstName { get; set; }

        public string UserLastName { get; set; }
        public string UserEmail { get; set; }
        public string LoginId { get; set; }

        public AspNetUsers Login { get; set; }
        public ICollection<File> File { get; set; }
        public ICollection<SharedOwnership> SharedOwnership { get; set; }
    }
}
