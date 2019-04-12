// MIT License, see COPYING.TXT
using System;

namespace BitAddict.Aras.ArasSync
{
    internal class CommandCategoryAttribute : Attribute
    {
        public CommandCategoryAttribute(string category)
        {
            Category = category;
        }

        public string Category { get; set; }
    }
}