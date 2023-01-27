using System;

namespace RetroPipes
{
    public class LocalStorageException : Exception
    {
        public LocalStorageException() : base() { }
        public LocalStorageException(string message) : base(message) { }
        public LocalStorageException(string message, Exception innerException) : base(message, innerException) { }
    }
}