﻿using ChessChallenge.API;
using System;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;

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
        Move bMove;
    public Move Think(Board board, Timer timer)
    {
        weAreWhite = board.IsWhiteToMove;
        Move[] moves = board.GetLegalMoves();
        float bMoveMat = 0f;
        foreach (var move in moves)
        {
             // code block to be executed
             board.MakeMove(move);
            float idk = getPieceValues(board);
            if (idk > bMoveMat)
            {
                bMove = move; 
            }
            board.UndoMove(move);

        }
        Console.WriteLine("total piecevalue is:"+getPieceValues(board));
        if (bMove != null)
        {
            return bMove;
        }
        return moves[0];
    }

    private float getPieceValues(Board board)
    {
        totalPieceValue = 0;
        for (int x = 0; x <= 7; x++)
        {
            for (int y = 0; y <= 7; y++)
            {
                var p = board.GetPiece(new Square(x, y));
                if (p.IsWhite == weAreWhite) // checks if we own the piece
                {
                    totalPieceValue += getPieceValue(p.PieceType, x, y);
                }
                else
                {
                    totalPieceValue -= getPieceValue(p.PieceType, 7 - x, 7 - y) * 0.9F;
                }
            }
        }
        return totalPieceValue;
    }
    private float getPieceValue(PieceType pieceType,int x, int y)
    {
        switch(pieceType)
        {
            case PieceType.Pawn:
                return 100 + y * 10;
            case PieceType.Knight:
                return 300;
            case PieceType.Bishop:
                return 300;
            case PieceType.Rook:
                return 500;
            case PieceType.Queen:
                return 900;
        }
        return 0; 
        
    }

    int evaluate(Board board)
    {

        return 0;
    }
}