﻿using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

public class MyBot : IChessBot
{
    // right now funktions are seperated. before submision, everything will be compacted into the think function.

    //example code
    //public Move Think(Board board, Timer timer)
    //{
    //    Move[] moves = board.GetLegalMoves();
    //    return moves[0];
    //}

    // how much each piece is worth
    float totalPieceValue;
    bool weAreWhite;
    //double arrCenterDistance = 33333333322222233211112332100123321001233211112332222223333333330.0; DOES NOT WORK because its floating point number
    string pieceSqareValues;  // kinda does work but hacky solution
    int[] pieceValues = {100, 300, 320, 500, 900, 2000 };
    int[] arrCenterDistanceInt;
    List<Move> draw_moves = new();

    public bool IsEndgameNoFunction = false;
    public bool IsEndgame(Board board){

       //Console.WriteLine(board.GetAllPieceLists()); 
        totalPieceValue = 0;
        for (int x = 0; x <= 7; x++)
        {
            for (int y = 0; y <= 7; y++)
            {
                var s = new Square(x, y);
                var p = board.GetPiece(s); // quite slow
                if (p.IsNull)
                {
                    continue;
                }
                totalPieceValue += getPieceValue(p.PieceType, x, p.IsWhite ? y : 7 - y);
                

            }
        }
        if (totalPieceValue <= 511800)
            return true;
        else
            return false;
        

    }
    public Move Think(Board board, Timer timer)
    {
        
        pieceSqareValues = toPieceArray(new[] { 1010101018181818, 1212141611111215, 1010101411090810, 1112120610101010, 0002040402061010, 0410121304111314, 0410131404111213, 0206101100020404, 0608080808101010, 0810111208111112, 0810121208121212, 0811101006080808, 1010101011121212, 0910101009101010, 0910101009101010, 0910101010101011, 0608080908101010, 0810111109101111, 1010111108111111, 0810111006080809, 0402020004020200, 0402020004020200, 0604040208060606, 1414101014161210,
        1010101036303230, 2015181412121413, 1212121211111111, 0909090910101010 }); // use https://onlinestringtools.com/split-string to split into 16 long parts
        //Botton is endgame
        //arrCenterDistanceInt = toPieceArray(arrCenterDistance);                                                                                                                                                                                                                                                                                                                                                                                                                                       
        //Console.WriteLine(pieceSqareValues.Length);
        //Console.WriteLine(getPieceValue(PieceType.King, 7, 7));


        weAreWhite = board.IsWhiteToMove;
        /*#DEBUG*/Console.WriteLine(" ------ calculate new move -----" + board.IsWhiteToMove);
        var bestMove = miniMax(board, timer.MillisecondsRemaining < 20000 ? timer.MillisecondsRemaining < 5000 ? 2 : 3 : 4, weAreWhite ? 1 : -1, -1000000, 1000000).Item1;
        bestMove.ToList().ForEach(move => { Console.WriteLine(move); });
        if (IsEndgame(board)){
            IsEndgameNoFunction = true;
            /*#DEBUG*/Console.WriteLine("We are in the endgame");
        }
        return bestMove[bestMove.Length - 1];
        //Console.WriteLine(isPieceProtectedAfterMove(board, moves[0]));
        
    }

    private Tuple<Move[], float> miniMax(Board board, int depth, int currentPlayer, float min, float max)
    {
        Move[] moves = board.GetLegalMoves(depth < 1);
        if (moves.Length == 0)
        {
            return new(new[] { Move.NullMove }, getPieceValues(board, currentPlayer));
        }
        Move bMove = moves[0];
        float bMoveMat = float.MinValue; // how good the best move is for the current player
        Tuple<Move[], float> bR = new(new[]{ bMove }, bMoveMat);
        foreach (var move in moves)
        {
            // code block to be executed
            
            board.MakeMove(move);
            
            Tuple<Move[], float> r = (depth > 0 ? miniMax(board, depth - 1, currentPlayer * -1, (currentPlayer == 1 ? bMoveMat : -1000000), (currentPlayer == -1 && bMoveMat != float.MinValue ? bMoveMat : 1000000)) : new(new[] { move }, getPieceValues(board, currentPlayer)));
            //Console.WriteLine(v);
            float v = r.Item2;

            if (v * currentPlayer > bMoveMat)
            {
                if (draw_moves.Count > 50)
                {
                    /*#DEBUG*/Console.WriteLine("flushing draw move bufffer");
                    draw_moves.Clear();
                }
                if (!draw_moves.Contains(move))
                {
                    if (board.IsDraw() != true)
                    {
                        bR = r;
                        bMove = move;
                        bMoveMat = v * currentPlayer;

                        if(bMoveMat * currentPlayer > max || bMoveMat * currentPlayer < min)
                        {
                            
                            board.UndoMove(move); 
                            break;
                        }

                        
                    }
                    else printErrorDraw(move);
                } 
                else if(board.IsDraw())
                {
                    Console.WriteLine("found doublicate draw move");
                }
            }

            board.UndoMove(move);

            //if(depth == 1)
            //{
                //Console.WriteLine("best move " + move + " with a v of " + v);
            //}
           
        }
        return new(bR.Item1.Append(bMove).ToArray(), bR.Item2);
    }
    
    void printErrorDraw(Move move)
    {
        draw_moves.Add(move);
        /*#DEBUG*/Console.WriteLine("draw detected" + move);
    }

    private float getPieceValues(Board board, int currentPlayer)
    {
  
        if (board.IsInCheckmate())
        {
            /*#DEBUG*/Console.WriteLine("found checkmate");
            return 100000000000 * currentPlayer; // very height number (chose not to use float.MaxValue beacuse it uses more tokens (3 instead of 1)) 
        }
        totalPieceValue = 0;

        //var skipped = board.TrySkipTurn();  // LOOK HERE: this needs to be here so we can if pieces will be atacked in the next round
       

        //if (board.IsDraw()) // seems to be slow
        //{
        //    totalPieceValue -= 100 * currentPlayer; // try to avoid a draw
        //}

        //foreach (Piece p in board.GetAllPieceLists().SelectMany(x => x))
        //{


        //    var s = p.Square;
        //    totalPieceValue += getPieceValue(p.PieceType, s.File, p.IsWhite ? s.Rank : 7 - s.Rank)
        //        * (p.IsWhite ? 1 : -1);

        //    //Console.WriteLine(getPieceValue(p.PieceType, s.Rank, p.IsWhite ? s.File : 7 - s.File)
        //    //    * (p.IsWhite == weAreWhite ? (board.SquareIsAttackedByOpponent(s) ? 0.1f : 1) : -0.9F));
        //}
        for (int x = 0; x <= 7; x++)
        {
            for (int y = 0; y <= 7; y++)
            {
                var s = new Square(x, y);
                var p = board.GetPiece(s); // quite slow
                if (p.IsNull)
                {
                    continue;
                }
                totalPieceValue += getPieceValue(p.PieceType, x, p.IsWhite ? y : 7 - y)
                * (p.IsWhite ? 1 : -1);// * (board.SquareIsAttackedByOpponent(s) ? 0 : 1);

            }
        }

        //totalPieceValue += board.GetAllPieceLists().SelectMany(x => x).Sum(p =>
        //{
        //    var s = p.Square;
        //    return getPieceValue(p.PieceType, s.Rank, p.IsWhite ? s.File : 7 - s.File) * (p.IsWhite ? 1 : -1);
        //});

        //foreach (PieceList plist in board.GetAllPieceLists())
        //{
        //    foreach (Piece p in plist)
        //    {
        //        var s = p.Square;
        //        totalPieceValue += getPieceValue(p.PieceType, s.File, p.IsWhite ? s.Rank : 7 - s.Rank)
        //            * (p.IsWhite ? 1 : -1);
        //    }
        //}

        //        totalPieceValue += getPieceValue(p.PieceType, x, p.IsWhite ? y : 7 - y)
        //            * (p.IsWhite == weAreWhite ? (board.SquareIsAttackedByOpponent(s) ? 0.1f : 1) : -0.9F);
        //        //Console.WriteLine(getPieceValue(p.PieceType, p.IsWhite ? x : 7 - x, p.IsWhite ? y : 7 - y)
        //        //* (p.IsWhite == weAreWhite ? (board.SquareIsAttackedByOpponent(s) ? 0.1f : 1) : -0.9F));

        //    }
        //}
        //if (skipped)
        //{
        //    board.UndoSkipTurn();
        //}

        //Console.WriteLine("total piecevalue is:" + totalPieceValue);

        return totalPieceValue;
    }
    private float getPieceValue(PieceType pieceType, int x, int y)
    {
        int pieceTypeIndex = (int)pieceType - 1;
        //Console.WriteLine(((x > 3 ? 7 - x : x /* this mirrors the table*/) + y * 4 + pieceTypeIndex * 32) * 2);
      
            // pieceValues[pieceTypeIndex] + (int.Parse(pieceSqareValues.Substring(((x > 3 ? 7 - x : x /* this mirrors the table*/) + y * 4 + 256) * 2, 1)) * 5 - 50);

        return pieceValues[pieceTypeIndex] + (int.Parse(pieceSqareValues.Substring(((x > 3 ? 7 - x : x /* this mirrors the table*/) + y * 4 + pieceTypeIndex * 32) * 2, 1)) * 5 - 50);

    }

    string toPieceArray(long[] arr)
    {
        return string.Join("", Array.ConvertAll(arr, element => element.ToString("D16")));
    }

    int evaluate(Board board)
    {
        return 0;
    }

    public bool isPieceProtectedAfterMove(Board board, Move move) {
        return !board.SquareIsAttackedByOpponent(move.TargetSquare);
    }
}
