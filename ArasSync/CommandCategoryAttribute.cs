using System;

namespace BitAddict.Aras.ArasSyncTool
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