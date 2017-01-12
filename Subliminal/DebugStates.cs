using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subliminal
{
    public static class DebugStates
    {
        private const int NUM_COLUMNS = 7;
        private const int NUM_ROWS = 6;

        public static void ImpossibleWin(BoardState bs)
        {
            // Initializes board to 0
            sbyte[][] board = new sbyte[NUM_ROWS][];

            for (int i = 0; i < board.Length; i++)
                board[i] = new sbyte[NUM_COLUMNS];

            // Sets custom values for state
            board[0] = new sbyte[] { 2, 0, 2, 2, 2, 1, 2 };
            board[1] = new sbyte[] { 1, 0, 2, 1, 1, 1, 1 };
            board[2] = new sbyte[] { 2, 0, 1, 1, 2, 2, 1 };
            board[3] = new sbyte[] { 2, 0, 2, 2, 2, 1, 2 };
            board[4] = new sbyte[] { 2, 2, 1, 1, 1, 2, 1 };
            board[5] = new sbyte[] { 1, 1, 2, 2, 1, 1, 1 };
            
            // Changes values
            bs.ManualChange(board, false);
        }
    }
}
