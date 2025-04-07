﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bishop_BSG_Tool
{
    static class Utility
    {
        public static bool PathIsFolder(string path)
        {
            return (File.GetAttributes(path) & FileAttributes.Directory) != 0;
        }
    }
}
