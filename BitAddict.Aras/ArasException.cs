// MIT License, see COPYING.TXT
using System;
using Aras.IOM;
using JetBrains.Annotations;

namespace BitAddict.Aras
{
    /// <inheritdoc />
    /// <summary>
    /// Represents AML query errors
    /// </summary>
    public class ArasException : Exception
    {
        /// <summary>
        /// Source item that caused error
        /// </summary>
        [CanBeNull]
        public Item SourceItem { get; set; }

        /// <summary>
        /// Item returned from Aaras that contains the error cause
        /// </summary>
        [CanBeNull]
        public Item ResultItem { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Create exception with message
        /// </summary>
        /// <param name="message"></param>
        public ArasException(string message) : base(message)
        { }

        /// <inheritdoc />
        /// <summary>
        /// Create exception from AML error Item. Sets ResultItem.
        /// </summary>
        /// <param name="resultItem"></param>
        public ArasException(Item resultItem) : base(resultItem?.getErrorDetail() ?? "null")
        {
            ResultItem = resultItem;
        }

        /// <inheritdoc />
        /// <summary>
        /// Create when Aras query fails due to another exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        // ReSharper disable once UnusedMember.Global
        public ArasException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}