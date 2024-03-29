﻿using System.Collections.Generic;
using System.IO;

namespace PrimitierModManager.MelonLoader
{
    internal static class TempFileCache
    {
        private static List<string> TempFiles = new List<string>();
        internal static string CreateFile()
        {
            string temppath = Path.GetTempFileName();
            TempFiles.Add(temppath);
            return temppath;
        }
        internal static void ClearCache()
        {
            if (TempFiles.Count <= 0)
                return;
            foreach (string file in TempFiles)
                if (File.Exists(file))
                    File.Delete(file);
        }
    }
}
