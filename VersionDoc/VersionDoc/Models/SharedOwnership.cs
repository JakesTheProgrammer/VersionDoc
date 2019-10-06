using System;
using System.Collections.Generic;

namespace VersionDoc.Models
{
    public partial class SharedOwnership
    {
        public int UsersId { get; set; }
        public Guid FilesId { get; set; }
        public string SharedBy { get; set; }

        public File Files { get; set; }
        public UserDetail Users { get; set; }
    }
}
