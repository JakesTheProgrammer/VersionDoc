using System;
using System.Collections.Generic;

namespace VersionDoc.Models
{
    public partial class Log
    {
        public int LogId { get; set; }
        public Guid FileId { get; set; }
        public string LogUploader { get; set; }
        public DateTime LogDateTime { get; set; }
        public string LogSize { get; set; }

        public File File { get; set; }
    }
}
