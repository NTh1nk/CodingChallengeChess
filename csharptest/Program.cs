using System;
using System.Collections.Generic;

namespace MinimaxExample
{
    class Program
    {
        static void Main(string[] args)
        {
            List<List<List<List<int>>>> board = new List<List<List<List<int>>>>
            {
                new List<List<List<int>>>
                {
                    new List<List<int>>
                    {
                        new List<int> { -1, 3 },
                        new List<int> { 5, 1 }
                    }
                },
                new List<List<List<int>>>
                {
                    new List<List<int>>
                    {
                        new List<int> { -6, -4 },
                        new List<int> { 0, 9 }
                    }
                }
            };

            int infinity = int.MaxValue;
            int maxDepth = 2;

            int Minimax(List<List<List<List<int>>>> board, int depth, int currentPlayer, double _min, double _max)
            {
                //var moves = board;
                //bool notEval = false;
                foreach (var pm1 in board)
                {
                    foreach (var pm2 in pm1)
                    {
                        foreach (var pm3 in pm2)
                        {

                            foreach (var moveVal in pm3)
                            {
                                
                                    
                    
                                int bMoveMat = -infinity * currentPlayer;

                                int v = moveVal;
                                Console.WriteLine("Evaluated move to be " + moveVal);
                                // v = Evaluate(newBoard); // You need to implement your evaluation function here

                                if (currentPlayer > 0)
                                {
                                    bMoveMat = Math.Max(bMoveMat, v);
                                    _min = Math.Max(_min, v);

                                    if (v >= _max)
                                    {
                                        Console.WriteLine("Cut off max");
                                        break;
                                    }
                                }
                                else
                                {
                                    bMoveMat = Math.Min(bMoveMat, v);
                                    _max = Math.Min(_max, v);

                                    if (v <= _min)
                                    {
                                        Console.WriteLine("Cut off min");
                                        break;
                                    }
                                }
                                
                            }
                        }
                    }
                }
                

                
            }

            Console.WriteLine(Minimax(board, maxDepth, 1, -infinity, infinity));
        }
    }
}