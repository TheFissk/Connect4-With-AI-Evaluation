using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Connect_4
{
    class Program
    {
        static void Main(string[] args)
        {
            TestSession(1, 600000); //10 minute test
            TestSession(5, 600000); //10 minute test
            TestSession(10, 600000); //10 minute test
            Console.Read();
        }

        static void TestSession (int depth, int DesiredTestTimeInMilliseconds)
        {
            Playfield playfield = new Playfield();
            AIEvaluator AI = new AIEvaluator();
            Random random = new Random();

            int[] Losses = { 0, 0 };
            int Draws = 0;
            int moves = 0;
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Restart();
            while (stopwatch.ElapsedMilliseconds < DesiredTestTimeInMilliseconds) //10 Minutes
            {
                playfield.ResetBoard();
                int AIActor = 0;
                while (playfield.Status == PlayStatus.IncompleteGame && stopwatch.ElapsedMilliseconds < DesiredTestTimeInMilliseconds)
                {
                    playfield.MakeMoveNoErrorChecking(AI.BestMove(playfield, AIActor, depth));
                    moves++;
                    if (AIActor == 0) AIActor = 1;
                    else AIActor = 0;
                }
                if (playfield.Status == PlayStatus.Draw) Draws++;
                else
                {
                    Losses[playfield.WhoseTurnIsIt()]++;
                }
            }
            stopwatch.Stop();
            Console.WriteLine($"Depth {depth} Test Complete. Duration was {stopwatch.ElapsedMilliseconds / 1000} Seconds ");
            Console.WriteLine($"Calculated {moves} moves, which is {(moves / stopwatch.ElapsedMilliseconds) / 1000} per second \n This was {Losses[0] + Losses[1] + Draws} games. Player 1 won {Losses[1]}, Player 2 won {Losses[0]}, and {Draws} were drawn.");
        }
    }

    class Playfield
    {
        //the gameboard is 7x6

        /// <summary>
        /// Width of the board
        /// </summary>
        public readonly int Width;
        /// <summary>
        /// One higher than the actual board height. This is required for speed purposes
        /// </summary>
        public readonly int Height;

        /// <summary>
        /// Returns the board with 1s for player 0 and -1s for player 1.
        /// </summary>
        /// <returns></returns>
        public int[,] GetBoard()
        { return RenderBoard(playerBoards[0], playerBoards[1], Width, Height); }

        /// <summary>
        /// Returns the board with 1s for player 0 and -1s for player 1.
        /// </summary>
        /// <returns></returns>
        public int[,] GetBoardFlipped()
        { return RenderBoard(playerBoards[1], playerBoards[0], Width, Height); } // returns the board as if player 2 is player 1

        public int WhoseTurnIsIt() //returns number of the player whose turn it is
        { return turn % 2; }

        public long GetPlayerboardAsLong(int player)
        { return playerBoards[player]; }

        public bool[] LegalMoves()
        { return LegalMoveArray(); }

        public PlayStatus Status;

        private int turn;
        private long[] playerBoards;
        private List<int> moves;
        private int[] rowheight;

        public Playfield()
        {
            Width = 7;
            Height = 7;
            turn = 0;
            playerBoards = new long[] { 0, 0 };
            moves = new List<int>();
            rowheight = new int[] { 0, 7, 14, 21, 28, 35, 42 };
            Status = PlayStatus.IncompleteGame;
        }

        /// <summary>
        /// Makes a move on the board
        /// </summary>
        /// <param name="column">the column you want to make the move against</param>
        /// <returns>returns false if the move is illegal</returns>
        public bool MakeMove(int column)
        {
            if (!IsMoveLegal(column)) return false;

            _makeMove(column);

            if (CheckforWin(playerBoards[(turn - 1) % 2])) Status = PlayStatus.Win;
            else if (turn == 42) Status = PlayStatus.Draw;
            return true;
        }

        /// <summary>
        /// Makes a move on the board, with no error checking, so illegal moves can be made. If an illegal move is made then the output becomes unpredictable
        /// </summary>
        /// <param name="column">the column you want to make the move against</param>
        /// <returns>returns true if the game is over (either a win or a Draw)</returns>
        public bool MakeMoveNoErrorChecking(int column)
        {
            _makeMove(column);

            if (CheckforWin(playerBoards[(turn - 1) % 2]))
            {
                Status = PlayStatus.Win;
                return true;
            }
            else if (turn == 42) {
                Status = PlayStatus.Draw;
                return true;
            }
            return false;
        }

        public void ResetBoard()
        {
            turn = 0;
            playerBoards = new long[] { 0, 0 };
            moves = new List<int>();
            rowheight = new int[] { 0, 7, 14, 21, 28, 35, 42 };
            Status = PlayStatus.IncompleteGame;
        }

        public void UndoMove()
        {
            turn--;
            int column = moves[turn];   // grab the move from the move history
            moves.RemoveAt(turn);       //remove that move from the history
            rowheight[column]--;
            long move = 1L << rowheight[column];
            playerBoards[turn % 2] ^= move;
            Status = PlayStatus.IncompleteGame;
        }

        private void _makeMove(int column)
        {
            long move = 1L << rowheight[column]++;  //store a bit where the stone was played
            playerBoards[turn % 2] ^= move;         //add that stone to the appriopriate position (using XOR)
            turn++;
            moves.Add(column);                      //add the move to the board
        }

        private Boolean CheckforWin(long bitboard)
        {
            int[] directions = { 1, 7, 6, 8 }; //checking in this order = | - \ /
            long bb;
            foreach (int direction in directions)
            {
                bb = bitboard & (bitboard >> direction);
                if ((bb & (bb >> (2 * direction))) != 0) return true; //https://github.com/denkspuren/BitboardC4/blob/master/BitboardDesign.md
            }
            return false;
        }

        public void PrintBoardToConsole()
        {
            int[,] _board = RenderBoard(playerBoards[0], playerBoards[1], Width, Height);
            for (int row = 0; row < Height; row++)
            {
                for (int column = 0; column < Width; column++)
                {
                    Console.Write($"{_board[column, row]} ");
                }
                Console.WriteLine();
            }
        }
        public void TestRender()
        {
            BitArray player0 = new BitArray(BitConverter.GetBytes(playerBoards[0]));
            BitArray player1 = new BitArray(BitConverter.GetBytes(playerBoards[1]));
            for (int i = 0; i < player0.Length; i++)
            {
                if (player0[i]) Console.Write("1");
                else Console.Write("0");
            }
            Console.WriteLine();
            for (int i = 0; i < player1.Length; i++)
            {
                if (player1[i]) Console.Write("1");
                else Console.Write("0");
            }
            Console.WriteLine();
        }
        public int[,] RenderBoard(long player0, long player1, int width, int height)
        {
            int[,] result = new int[width, height];
            BitArray playerZero = new BitArray(BitConverter.GetBytes(player0));
            BitArray playerOne = new BitArray(BitConverter.GetBytes(player0));
            for (int column = 0; column < width; column++)
            {
                for (int row = height - 1; row >= 0; row--)
                {
                    int printrow = height - row - 1;
                    if (playerZero[column * height + row]) result[column, printrow] = -1;
                    else if (playerOne[column * height + row]) result[column, printrow] = 1;
                    else result[column, printrow] = 0;
                }
            }
            return result;
        }

        private bool[] LegalMoveArray()
        {
            bool[] result = new bool[Width];
            for (int i = 0; i < Width; i++)
            {
                result[i] = IsMoveLegal(i);
            }
            return result;
        }

        public bool IsMoveLegal(int move)
        {
            if (move > Width) return false;
            if (rowheight[move] >= move * Height + Height - 1) return false;
            return true;
        }
    }


    class AIEvaluator
    {
        /// <summary>
        /// Determines the best move available
        /// </summary>
        /// <param name="playfield">The board in question</param>
        /// <param name="AIActor">The player number that represents the AI (0 or 1)</param>
        /// <param name="depth">The depth that we are evaluating to</param>
        /// <returns>Returns the column that has the best move</returns>
        public int BestMove(Playfield playfield, int AIActor, int depth = 1)
        {
            int bestmove = 0;
            float alpha;
            bool AIActorIsPlayer0;
            //set the alpha to the worse possible value for us, and a bool for which character the AI Actor is
            if (AIActor == 0)
            {
                alpha = float.MinValue;
                AIActorIsPlayer0 = true;
            }
            else
            {
                alpha = float.MaxValue;
                AIActorIsPlayer0 = false;
            }

            float LocalAlpha = 0;

            bool[] IsALegalMove = playfield.LegalMoves();


            for (int i = 0; i < playfield.Width; i++)
            {

                if(IsALegalMove[i])
                {
                    if (playfield.MakeMoveNoErrorChecking(i)) //returns true if the game is over (draw or win)
                    {
                        if (playfield.Status == PlayStatus.Win)
                        {
                            playfield.UndoMove();   //undo the move so that the board is in its original state
                            return i;               //if the move wins then its the best move. Easy
                        }
                        else if (playfield.Status == PlayStatus.Draw) LocalAlpha = 0; //draws return 0
                    } else
                    {
                        //BELOW HERE YOU CAN CHOOSE WHICH EVALUATION STYLE YOU WANT
                        LocalAlpha = EvaluatePositionNoOptimizations(playfield, true, depth - 1);
                    }

                    playfield.UndoMove(); //reset the board to its original condition
                    if ((AIActorIsPlayer0 && LocalAlpha > alpha) || (!AIActorIsPlayer0 && LocalAlpha < alpha))
                    {
                        alpha = LocalAlpha;
                        bestmove = i;
                    }
                }
            }
            return bestmove;
        }


        /// <summary>
        /// Evaluates the board, looking for the best position for the AI Actor
        /// </summary>
        /// <param name="playfield">The board in question</param>
        /// <param name="IsOpponentsMove">Is the player playing right now the AI actor or the opponent, Defaults to it being the AI actor</param>
        /// <param name="depth">remaining depth to evaluate. Defaults to 0</param>
        /// <param name="alphaValue">alpha of the current best move. Needed for A/B pruning. Defaults to the AI actor is player 0</param>
        /// <returns>returns the evaluation of the board position for player 0</returns>
        private float EvaluatePositionNoOptimizations(Playfield playfield, bool IsOpponentsMove = false, int depth = 0)
        {
            //check to see if we've reached the bottom of the evaluation tree. If we have evaluate that position
            if (depth < 1)
            {
                return Evaluate(playfield.GetBoard());
            }

            float LocalAlpha; //LocalAlpha is the value "best" move found so far for the players whose move it is, defaults to the worst value.
            float CurrentMoveAlpha = 0;
            if (playfield.WhoseTurnIsIt() == 0)
                LocalAlpha = float.MinValue;
            else
                LocalAlpha = float.MaxValue;

            bool[] IsALegalMove = playfield.LegalMoves();

            for (int i = 0; i < playfield.Width; i++)
            {

                if (IsALegalMove[i])
                {
                    if (playfield.MakeMoveNoErrorChecking(i)) //returns true if the game is over (draw or win)
                    {
                        if (playfield.Status == PlayStatus.Win)
                        {
                            playfield.UndoMove();
                            if (playfield.WhoseTurnIsIt() == 0)
                                return float.MaxValue;
                            else
                                return float.MinValue;
                        }
                        else if (playfield.Status == PlayStatus.Draw) CurrentMoveAlpha = 0; //draws return 0
                    }
                    else
                    {
                        CurrentMoveAlpha = EvaluatePositionNoOptimizations(playfield, true, depth - 1);
                    }

                    playfield.UndoMove(); //reset the board to its original condition

                    if ((playfield.WhoseTurnIsIt() == 0 && CurrentMoveAlpha > LocalAlpha) || (playfield.WhoseTurnIsIt() != 0 && CurrentMoveAlpha < LocalAlpha))
                    {
                        LocalAlpha = CurrentMoveAlpha;
                    }
                }
            }
            return LocalAlpha;
        }


        /// <summary>
        /// Evaluates a position. positive is good for the player represented by 1s, negative is good for the player represented by -1s
        /// </summary>
        /// <param name="board">put 1 for player 1, -1 for player 2. 0 for blank squares</param>
        /// <returns></returns>
        private float Evaluate(int[,] board)
        {
            Random random = new Random();
            return random.Next(-100,100);
        }
    }
    enum PlayStatus
    {
        Win,
        Draw,
        IncompleteGame,
    }
}
