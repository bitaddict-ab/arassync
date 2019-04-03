using System.Collections.Generic;
using System.Runtime.InteropServices;
using Aras.IOM;

namespace BitAddict.Aras
{
    /// <summary>
    /// Compare strings by nautural sort (i.e. A2 before A10)
    /// </summary>
    public class NaturalSortComparer : Comparer<string>
    {
        /// <summary>
        /// Singleton
        /// </summary>
        public static readonly NaturalSortComparer Instance = new NaturalSortComparer();

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string psz1, string psz2);

        /// <inheritdoc/>
        public override int Compare(string x, string y)
        {
            if (x == null && y == null)
                return 0;
            if (x == null)
                return 1;
            if (y == null)
                return -1;

            return StrCmpLogicalW(x, y);
        }
    }

    /// <summary>
    /// Compare Aras Items by ID
    /// </summary>
    public class ItemIdComparer : IEqualityComparer<Item>
    {
        /// <inheritdoc />
        public bool Equals(Item x, Item y) => x?.getID() == y?.getID();

        /// <inheritdoc />
        public int GetHashCode(Item obj) => obj?.getID().GetHashCode() ?? 0;
    }
}