using System;
using System.IO;

namespace RetroPipes.Helpers
{
    internal static class FileHelpers
    {
        internal static string GetLocalStoreFilePath(string filename)
        {
            return Path.Combine(System.AppContext.BaseDirectory, filename);
        }
    }
}
