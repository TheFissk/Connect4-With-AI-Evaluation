using System;

public class AIEvaluator
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

            if (IsALegalMove[i])
            {
                if (playfield.MakeMoveNoErrorChecking(i)) //returns true if the game is over (draw or win)
                {
                    if (playfield.Status == PlayStatus.Win)
                    {
                        playfield.UndoMove();   //undo the move so that the board is in its original state
                        return i;               //if the move wins then its the best move. Easy
                    }
                    else if (playfield.Status == PlayStatus.Draw) LocalAlpha = 0; //draws return 0
                }
                else
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
        return random.Next(-100, 100);
    }
}