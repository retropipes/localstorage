// MARS Web App by Rockwell Automation, Inc. (C) 2019-present

using System;

namespace RetroPipes.Storage;

public class LocalStorageException : Exception
{
    public LocalStorageException() : base() { }
    public LocalStorageException(string message) : base(message) { }
    public LocalStorageException(string message, Exception innerException) : base(message, innerException) { }
}
