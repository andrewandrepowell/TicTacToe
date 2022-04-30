using System;
using System.Collections.Generic;
using System.Linq;

namespace TicTacToe
{
    public interface Player
    {
        void SetPieceOnBoard(Board board);
        int GetPlayerPiece();
        string GetPlayerName();
    }
    public class Human : Player
    {
        private int piece;
        private string name;
        public Human(int piece, string name)
        {
            this.piece = piece;
            this.name = name;
        }
        public string GetPlayerName()
        {
            return name;
        }
        public void SetPieceOnBoard(Board board)
        {
            // If the board is full, fail. Can only place on a not full board.
            if (board.IsFull())
            {
                throw new InvalidOperationException("Board is full so a move can't be made.");
            }

            // Keep trying to get a valid move from player.
            do
            {
                try
                {
                    int row, col;
                    Console.WriteLine("Player {0}, please enter the row where you want to place your piece.", name);
                    Console.WriteLine("Rows must be in range [0, {0}]. Rows are from top to bottom.", Board.SIZE - 1);
                    row = int.Parse(Console.ReadLine());
                    Console.WriteLine("Selected row: {0}", row);
                    if (row < 0 || row >= Board.SIZE)
                    {
                        Console.WriteLine("Invalid row.");
                        continue;
                    }
                    Console.WriteLine("Player {0}, please enter the col where you want to place your piece.", name);
                    Console.WriteLine("Columns must be in range [0, {0}]. Columns are from left to right.", Board.SIZE - 1);
                    col = int.Parse(Console.ReadLine());
                    Console.WriteLine("Selected col: {0}", col);
                    if (col < 0 || col >= Board.SIZE)
                    {
                        Console.WriteLine("Invalid column.");
                        continue;
                    }
                    if (board.GetPiece(row, col) != null)
                    {
                        Console.WriteLine("Piece already located at {0}, {1}", row, col);
                        continue;
                    }
                    board.SetPiece(row, col, piece);
                    Console.WriteLine("Placed piece at {0}, {1}!", row, col);
                    return;
                }
                catch (FormatException)
                {
                    Console.WriteLine("Incorrect format!");
                }
            }
            while (true);
        }
        public int GetPlayerPiece()
        {
            return piece;
        }
    }
    public class AI : Player
    {
        private int ai_piece;
        private int[] op_pieces;
        private string name;
        public AI(int ai_piece, int[] op_pieces, string name = "Robot")
        {
            foreach (int op_piece in op_pieces)
            {
                if (ai_piece == op_piece)
                {
                    throw new ArgumentException(String.Format("piece {0} shouldn't be the same as op_piece.", ai_piece));
                }
            }

            this.ai_piece = ai_piece;
            this.op_pieces = op_pieces;
            this.name = name;
        }
        public string GetPlayerName()
        {
            return name;
        }
        public void SetPieceOnBoard(Board board)
        {
            // If the board is full, fail. Can only place on a not full board.
            if (board.IsFull())
            {
                throw new InvalidOperationException("Board is full so a move can't be made.");
            }

            // Create strategy for AI and determine best move.
            Strategy ai_strat = new Strategy(ai_piece, board);
            (int ai_weight, (int, int) ai_move) = ai_strat.GetBestMove();

            // Create strategies of opponents and determine their best moves.
            int[] op_weights = new int[op_pieces.Length];
            (int, int)[] op_moves = new (int, int)[op_pieces.Length];
            for (int index = 0; index < op_pieces.Length; index++)
            {
                Strategy op_strat = new Strategy(op_pieces[index], board);
                (op_weights[index], op_moves[index]) = op_strat.GetBestMove();
            }

            // Determine the best move of all the opponents.
            (int op_weight, int op_index) = Strategy.Min(op_weights);
            (int, int) op_move = op_moves[op_index];

            // Determine a random position and a random chance to randomly be random!
            Random random = new Random();
            bool rd_is = (random.Next(0, 4) == 0); // 20% chance to BE RANDOM.
            (int, int) rd_move = ai_strat.GetRandomMove();

            // SOMETIMES YOU JUST GOTTA BE RANDOM.
            if (rd_is)
            {
                (int row, int col) = rd_move;
                board.SetPiece(row, col, ai_piece);
            }
            // If the ai is about to win, go ahead and win!
            else if (ai_weight == 1)
            {
                (int row, int col) = ai_move;
                board.SetPiece(row, col, ai_piece);
            }
            // If the opponent is about to win, block them!
            else if (op_weight == 1)
            {
                (int row, int col) = op_move;
                board.SetPiece(row, col, ai_piece);
            }
            // If there absolute no good moves, just go random.
            else if (ai_weight == Board.SIZE)
            {
                (int row, int col) = rd_move;
                board.SetPiece(row, col, ai_piece);
            }
            // Just go best move.
            else
            {
                (int row, int col) = ai_move;
                board.SetPiece(row, col, ai_piece);
            }
        }
        public int GetPlayerPiece()
        {
            return ai_piece;
        }
    }
    public class Board
    {
        public const int SIZE = 3; // Number of rows and columns. Condition to win.
        private int?[,] board = new int?[SIZE, SIZE];
        public Board()
        {
            if (SIZE < 2)
            {
                throw new InvalidOperationException("Size of board needs to be at least 2. Any lower and it's just too small.");
            }
        }
        public void SetPiece(int row, int col, int piece)
        {
            if (board[row, col] != null)
                throw new ArgumentException(String.Format("Piece already exists at {0}, {1}", row, col));
            if (piece < 0)
                throw new ArgumentException("Negative pieces aren't allowed.");
            board[row, col] = piece;
        }
        public int? GetPiece(int row, int col)
        {
            return board[row, col];
        }
        public int? GetWinner()
        {
            // Check rows for winner.
            for (int row = 0; row < SIZE; row++)
            {
                for (int col = 0; col < SIZE; col++)
                {
                    if (board[row, col] == null)
                    {
                        break;
                    } 
                    else if ((col != 0) && (board[row, col-1] != board[row, col]))
                    {
                        break;
                    }
                    else if ((col == SIZE-1))
                    {
                        return board[row, col];
                    }
                }
            }

            // Check columns for winner.
            for (int col = 0; col < SIZE; col++)
            {
                for (int row = 0; row < SIZE; row++)
                {
                    if (board[row, col] == null)
                    {
                        break;
                    }
                    else if ((row != 0) && (board[row - 1, col] != board[row, col]))
                    {
                        break;
                    }
                    else if ((row == SIZE - 1))
                    {
                        return board[row, col];
                    }
                }
            }

            // Check 0,0 to SIZE-1, SIZE-1 Diagonalonal for winner.
            for (int index = 0; index < SIZE; index++)
            {
                if (board[index, index] == null)
                {
                    break;
                }
                else if ((index !=0) && (board[index - 1, index - 1] != board[index, index]))
                {
                    break;
                }
                else if ((index == SIZE-1))
                {
                    return board[index, index];
                }
            }

            // Check 0,SIZE-1 to SIZE-1, 0 Diagonalonal for winner.
            for (int index = 0; index < SIZE; index++)
            {
                int row = SIZE - 1 - index;
                int col = index;
                if (board[row, col] == null)
                {
                    break;
                }
                else if ((index != 0) && (board[row + 1, col - 1] != board[row, col]))
                {
                    break;
                }
                else if ((index == SIZE - 1))
                {
                    return board[row, col];
                }
            }

            // If no winners are found return no winner.
            return null;
        }

        public bool IsFull()
        {
            for (int row = 0; row < SIZE; row++)
            {
                for (int col = 0; col < SIZE; col++)
                {
                    if (board[row, col] == null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void Display()
        {
            for (int row = 0; row < SIZE; row++)
            {
                for (int col = 0; col < SIZE; col++)
                {
                    Console.Write("({0}, {1})={2} ", row, col, board[row, col]);
                }
                Console.Write('\n');
            }
            
        }
    }
    public class Strategy
    {
        private enum Diagonal : int 
        { 
            BL_TO_TR = 0, TL_TO_BR = 1 
        };
        private enum Dimension : int
        {
            ROWS = 0, COLS = 1, DAGS = 2
        };
        private int piece;
        private Board board;
        public Strategy(int piece, Board board)
        {
            this.piece = piece;
            this.board = board;
        }
        static public (int, int) Min(int[] weights)
        {
            int? min_value = null;
            int? min_index = null;
            if (weights.Length == 0)
                throw new ArgumentException("Length of weights must be greater than zero.");
            for (int index = 0; index < weights.Length; index++)
            {
                if (min_value == null || weights[index] < min_value)
                {
                    min_value = weights[index];
                    min_index = index;
                }
            }    
            return ((int)min_value, (int)min_index);
        }
        
        public (int, (int, int)) GetBestMove()
        {
            // If the board is already full, fail.
            if (board.IsFull())
            {
                throw new InvalidOperationException("Board is full so a move can't be made.");
            }

            // Forward declare the variables for the next set of operations.
            int[] row_weights = new int[Board.SIZE];
            int[] col_weights = new int[Board.SIZE];
            int[] dag_weights = new int[Enum.GetNames(typeof(Diagonal)).Length];
            (int, int)[] row_moves = new (int, int)[Board.SIZE];
            (int, int)[] col_moves = new (int, int)[Board.SIZE];
            (int, int)[] dag_moves = new (int, int)[Enum.GetNames(typeof(Diagonal)).Length];

            // Determine weight and best move for every row.
            for (int row = 0; row < Board.SIZE; row++)
            {
                int weight = 0;
                for (int col = 0; col < Board.SIZE; col++)
                {
                    if (board.GetPiece(row, col) == null)
                    {
                        weight++;
                        row_moves[row] = (row, col);
                    }
                    else if (board.GetPiece(row, col) != piece)
                    {
                        weight = Board.SIZE;
                        break;
                    }
                }
                row_weights[row] = weight;
            }

            // Determine weight and best column for every column.
            for (int col = 0; col < Board.SIZE; col++)
            {
                int weight = 0;
                for (int row = 0; row < Board.SIZE; row++)
                {
                    if (board.GetPiece(row, col) == null)
                    {
                        weight++;
                        col_moves[col] = (row, col);
                    }
                    else if (board.GetPiece(row, col) != piece)
                    {
                        weight = Board.SIZE;
                        break;
                    }
                }
                col_weights[col] = weight;
            }

            // Determine weight for 0,0->SIZE-1, SIZE-1.
            {
                int weight = 0;
                for (int index = 0; index < Board.SIZE; index++)
                {
                    if (board.GetPiece(index, index) == null)
                    {
                        weight++;
                        dag_moves[0] = (index, index);
                    }
                    else if (board.GetPiece(index, index) != piece)
                    {
                        weight = Board.SIZE;
                        break;
                    }
                }
                dag_weights[(int)Diagonal.BL_TO_TR] = weight;
            }

            // Determine weight for 0,SIZE-1->SIZE-1, 0.
            {
                int weight = 0;
                for (int index = 0; index < Board.SIZE; index++)
                {
                    int row = Board.SIZE - 1 - index;
                    int col = index;
                    if (board.GetPiece(row, col) == null)
                    {
                        weight++;
                        dag_moves[1] = (index, index);
                    }
                    else if (board.GetPiece(row, col) != piece)
                    {
                        weight = Board.SIZE;
                        break;
                    }
                }
                dag_weights[(int)Diagonal.TL_TO_BR] = weight;
            }

            // Determine which dimension presents the best chance of winning.
            int[] dim_weights = new int[Enum.GetNames(typeof(Dimension)).Length];
            int[] dim_indices = new int[Enum.GetNames(typeof(Dimension)).Length];
            (int, int)[] dim_moves = new (int, int)[Enum.GetNames(typeof(Dimension)).Length];
            int dim_weight, dim_index;
            (int, int) dim_move;

            // Determine best move in each dimension with weights, i.e. rows, columns, and diagonals.
            (dim_weights[(int)Dimension.ROWS], dim_indices[(int)Dimension.ROWS]) = Min(row_weights);
            (dim_weights[(int)Dimension.COLS], dim_indices[(int)Dimension.COLS]) = Min(col_weights);
            (dim_weights[(int)Dimension.DAGS], dim_indices[(int)Dimension.DAGS]) = Min(dag_weights);
            dim_moves[(int)Dimension.ROWS] = row_moves[dim_indices[(int)Dimension.ROWS]];
            dim_moves[(int)Dimension.COLS] = col_moves[dim_indices[(int)Dimension.COLS]];
            dim_moves[(int)Dimension.DAGS] = dag_moves[dim_indices[(int)Dimension.DAGS]];

            // Determine best move overall.
            (dim_weight, dim_index) = Min(dim_weights);
            dim_move = dim_moves[dim_index];

            return (dim_weight, dim_move);
        }
        public (int, int)  GetRandomMove()
        {
            // If the board is already full, fail.
            if (board.IsFull())
            {
                throw new InvalidOperationException("Board is full so a move can't be made.");
            }

            // Find possible moves.
            List<(int, int)> moves = new List<(int, int)>();
            for (int row = 0; row < Board.SIZE; row++)
            {
                for (int col = 0; col < Board.SIZE; col++)
                {
                    if (board.GetPiece(row, col) == null)
                    {
                        moves.Add((row, col));
                    }
                }
            }

            // Randomly pick from the available moves.
            Random random = new Random();
            (int, int) move = moves[random.Next(0, moves.Count - 1)];
            return move;
        }
    }
    public class Displayer
    {
        private static readonly int TOTAL_SYMBOLS = 8;
        private static readonly int SYMBOL_SIZE = 5;
        private static readonly int BOARD_THICKNESS = 2;
        private static readonly char BOARD_SYMBOL = '=';
        private static readonly string[] PRETTY_SYMBOLS =
        {
            "     X   X OOO   11   22  333  4  4 5555",
            "      X X O   O 111  2  2    3 4  4 5   ",
            "       X  O   O  11    2   33  4444 555 ",
            "      X X O   O  11   2      3    4    5",
            "     X   X OOO  11l1 2222 333     4 555 "
        };

        public static void Display(Board board)
        {
            for (int i = 0; i < BOARD_THICKNESS; i++)
            {
                for (int j = 0; j < (BOARD_THICKNESS + Board.SIZE * (SYMBOL_SIZE + BOARD_THICKNESS)); j++)
                {
                    Console.Write(BOARD_SYMBOL);
                }
                Console.Write("\n");
            }
            for (int row = 0; row < Board.SIZE; row++)
            {
                for (int i = 0; i < SYMBOL_SIZE; i++)
                {
                    for (int j = 0; j < BOARD_THICKNESS; j++)
                    {
                        Console.Write(BOARD_SYMBOL);
                    }
                    for (int col = 0; col < Board.SIZE; col++)
                    {
                        int? piece = board.GetPiece(row, col);
                        int symbol = (piece == null) ? 0 : (piece.Value + 1);
                        if (symbol < 0 || symbol >= TOTAL_SYMBOLS)
                            throw new InvalidOperationException(String.Format("Symbol {0} with peice {1} is out of range [0, {2}-1].", symbol, piece, TOTAL_SYMBOLS));

                        for (int k = 0; k < SYMBOL_SIZE; k++)
                        {
                            Console.Write(PRETTY_SYMBOLS[i][(symbol * SYMBOL_SIZE) + k]);
                        }
                        for (int j = 0; j < BOARD_THICKNESS; j++)
                        {
                            Console.Write(BOARD_SYMBOL);
                        }
                    }
                    Console.Write("\n");
                }
                for (int i = 0; i < BOARD_THICKNESS; i++)
                {
                    for (int j = 0; j < (BOARD_THICKNESS + Board.SIZE * (SYMBOL_SIZE + BOARD_THICKNESS)); j++)
                    {
                        Console.Write(BOARD_SYMBOL);
                    }
                    Console.Write("\n");
                }
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            /*
             * Welcome Tic-Tac-Toe.
             * 
             * This program is simply an exercise to learn C# for the first time. Enjoy!
             */

            // Create board.
            Board board = new Board();

            // Create the players.
            const int HUMANS = 1;
            const int AIS = 1;
            List<Player> players = new List<Player>();

            // Add the human players.
            for (int index = 0; index < HUMANS; index++)
            {
                players.Add(new Human(players.Count, String.Format("Human{0}", index)));
            }

            // Add the AI players.
            int[] pieces = Enumerable.Range(0, HUMANS + AIS).ToArray();
            for (int index = 0; index < AIS; index++)
            {
                // Determine the opponents of the AI.
                int ai_piece = players.Count;
                int[] op_pieces = pieces.Where(piece => piece != ai_piece).ToArray();
                AI ai = new AI(ai_piece, op_pieces, String.Format("Robot{0}", index));
                players.Add(ai);
            }

            // Perform main loops.
            while (true)
            {
                // Perform the same operations for each player.
                foreach (Player player in players)
                {
                    Console.WriteLine("It is now {0} turn!", player.GetPlayerName());

                    // Display the board.
                    Displayer.Display(board);

                    // Player can go ahead and place a piece on the board.
                    player.SetPieceOnBoard(board);

                    // Check to see if player won.
                    int? winner_piece = board.GetWinner();
                    if (winner_piece == player.GetPlayerPiece())
                    {
                        Console.WriteLine("{0} is the winner! Woo!", player.GetPlayerName());
                        Displayer.Display(board);
                        Console.ReadKey();
                        return;
                    }

                    // If the board is full, it's definitely a draw.
                    if (board.IsFull())
                    {
                        Console.WriteLine("No winners!");
                        Displayer.Display(board);
                        Console.ReadKey();
                        return;
                    }
                }
            }
        }
    }
}
