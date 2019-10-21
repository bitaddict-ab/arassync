﻿// MIT License, see COPYING.TXT
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
        /// Apply an item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Item Apply(Item item)
        {
            var action = item.getAction()?.ToLower();
            switch (action)
            {
                case "get":
                {
                    // Rudimentary support for fetching all generations of an item
                    if (item.getProperty("generation") == "1" && item.getPropertyCondition("generation") == "ge")
                    {
                        var allItems = _mockItems
                            .Select(mockItem => mockItem.Value)
                            .Where(mockItem => mockItem.getType() == item.getType());
                        var collection = this.NewItem(item.getType());
                        foreach (var mockItem in allItems)
                        {
                            collection.appendItem(mockItem);
                        }
                        return collection;
                    }

                    // Get items normally
                    var i = GetMockItem(item.getType(), item.getID());
                    if (i != null)
                    {
                        return i;
                    }
                    break;
                }
                case "update":
                {
                    var i = UpdateMockItem(item);
                    if (i != null)
                    {
                        return i;
                    }
                    break;
                }
            }
            return null;
        }

        /// <summary>
        /// Update an item in the mock list.
        /// </summary>
        /// <param name="item"></param>
        private Item UpdateMockItem(Item item)
        {
            _mockItems.TryGetValue(item.getID(), out var storedItem);
            if (storedItem == null)
            {
                AddMockItem(item);
            }
            else
            {
                foreach (var propertyName in item.GetProperties())
                {
                    storedItem.setProperty(propertyName, item.getProperty(propertyName));
                }
            }

            return storedItem;
        }

        /// <summary>
        /// Get an item from the mock list.
        /// </summary>
        /// <param name="itemType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private Item GetMockItem(string itemType, string id)
        {
            return _mockItems
                .Where(tuple => tuple.Key == id && tuple.Value.getType() == itemType)
                .Select(tuple => tuple.Value)
                .FirstOrDefault();
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
