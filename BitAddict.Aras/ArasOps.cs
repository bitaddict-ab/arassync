// MIT License, see COPYING.TXT
using System;
using Aras.IOM;

namespace BitAddict.Aras
{
    /// <summary>
    /// Commonly used Aras Operations
    /// </summary>
    public class ArasOps
    {
        /// <summary>
        ///  Reset modified data to what it was before. (AML does not work)
        /// </summary>
        /// <param name="item"></param>
        public static void RestoreModifiedProperties(Item item)
        {
            if (item.getProperty("modified_on") == "" ||
                item.getProperty("modified_by_id") == "")
                throw new ArgumentException("Item is missing modified properties", nameof(item));

            var sql =
                "UPDATE [Innovator].[PART]\n" +
                "SET [modified_on]    = '" + item.getProperty("modified_on") + "',\n" +
                "    [modified_by_id] = '" + item.getProperty("modified_by_id") + "'\n" +
                "WHERE [id] = '" + item.getID() + "'";

            ArasExtensions.Innovator.ApplySQL(sql);
        }

        /// <summary>
        /// Get the item with 'is_current=1' of given type and 'keyed_name'
        /// </summary>
        /// <param name="type"></param>
        /// <param name="keyedName"></param>
        /// <returns></returns>
        public static Item GetCurrentItem(string type, string keyedName)
        {
            var item = ArasExtensions.Innovator.newItem(type, "get");
            item.setProperty("keyed_name", keyedName);
            return ArasExtensions.Innovator.ApplyItem(item);
        }

        /// <summary>
        /// Delete a specific Item.
        /// </summary>
        /// <param name="itemType"></param>
        /// <param name="id"></param>
        /// <exception cref="ArasException"></exception>
        public static void DeleteItem(string itemType, string id)
        {
            var innovator = ArasExtensions.Innovator;

            // TODO: Fetch development db instance here, disallow if running on server.
            if (innovator.getConnection().GetDatabaseName() != "Consilium DEVELOPMENT")
                throw new ArasException("Not allowed to run raw delete on non-development db");

            var deleteSQL = $"DELETE FROM [Innovator].[{itemType.Replace(" ", "_")}]\n" +
                            $"WHERE       [id] = '{id}'";

            innovator.ApplySQL(deleteSQL);
        }
    }
}
