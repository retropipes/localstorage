// MARS Web App by Rockwell Automation, Inc. (C) 2019-present

using System;

namespace RetroPipes.Storage.Tests;

internal static class TestHelpers
{
    /// <summary>
    /// Configuration that can be used for initializing a unique LocalStorage instance.
    /// </summary>
    internal static ILocalStorageConfiguration UniqueInstance() => new LocalStorageConfiguration()
    {
        Filename = Guid.NewGuid().ToString()
    };
}
