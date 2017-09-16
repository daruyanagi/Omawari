using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Omawari.Utilities
{
    public static class EnvironmentHelper
    {
        public static string GetOneDrivePath()
        {
            var temp = (string)Microsoft.Win32.Registry.GetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\OneDrive",
                @"UserFolder",
                string.Empty
                );

            if (string.IsNullOrEmpty(temp)) throw new System.IO.DirectoryNotFoundException();

            return temp;
        }
        
        public static string GetAppFolderOnOneDrive()
        {
            var assembly = Assembly.GetExecutingAssembly().GetName();
            var temp = Path.Combine(Utilities.EnvironmentHelper.GetOneDrivePath(), assembly.Name);
            Directory.CreateDirectory(temp); // 作成しておく
            return temp;
        }
    }
}
