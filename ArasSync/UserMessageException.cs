using System;

namespace BitAddict.Aras.ArasSyncTool
{
    /// <summary>
    /// Represents benign errors.
    ///
    /// Will just print message and exit with rval 1,
    /// and not give full stack trace and return with 3.
    /// </summary>
    internal class UserMessageException : Exception
    {
        internal UserMessageException(string message) : base(message) { }
        internal UserMessageException(string message, Exception baseException) : base(message, baseException) { }
    }
}