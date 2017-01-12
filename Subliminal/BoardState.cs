using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subliminal
{
    public class BoardState
    {
        /*   0 1 2 3 . . c
         * 1 0 0 0 0 0 0 0
         * 2 0 . . . . . 0
         * 3 0 . . . . . 0
         * . 0 . . . . . 0
         * . 0 . . . . . 0
         * r 0 0 0 0 0 0 0
         */
        private sbyte[][] _board;
        private int _rowSize;
        private int _columnSize;
        private bool _playerTurn;
        private const int CONNECT = 4; // Number of discs needed in a row
        private int _lastMove;

        // Rating stuff
        private int _rating;
        private int _p1Solutions;
        private int _p2Solutions;

        public delegate void MoveHandler(Position pos, bool player);
        public event MoveHandler PlayerMoved;

        protected virtual void OnPlayerMove(Position pos, bool player)
        {
            _lastMove = pos.Column;

            MoveHandler handler = PlayerMoved;
            if (handler == null) return; // No subscribers

            handler(pos, player);
        }

        public BoardState(int rowSize, int columnSize)
        {
            // Initial state

            // Sets row + col sizes
            _rowSize = rowSize;
            _columnSize = columnSize;

            InitializeBoard(_rowSize, _columnSize);
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="bs">Board State</param>
        public BoardState(BoardState bs)
        {
            // Copies all values
            _board = bs._board.Select(a => a.ToArray()).ToArray(); // Deep copy
            _rowSize = bs._rowSize;
            _columnSize = bs._columnSize;
            _playerTurn = bs._playerTurn;
            _rating = bs._rating;

            _p1Solutions = bs._p1Solutions;
            _p2Solutions = bs._p2Solutions;
        }

        // For debugging!
        public void ManualChange(sbyte[][] board, bool playerTurn)
        {
            // Copies all values
            _board = board.Select(a => a.ToArray()).ToArray(); // Deep copy
            _rowSize = board.Length;

            // Sets row + col sizes
            if (_rowSize > 0) _columnSize = board[0].Length;
            else _columnSize = 0;

            //  Calculates ratings
            _rating = CalculateRating(out _p1Solutions, out _p2Solutions);

            // Sets player turn
            _playerTurn = playerTurn;
            _lastMove = -1;

            // Calls move function
            for (int col = 0; col < _columnSize; col++)
            {
                for (int row = _rowSize - 1; row >= 0; row--)
                {
                    sbyte color = _board[row][col];
                    if (color == 0) break; // No discs should be above

                    OnPlayerMove(new Position(row, col), color == 2);
                }
            }
        }

        private void InitializeBoard(int rowSize, int columnSize)
        {
            // Sets player turn to P1
            _playerTurn = false;
            _lastMove = -1;
            
            // Initializes board to 0
            _board = new sbyte[rowSize][];

            for (int i = 0; i < rowSize; i++)
                _board[i] = new sbyte[columnSize];

            // Resets ratings
            _rating = 0;
            _p1Solutions = 0;
            _p2Solutions = 0;
        }

        public void Reset()
        {
            InitializeBoard(_rowSize, _columnSize);
        }

        #region Rating Calculation (Utility Function)
        private int CalculateRating(out int p1Solutions, out int p2Solutions)
        {
            // Utility function
            
            int p1Score = 0, p2Score = 0;
            p1Solutions = 0;
            p2Solutions = 0;

            // Used to avoid overlapping crossings
            List<Position> p1Horizontals = new List<Position>();
            List<Position> p1Verticals = new List<Position>();
            List<Position> p1DiagonalsLeft = new List<Position>();
            List<Position> p1DiagonalsRight = new List<Position>();

            List<Position> p2Horizontals = new List<Position>();
            List<Position> p2Verticals = new List<Position>();
            List<Position> p2DiagonalsLeft = new List<Position>();
            List<Position> p2DiagonalsRight = new List<Position>();

            // Starts from the bottom row
            for (int col = 0; col < _columnSize; col++)
            {
                for (int row = _rowSize - 1; row >= 0; row--)
                {
                    sbyte color = _board[row][col];
                    if (color == 0) break; // No discs should be above
                    else if (color == 1)
                    {
                        // Player 1
                        int hScore, vScore, dScoreLeft, dScoreRight;
                        int hSolutions, vSolutions, dSolutionsLeft, dSolutionsRight;

                        // Evaluates disc placement (All return a value between 0 and 4)
                        hScore = HorizontalScore(row, col, color, p1Horizontals, out hSolutions);
                        vScore = VerticalScore(row, col, color, p1Verticals, out vSolutions);
                        dScoreLeft = DiagonalScoreDown(row, col, color, p1DiagonalsLeft, out dSolutionsLeft);
                        dScoreRight = DiagonalScoreUp(row, col, color, p1DiagonalsRight, out dSolutionsRight);

                        // Adds to total
                        p1Score += hScore + vScore + dScoreLeft + dScoreRight;
                        p1Solutions += hSolutions + vSolutions + dSolutionsLeft + dSolutionsRight;
                    }
                    else if (color == 2)
                    {
                        // Player 2
                        int hScore, vScore, dScoreLeft, dScoreRight;
                        int hSolutions, vSolutions, dSolutionsLeft, dSolutionsRight;

                        // Evaluates disc placement (All return a value between 0 and 4)
                        hScore = HorizontalScore(row, col, color, p2Horizontals, out hSolutions);
                        vScore = VerticalScore(row, col, color, p2Verticals, out vSolutions);
                        dScoreLeft = DiagonalScoreDown(row, col, color, p2DiagonalsLeft, out dSolutionsLeft);
                        dScoreRight = DiagonalScoreUp(row, col, color, p2DiagonalsRight, out dSolutionsRight);

                        // Adds to total
                        p2Score += hScore + vScore + dScoreLeft + dScoreRight;
                        p2Solutions += hSolutions + vSolutions + dSolutionsLeft + dSolutionsRight;
                    }
                }
            }
            
            if (p1Solutions > 0)
                return int.MaxValue;
            else if (p2Solutions > 0)
                return int.MinValue;
            else
                return p1Score - p2Score;
        }

        private int DiagonalScoreDown(int row, int col, sbyte color, List<Position> previous, out int solutionScore)
        {
            int score = 0;
            solutionScore = 0;

            // Goes from left to right, from up to down (negative slope)
            for (int i = CONNECT - 1; i >= 0; i--)
            {
                int startCol = col - i;
                int startRow = row - i;
                int runningScore = 0;

                for (int ii = 0; ii < CONNECT; ii++)
                {
                    Position pos = new Position(startRow + ii, startCol + ii);

                    // Skips if out of bounds
                    if (!WithinBounds(pos)) break;

                    sbyte selDisc = _board[startRow + ii][startCol + ii];
                    if (!(selDisc == 0 || selDisc == color) || previous.Contains(pos)) break;

                    // Checks if it's a solution
                    if (selDisc == color) runningScore++;

                    // Adds score (Last disc)
                    if (ii == CONNECT - 1)
                    {
                        previous.Add(new Position(startRow, startCol));
                        if (runningScore == CONNECT)
                        {
                            solutionScore++;
                            score += 1000;
                            continue;
                        }

                        // 1 = 2^0, 2 = 2^2, 3 = 2^4, 4 = 2^10
                        score += 1 << ((runningScore - 1) << 1);
                    }
                }
            }

            return score;
        }

        private int DiagonalScoreUp(int row, int col, sbyte color, List<Position> previous, out int solutionScore)
        {
            int score = 0;
            solutionScore = 0;

            // Goes from right to left, from down to up (negative slope)
            for (int i = CONNECT - 1; i >= 0; i--)
            {
                int startCol = col + i;
                int startRow = row - i;
                int runningScore = 0;

                for (int ii = 0; ii < CONNECT; ii++)
                {
                    Position pos = new Position(startRow + ii, startCol - ii);

                    // Skips if out of bounds
                    if (!WithinBounds(pos)) break;

                    sbyte selDisc = _board[startRow + ii][startCol - ii];
                    if (!(selDisc == 0 || selDisc == color) || previous.Contains(pos)) break;

                    // Checks if it's a solution
                    if (selDisc == color) runningScore++;

                    // Adds score (Last disc)
                    if (ii == CONNECT - 1)
                    {
                        previous.Add(new Position(startRow, startCol));
                        if (runningScore == CONNECT)
                        {
                            solutionScore++;
                            score += 1000;
                            continue;
                        }

                        // 1 = 2^0, 2 = 2^2, 3 = 2^4, 4 = 2^10
                        score += 1 << ((runningScore - 1) << 1);
                    }
                }
            }

            return score;
        }

        private int HorizontalScore(int row, int col, sbyte color, List<Position> previous, out int solutionScore)
        {
            int score = 0;
            solutionScore = 0;

            // Goes from left to right
            for (int i = CONNECT - 1; i >= 0; i--)
            {
                int startCol = col - i;
                int runningScore = 0;

                for (int ii = 0; ii < CONNECT; ii++)
                {
                    Position pos = new Position(row, startCol + ii);

                    // Skips if out of bounds
                    if (!WithinBounds(pos)) break;

                    sbyte selDisc = _board[row][startCol + ii];
                    if (!(selDisc == 0 || selDisc == color) || previous.Contains(pos)) break;

                    // Checks if it's a solution
                    if (selDisc == color) runningScore++; 

                    // Adds score (Last disc)
                    if (ii == CONNECT - 1)
                    {
                        previous.Add(new Position(row, startCol));
                        if (runningScore == CONNECT)
                        {
                            solutionScore++;
                            score += 1000;
                            continue;
                        }

                        // 1 = 2^0, 2 = 2^2, 3 = 2^4, 4 = 2^10
                        score += 1 << ((runningScore - 1) << 1);
                    }
                }
            }

            return score;
        }

        private int VerticalScore(int row, int col, sbyte color, List<Position> previous, out int solutionScore)
        {
            int score = 0;
            solutionScore = 0;

            // Goes from up to down
            for (int i = CONNECT - 1; i >= 0; i--)
            {
                int startRow = row - i;
                int runningScore = 0;

                for (int ii = 0; ii < CONNECT; ii++)
                {
                    Position pos = new Position(startRow + ii, col);

                    // Skips if out of bounds
                    if (!WithinBounds(pos)) break;

                    sbyte selDisc = _board[startRow + ii][col];
                    if (!(selDisc == 0 || selDisc == color) || previous.Contains(pos)) break;

                    // Checks if it's a solution
                    if (selDisc == color) runningScore++;

                    // Adds score
                    if (ii == CONNECT - 1)
                    {
                        previous.Add(new Position(startRow, col));
                        if (runningScore == CONNECT)
                        {
                            solutionScore++;
                            score += 1000;
                            continue;
                        }

                        // 1 = 2^0, 2 = 2^2, 3 = 2^4, 4 = 2^10
                        score += 1 << ((runningScore - 1) << 1);
                    }
                }
            }
            
            return score;
        }

        private bool WithinBounds(Position pos)
        {
            // Returns false if out of bounds
            if (pos.Column < 0 || pos.Column >= _columnSize
                || pos.Row < 0 || pos.Row >= _rowSize) return false;

            return true;
        }
        #endregion

        #region Game Logic
        /// <summary>
        /// Gets list of possible moves
        /// </summary>
        /// <returns>Index of possible moves</returns>
        public List<int> PossibleMoves()
        {
            // Successor function
            List<int> moves = new List<int>();

            for(int i = 0; i < _columnSize; i++)
            {
                if (_board[0][i] == 0) moves.Add(i);
            }

            return moves;
        }

        /// <summary>
        /// Determines if selected column is a legal move
        /// </summary>
        /// <param name="column">Column Index</param>
        /// <param name="player">Player (False = P1, True = P2)</param>
        /// <returns>Legal?</returns>
        public bool LegalMove(int column, bool player)
        {
            // Returns false if out of bounds
            if (player != _playerTurn || column < 0 || column >= _columnSize) return false;

            for (int row = 0; row < _rowSize; row++)
            {
                if (_board[row][column] == 0) return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to commit selected move
        /// </summary>
        /// <param name="column">Column Index</param>
        /// <param name="player">Player (False = P1, True = P2)</param>
        /// <returns>Row index of replaced disc (-1 = illegal move)</returns>
        public int CommitMove(int column, bool player)
        {
            // Returns -1 if illegal move
            if (!LegalMove(column, player)) return -1;

            int row = 0;

            // Finds last empty row in column
            while (row < _rowSize)
            {
                // Breaks if disc if found
                if (_board[row][column] != 0)
                    break;

                row++;
            }

            // Decrements index
            if (row != 0) row--;

            // Commits move
            _board[row][column] = (sbyte)(player ? 2 : 1);
            _playerTurn = !_playerTurn; // Alternates turn

            // Calls event + Re-calculates ratings
            _rating = CalculateRating(out _p1Solutions, out _p2Solutions);
            OnPlayerMove(new Position(row, column), player);

            // Returns row index
            return row;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets player's turn (False = P1, True = P2)
        /// </summary>
        public bool PlayerTurn { get { return _playerTurn; } }

        /// <summary>
        /// Gets row size
        /// </summary>
        public int RowSize { get { return _rowSize; } }

        /// <summary>
        /// Gets column size
        /// </summary>
        public int ColumnSize { get { return _columnSize; } }

        /// <summary>
        /// Gets player 1 solution count
        /// </summary>
        public int Player1Solutions { get { return _p1Solutions; } }

        /// <summary>
        /// Gets player 2 solution count
        /// </summary>
        public int Player2Soltuions { get { return _p2Solutions; } }

        /// <summary>
        /// Gets board rating
        /// </summary>
        public int Rating { get { return _rating; } }

        /// <summary>
        /// Gets last move
        /// </summary>
        public int LastMove { get { return _lastMove; } }
        #endregion
    }
}
