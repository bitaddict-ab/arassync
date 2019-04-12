// MIT License, see COPYING.TXT

using System;
using System.Diagnostics.CodeAnalysis;

namespace BitAddict.Aras.ArasSync
{
    /// <inheritdoc />
    ///  <summary>
    ///  Represents benign errors.
    ///  Will just print message and exit with rval 1,
    ///  and not give full stack trace and return with 3.
    ///  </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal class UserMessageException : Exception
    {
        internal UserMessageException(string message) : base(message) { }
        internal UserMessageException(string message, Exception baseException) : base(message, baseException) { }
    }
}