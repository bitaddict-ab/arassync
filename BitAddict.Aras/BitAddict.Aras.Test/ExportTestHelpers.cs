using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeOpenXml;
using ICSharpCode.SharpZipLib.Zip;

namespace BitAddict.Aras.Test
{
    public static class ExportTestHelpers
    {
        public static ExcelWorksheet GetWorksheet(ExcelWorkbook wb, string name)
        {
            var sheet = wb.Worksheets[name];

            Assert.IsNotNull(sheet, $"no worksheet named {name} found: " +
                                    $"[{string.Join(", ", wb.Worksheets.Select(ws => ws.Name))}]");

            return sheet;
        }

        public static ExcelWorksheet GetWorksheet(string zipFilePath, string name)
        {
            var wb = GetWorkbook(zipFilePath);
            return GetWorksheet(wb, name);
        }

        public static ExcelWorkbook GetWorkbook(string zipFilePath)
        {
            using (var zipEnum = CreateZipEnumerator(zipFilePath))
            {
                var entry = zipEnum.Enumerate()
                    .First(e => e.Name.EndsWith(".xlsx") && !e.Name.Contains('\\'));

                using (var memStream = new MemoryStream(new byte[entry.Size]))
                {
                    zipEnum.ZipInputStream.CopyTo(memStream);
                    memStream.Position = 0;

                    var excelPackage = new ExcelPackage();
                    excelPackage.Load(memStream);
                    return excelPackage.Workbook;
                }
            }
        }

        public static ZipEnumerator CreateZipEnumerator(string zipFile)
        {
            var stream = File.Open(zipFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            var zip = new ZipInputStream(stream);
            return new ZipEnumerator(zip);
        }

        public class ZipEnumerator : IDisposable
        {
            public ZipInputStream ZipInputStream { get; }

            public ZipEnumerator(ZipInputStream zip)
            {
                ZipInputStream = zip;
            }

            public IEnumerable<ZipEntry> Enumerate()
            {
                ZipEntry e;
                while ((e = ZipInputStream.GetNextEntry()) != null)
                    yield return e;
            }

            public void Dispose()
            {
                ZipInputStream.Dispose();
            }
        }
    }
}
