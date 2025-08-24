using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DatabaseManager.FileUtility
{
    public class BaseWriter
    {
        public readonly string DefaultSaveFolder = "export";

        protected void CheckDefaultSaveFolder()
        {
            if (!Directory.Exists(this.DefaultSaveFolder))
            {
                Directory.CreateDirectory(this.DefaultSaveFolder);
            }
        }
    }
}
