// MIT License, see COPYING.TXT
using System;

namespace BitAddict.Aras.ArasSync
{
    /// <inheritdoc />
    /// <summary>
    /// Defines what cateogry an ArasSync command belongs to.
    /// Used when printing help text.
    /// </summary>
    public class CommandCategoryAttribute : Attribute
    {
        public CommandCategoryAttribute(string category)
        {
            Category = category;
        }

        public string Category { get; set; }
    }
}