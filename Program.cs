using System;

namespace Sudoku
{
    internal static class Program
    {
        private static void Main()
        {
            var sudoku = new Sudoku(Console.In, Console.Out);

            sudoku.Resolve();
        }
    }
}