﻿using System;
using RetroPipes;

namespace RetroPipes.LocalStorageTests
{
    internal static class TestHelpers
    {
        /// <summary>
        /// Configuration that can be used for initializing a unique LocalStorage instance.
        /// </summary>
        internal static ILocalStorageConfiguration UniqueInstance()
        {
            return new LocalStorageConfiguration()
            {
                Filename = Guid.NewGuid().ToString()
            };
        }
    }
}