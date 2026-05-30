using System;
using System.Collections.Generic;

namespace SudokuWinFormsApp
{
    public static class SudokuGenerator
    {
        private static readonly Random random = new Random();

        // Genera un nuovo puzzle sudoku con il numero specificato di celle vuote
        // Ritorna sia il puzzle sia la soluzione
        public static (int[,] puzzle, int[,] solution) GeneratePuzzle(int emptyCells = 40)
        {
            // Genera la soluzione
            int[,] solution = GenerateCompleteSolution();

            // Rimuovi le celle in maniera random per generare il puzzle
            int[,] puzzle = (int[,])solution.Clone();
            RemoveCells(puzzle, solution, emptyCells);

            return (puzzle, solution);
        }

        private static int[,] GenerateCompleteSolution()
        {
            int[,] grid = new int[9, 9];

            // Completa prima i quadrati della diagonale: questi sono infatti
            // indipendenti tra loro
            FillDiagonalBoxes(grid);

            // Risolvi il resto del puzzle mediante un metodo di backtracking
            SolveWithBacktracking(grid, 0, 0);

            return grid;
        }

        // Riempi i quadrati della diagonale
        // PS: i numeri che vengono inseriti in queste caselle non dipendono
        // dai valori delle colonnne e delle righe, per cui possiamo compilarle
        // senza controlli particolari
        private static void FillDiagonalBoxes(int[,] grid)
        {
            for (int box = 0; box < 9; box += 3)
            {
                var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                Shuffle(numbers);

                int idx = 0;
                for (int r = 0; r < 3; r++)
                    for (int c = 0; c < 3; c++)
                        grid[box + r, box + c] = numbers[idx++];
            }
        }

        // Risolvi con il backtracking
        private static bool SolveWithBacktracking(int[,] grid, int row, int col)
        {
            // Trova la prossima cella vuota
            while (row < 9 && grid[row, col] != 0)
            {
                col++;
                if (col == 9) { col = 0; row++; }
            }

            // Se siamo arrivati a 9 vuol dire che il puzzle è completato (0-8).
            if (row == 9) return true;

            // Prova ogni numero per trovare la soluzione
            var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Shuffle(numbers);

            foreach (int num in numbers)
            {
                if (IsValid(grid, row, col, num))
                {
                    // Se è valido proviamo a risolvere il puzzle con una chiamata ricorsiva.
                    grid[row, col] = num;
                    if (SolveWithBacktracking(grid, row, col))
                        return true;

                    // Se è sbagliato, resettiamo la cella e proviamo un altro numero
                    grid[row, col] = 0;
                }
            }
            return false;
        }

        private static bool IsValid(int[,] grid, int row, int col, int num)
        {
            // Controlla righe e colonne
            for (int i = 0; i < 9; i++)
                if (grid[row, i] == num || grid[i, col] == num)
                    return false;

            // Controlla il quadrato in cui si trova la cella
            int boxRow = 3 * (row / 3), boxCol = 3 * (col / 3);
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    if (grid[boxRow + r, boxCol + c] == num)
                        return false;

            return true;
        }

        // Rimuovi delle celle per creare il puzzle
        private static void RemoveCells(int[,] puzzle, int[,] solution, int targetEmpty)
        {
            // Crea un array contente tutte le possibili celle
            var positions = new List<(int, int)>();
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    positions.Add((r, c));

            // Randomizza le posizioni.
            Shuffle(positions);
            int removed = 0;

            foreach (var (r, c) in positions)
            {
                // Esci se siamo arrivati al numero di caselle voluto
                if (removed >= targetEmpty) break;

                int backup = puzzle[r, c];
                puzzle[r, c] = 0;

                // Controlla che ci siano ancora soluzioni uniche
                if (CountSolutions(puzzle) == 1)
                {
                    removed++;
                }
                else
                {
                    // Non ci sono soluzioni uniche, quindi backtrack
                    puzzle[r, c] = backup;
                }
            }
        }

        private static int CountSolutions(int[,] grid, int limit = 2)
        {
            int count = 0;
            CountSolutionsHelper(grid, 0, 0, ref count, limit);
            return count;
        }

        // Medesimo algoritmo di backtracking in modo da calcolare il numero di soluzioni possibili
        private static void CountSolutionsHelper(int[,] grid, int row, int col, ref int count, int limit)
        {
            if (count >= limit) return;

            // Find next empty cell
            while (row < 9 && grid[row, col] != 0)
            {
                col++;
                if (col == 9) { col = 0; row++; }
            }

            if (row == 9) { count++; return; }

            for (int num = 1; num <= 9; num++)
            {
                if (IsValid(grid, row, col, num))
                {
                    grid[row, col] = num;
                    CountSolutionsHelper(grid, row, col, ref count, limit);
                    grid[row, col] = 0;

                    if (count >= limit) return;
                }
            }
        }

        // Metodo generico per randomizzare un array
        private static void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}