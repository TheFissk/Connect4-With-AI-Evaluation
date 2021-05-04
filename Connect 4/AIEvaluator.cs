using System;

namespace Connect_4
{
    /// <summary>
    /// The GA is 5x5x5, why? because I feel like it
    /// </summary>
    internal class AIEvaluator
    {
        NodeGroup InputNodeGroup;
        /// <summary>
        /// Generates a completely new AI with completely random weights
        /// </summary>
        public AIEvaluator(int InputSize, int OutputSize)
        {
            InputNodeGroup = new NodeGroup(InputSize);
        }

        #region Classic AI Methods
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

        #endregion

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

        private class NodeGroup
        {

            public Node[] Nodes;
            public int Size;

            /// <summary>
            /// creates a NodeGroup that isn't linked with another Nodegroup (aka the inputs)
            /// </summary>
            /// <param name="values"></param>
            public NodeGroup(int size)
            {
                Size = size;
            }

            /// <summary>
            /// Creates a Nodegroup thats linked to the existing nodegroup with new weights
            /// </summary>
            /// <param name="InputNodes">The nodes that its linked to</param>
            /// <param name="size">The size of the group</param>
            public NodeGroup(NodeGroup InputNodes, int size)
            {
                Nodes = new Node[size];
                for (int i = 0; i < size; i++)
                {

                }
            }

            /// <summary>
            /// Sets the output Values of the nodes, most useful for input nodes
            /// </summary>
            /// <param name="Values">the values you want to set</param>
            void SetNodeValues(double[] Values)
            {
                Nodes = new Node[Values.Length];
                for (int i = 0; i < Values.Length; i++)
                {
                    Nodes[i] = new Node(Values[i]);
                }
            }


        }
        private class Node
        {
            public double OutputValue;
            public NodeGroup InputNodes;
            public double[] Weights;

            /// <summary>
            /// For an input node with no connected values
            /// </summary>
            /// <param name="value">The Value of the node</param>
            public Node(double value)
            {
                OutputValue = value;
            }

            /// <summary>
            /// Creates a new node with all new Nodes
            /// </summary>
            /// <param name="nodeGroup"></param>
            public Node(NodeGroup nodeGroup)
            {
                InputNodes = nodeGroup;
                Weights = new double[nodeGroup.Size];
                Random random = new Random();
                for (int i = 0; i < Weights.Length; i++)
                {
                    Weights[i] = random.NextDouble();
                }
            }

            /// <summary>
            /// Calculates the OutputValue
            /// </summary>
            /// <returns>returns the OutputValue</returns>
            public double Evaluate()
            {
                OutputValue = 0;
                for (int i = 0; i < InputNodes.Size; i++)
                {
                    OutputValue += InputNodes.Nodes[i].OutputValue * Weights[i];
                }
                return OutputValue;
            }
        }
    }
}