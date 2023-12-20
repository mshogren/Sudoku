using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Sudoku
{
    public class Cell : SortedSet<int>
    {
        private readonly int _i;
        private readonly int _j;
        private readonly Sudoku _sudoku;

        private Cell(Sudoku sudoku, int i, int j)
        {
            _sudoku = sudoku;
            _i = i;
            _j = j;
        }

        private Cell(Sudoku sudoku, int i, int j, IEnumerable<int> collection) : base(collection)
        {
            _sudoku = sudoku;
            _i = i;
            _j = j;
        }

        [PublicAPI]
        public IEnumerable<Cell> ColumnNeighbours
        {
            get
            {
                foreach (var m in Enumerable.Range(0, 9).Where(x => x != _i)) yield return _sudoku[m, _j];
            }
        }

        [PublicAPI]
        public IEnumerable<Cell> RowNeighbours
        {
            get
            {
                foreach (var n in Enumerable.Range(0, 9).Where(x => x != _j)) yield return _sudoku[_i, n];
            }
        }

        [PublicAPI]
        public IEnumerable<Cell> SquareNeighbours
        {
            get
            {
                var x = _i / 3;
                var y = _j / 3;
                foreach (var m in Enumerable.Range(x * 3, 3))
                    foreach (var n in Enumerable.Range(y * 3, 3))
                        if (m != _i || n != _j)
                            yield return _sudoku[m, n];
            }
        }

        public void Deconstruct(out int i, out int j)
        {
            i = _i;
            j = _j;
        }

        public bool HasSinglePossibleValue(int value)
        {
            return RowNeighbours.HaveEliminated(value)
                   || ColumnNeighbours.HaveEliminated(value)
                   || SquareNeighbours.HaveEliminated(value);
        }

        public bool RemoveAllExcept(int value)
        {
            IntersectWith(new[] { value });
            return true;
        }

        public bool RemoveFromNeighbours(int value)
        {
            var resolved = false;
            var neighbours = RowNeighbours.Union(ColumnNeighbours).Union(SquareNeighbours);

            foreach (var cell in neighbours) resolved |= cell.Remove(value);
            return resolved;
        }

        public class CellFactory(Sudoku sudoku, int i, int j)
        {
            public Cell Create(int digit)
            {
                return new Cell(sudoku, i, j) { digit };
            }

            public Cell Create(IEnumerable<int> enumerable)
            {
                return new Cell(sudoku, i, j, enumerable);
            }
        }
    }

    public static class Extensions
    {
        public static bool HaveEliminated(this IEnumerable<SortedSet<int>> collection, int value)
        {
            return collection.All(x => !x.Contains(value));
        }
    }
}