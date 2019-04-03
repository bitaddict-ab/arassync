using System;
using System.Linq;
using Aras.IOM;
using JetBrains.Annotations;

namespace BitAddict.Aras
{
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

        /// <summary>
        /// Create exception with message
        /// </summary>
        /// <param name="message"></param>
        public ArasException(string message) : base(message)
        { }

        /// <summary>
        /// Create exception from AML error Item. Sets ResultItem.
        /// </summary>
        /// <param name="resultItem"></param>
        public ArasException(Item resultItem) : base(resultItem?.getErrorDetail() ?? "null")
        {
            ResultItem = resultItem;
        }

        /// <summary>
        /// Create when Aras query fails due to another exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ArasException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}