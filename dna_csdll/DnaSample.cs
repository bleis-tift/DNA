using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ExcelDna.Integration;

namespace dna_csdll
{
    public static class DnaSample
    {
        static IEnumerable<Tuple<int, int>> range =
            from r in Enumerable.Range(1, 99)
            from c in Enumerable.Range(1, 99)
            select Tuple.Create(r, c);

        [ExcelCommand(MenuName = "Bench(C#)", MenuText = "COM Object")]
        public static void ComCs()
        {
            var sw = Stopwatch.StartNew();

            dynamic excel = ExcelDnaUtil.Application;
            dynamic sheet = excel.ActiveSheet;
            foreach (var r_c in range)
            {
                dynamic cell = sheet.Cells.Item(r_c.Item1, r_c.Item2);
                cell.Value2 = 1;
            }

            sw.Stop();
            excel.StatusBar = sw.Elapsed.TotalSeconds;
        }

        [ExcelCommand(MenuName = "Bench(C#)", MenuText = "DNA Object")]
        public static void DnaCS()
        {
            var sw = Stopwatch.StartNew();

            foreach (var r_c in range)
            {
                var row = r_c.Item1 - 1;
                var col = r_c.Item2 - 1;
                var cell = new ExcelReference(row, col);
                cell.SetValue(1);
            }

            sw.Stop();
            dynamic excel = ExcelDnaUtil.Application;
            excel.StatusBar = sw.Elapsed.TotalSeconds;
        }
    }
}