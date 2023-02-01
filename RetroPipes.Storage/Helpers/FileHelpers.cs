// MARS Web App by Rockwell Automation, Inc. (C) 2019-present

using System.IO;

namespace RetroPipes.Storage.Helpers;

internal static class FileHelpers
{
    internal static string GetLocalStoreFilePath(string filename) => Path.Combine(System.AppContext.BaseDirectory, filename);
}
