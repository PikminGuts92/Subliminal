using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subliminal
{
    public static class ComRoutines
    {
        public static List<BoardState> GetNextStates(BoardState bs, bool maxPlayer)
        {
            List<int> moves = bs.PossibleMoves();
            List<BoardState> states = new List<BoardState>();

            // Creates next states by possible moves
            foreach(int move in moves)
            {
                BoardState nextState = new BoardState(bs);
                nextState.CommitMove(move, bs.PlayerTurn);

                states.Add(nextState);
            }

            // Sorts states by rating
            if (maxPlayer)
            {
                states.Sort(delegate (BoardState a, BoardState b)
                {
                    return b.Rating - a.Rating;
                });
            }
            else
            {
                states.Sort(delegate (BoardState a, BoardState b)
                {
                    return a.Rating - b.Rating;
                });
            }

            return states;
        }

        public static int FindBestMove(BoardState bs, int lookAhead, bool player)
        {
            int bestMoveIndex;

            /*
            // Looks ahead once
            int rating = AlphaBeta(bs, 1, int.MinValue, int.MaxValue, !player, out bestMoveIndex);

            if ((player && rating == int.MinValue) || (!player && rating == int.MaxValue))
                return bestMoveIndex;
            
            // Then does n lookahead
            rating = AlphaBeta(bs, lookAhead, int.MinValue, int.MaxValue, !player, out bestMoveIndex);
            */

            AlphaBeta2(bs, lookAhead, int.MinValue, int.MaxValue, !player, out bestMoveIndex);
            return bestMoveIndex;
        }

        private static int AlphaBeta2(BoardState bs, int depth, int alpha, int beta, bool maxPlayer, out int bestMove)
        {
            bestMove = -1;

            List<int> moves = bs.PossibleMoves();
            if (depth == 0 || bs.Player1Solutions > 0
                || bs.Player2Soltuions > 0 || moves.Count == 0) // End state
            {
                return bs.Rating;
            }

            List<BoardState> nextStates = GetNextStates(bs, maxPlayer);

            if (maxPlayer)
            {
                foreach (BoardState nextState in nextStates)
                {
                    int moveIdx;
                    int rating = AlphaBeta2(nextState, depth - 1, alpha, beta, false, out moveIdx);

                    // Sets max alpha
                    if (rating > alpha)
                    {
                        alpha = rating;
                        bestMove = nextState.LastMove;
                    }

                    if (beta <= alpha) // Beta cuts off
                    {
                        break;
                    }
                }

                return alpha;
            }
            else
            {
                foreach (BoardState nextState in nextStates)
                {
                    int moveIdx;
                    int rating = AlphaBeta2(nextState, depth - 1, alpha, beta, true, out moveIdx);

                    // Sets min beta
                    if (rating < beta)
                    {
                        beta = rating;
                        bestMove = nextState.LastMove;
                    }

                    if (beta <= alpha) // Alpha cuts off
                    {
                        break;
                    }
                }

                return beta;
            }
        }

        private static int AlphaBeta(BoardState bs, int depth, int alpha, int beta, bool maxPlayer, out int bestMove)
        {
            bestMove = -1;

            List<int> moves = bs.PossibleMoves();
            if (depth == 0 || bs.Player1Solutions > 0
                || bs.Player2Soltuions > 0 || moves.Count == 0) // End state
            {
                return bs.Rating;
            }

            if (maxPlayer)
            {
                int v = int.MinValue;

                foreach (int move in moves)
                {
                    BoardState nextState = new BoardState(bs);
                    nextState.CommitMove(move, nextState.PlayerTurn);

                    int moveIdx;
                    int rating = AlphaBeta(nextState, depth - 1, alpha, beta, false, out moveIdx);

                    // Sets max alpha
                    if (rating > alpha)
                    {
                        alpha = rating;
                        bestMove = move;
                    }

                    if (beta <= alpha) // Beta cuts off
                    {
                        break;
                    }
                }

                return alpha;
            }
            else
            {
                int v = int.MaxValue;

                foreach (int move in moves)
                {
                    BoardState nextState = new BoardState(bs);
                    nextState.CommitMove(move, nextState.PlayerTurn);

                    int moveIdx;
                    int rating = AlphaBeta(nextState, depth - 1, alpha, beta, true, out moveIdx);

                    // Sets min beta
                    if (rating < beta)
                    {
                        beta = rating;
                        bestMove = move;
                    }
                    
                    if (beta <= alpha) // Alpha cuts off
                    {
                        break;
                    }
                }

                return beta;
            }
        }
    }
}
