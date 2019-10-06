using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VersionDoc.Models
{
    public class UploadInfo
    {
        public bool Pub { get; set; }
        public bool Priv { get; set; }
        public bool shared { get; set; }
        public string UserEmail { get; set; }
    }
}
