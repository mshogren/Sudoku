using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace Sudoku
{
    public class Sudoku
    {
        private readonly Cell[,] _array = new Cell[9, 9];
        private readonly TextWriter _log;

        private Sudoku(TextWriter log)
        {
            _log = log;
        }

        public Sudoku(TextReader input, TextWriter log) : this(log)
        {
            for (var i = 0; i < 9; i++)
            {
                var line = input.ReadLine();
                if (string.IsNullOrEmpty(line)) throw new InvalidDataException($"Line '{i + 1}' is blank.");

                var chars = line.ToCharArray();

                for (var j = 0; j < 9; j++)
                {
                    var factory = new Cell.CellFactory(this, i, j);

                    var current = chars[j];
                    if (int.TryParse(current.ToString(), out int digit))
                        this[i, j] = factory.Create(digit);
                    else if (current == ' ')
                        this[i, j] = factory.Create(Enumerable.Range(1, 9));
                    else
                        throw new InvalidDataException($"'{current}' is not valid.");
                }
            }

            _log.WriteLine(Print());
        }

        [PublicAPI]
        public IEnumerable<Cell> AllCells => _array.Cast<Cell>();

        [PublicAPI]
        public Cell this[int i, int j]
        {
            get => _array[i, j];
            set => _array[i, j] = value;
        }

        [PublicAPI]
        public Sudoku Clone()
        {
            var sudoku = new Sudoku(_log);

            for (var i = 0; i < 9; i++)
                for (var j = 0; j < 9; j++)
                {
                    var factory = new Cell.CellFactory(sudoku, i, j);
                    sudoku[i, j] = factory.Create(this[i, j]);
                }

            return sudoku;
        }

        [PublicAPI]
        public string Print()
        {
            var border = "+---+---+---+\n";
            var output = "\n" + border;
            for (var i = 0; i < 9; i++)
            {
                output += "|";
                for (var j = 0; j < 9; j++)
                {
                    if (this[i, j].Count == 1)
                        output += this[i, j].First().ToString();
                    else
                        output += " ";

                    if (j % 3 == 2) output += "|";
                }

                output += "\n";
                if (i % 3 == 2) output += border;
            }

            return output;
        }

        public bool Resolve()
        {
            bool resolved;
            var iterations = 0;
            do
            {
                resolved = ResolveConstraints();
                _log.Write(Print());
                iterations++;
            } while (resolved);

            _log.WriteLine($"{iterations} iterations");

            if (AllCells.All(x => x.Count == 1))
            {
                _log.WriteLine("Puzzle solved");
                return true;
            }

            var brokenCells = AllCells.Where(x => x.Count == 0).ToList();
            var unresolvedCells = AllCells.Where(x => x.Count > 1).OrderBy(x => x.Count).ToList();

            if (brokenCells.Count != 0)
            {
                _log.WriteLine("Not solved (dead end)");
            }
            else if (unresolvedCells.Count != 0)
            {
                _log.WriteLine("Not solved (time to guess)");

                var candidate = unresolvedCells.First();
                var (i, j) = candidate;

                _log.WriteLine($"[{i}, {j}]");

                foreach (var guess in candidate)
                {
                    _log.WriteLine($"Guess: {guess}");
                    var sudoku = Clone();
                    sudoku[i, j].Add(guess);
                    sudoku[i, j].RemoveAllExcept(guess);

                    var done = sudoku.Resolve();

                    if (done)
                    {
                        foreach (var current in _array)
                        {
                            var (m, n) = current;
                            this[m, n].RemoveAllExcept(sudoku[m, n].First());
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        [PublicAPI]
        public bool ResolveConstraints()
        {
            var resolved = false;

            foreach (var current in _array)
                if (current.Count == 1)
                {
                    var value = current.First();

                    resolved |= current.RemoveFromNeighbours(value);
                }
                else
                {
                    foreach (var value in Enumerable.Range(1, 9))
                        if (current.Contains(value) && current.HasSinglePossibleValue(value))
                            resolved |= current.RemoveAllExcept(value);
                }

            return resolved;
        }
    }
}