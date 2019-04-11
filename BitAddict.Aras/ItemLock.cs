using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aras.IOM;

namespace BitAddict.Aras
{
    /// <summary>
    /// Locks an Aras item using the Disposable pattern
    /// </summary>
    public class ItemLock : IDisposable
    {
        private readonly Item _item;

        /// <summary>
        /// Create ItemLock
        /// </summary>
        /// <param name="item">Item to lock, unlocked in Dispose()</param>
        public ItemLock(Item item)
        {
            item.lockItem();
            _item = item;

        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _item?.unlockItem();
        }
    }
}
