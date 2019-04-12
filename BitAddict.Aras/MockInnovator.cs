// MIT License, see COPYING.TXT
using System.Collections.Generic;
using System.Linq;
using Aras.IOM;
// ReSharper disable InconsistentNaming

namespace BitAddict.Aras
{
    /// <summary>
    /// Mocked wrapper for innovator that can return fake values on some queries.
    /// Expand functionality as required by tests.
    /// </summary>
    public class MockInnovator : Innovator
    {
        private readonly Dictionary<string, Item> _mockItems = new Dictionary<string, Item>();

        /// <inheritdoc/>
        public MockInnovator(IServerConnection serverConnection) : base(serverConnection)
        { }

        /// <summary>
        /// Add an item to the mock list
        /// </summary>
        /// <param name="item"></param>
        public void AddMockItem(Item item)
        {
            _mockItems[item.getID()] = item;
        }

        /// <summary>
        /// Return mock item by id if found, otherwise hit database
        /// </summary>
        /// <param name="itemType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public new Item getItemById(string itemType, string id)
        {
            _mockItems.TryGetValue(id, out var item);
            return item ?? base.getItemById(itemType, id);
        }

        /// <summary>
        /// Return mock item by keyed name if found, otherwise hit database
        /// </summary>
        /// <param name="itemType"></param>
        /// <param name="keyedName"></param>
        /// <returns></returns>
        public new Item getItemByKeyedName(string itemType, string keyedName)
        {
            var item = _mockItems.Values.FirstOrDefault(i => i.getProperty("keyed_name") == keyedName);
            return item ?? base.getItemByKeyedName(itemType, keyedName);
        }
    }
}
