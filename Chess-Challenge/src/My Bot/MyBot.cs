using ChessChallenge.API;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
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
        Console.WriteLine(" ------ calculate new move -----", timer);
        Move[] moves = board.GetLegalMoves();
        float bMoveMat = float.MinValue;
        foreach (var move in moves)
        {
            // code block to be executed
            board.MakeMove(move);
            var skipped = board.TrySkipTurn();  // LOOK HERE: this needs to be here so we can if pieces will be atacked in the next round

            float idk = getPieceValues(board);
            if (idk > bMoveMat)
            {
                bMove = move; 
                bMoveMat = idk;
                Console.WriteLine("this move was better so is changing to " + move);
            }
            if (skipped)
            {
                board.UndoSkipTurn();
            }
            board.UndoMove(move);

        }
        if (bMove != null)
        {
            return bMove;
        }
        Console.WriteLine(isPieceProtectedAfterMove(board, moves[0]));
        return moves[0];
    }

    private float getPieceValues(Board board)
    {
        totalPieceValue = 0;

        if(board.IsInCheckmate())
        {
            return 1000000000000000000;
        }
        //Console.WriteLine(board.GetAllPieceLists().SelectMany(x => x).ToList().Count);

        foreach (Piece p in board.GetAllPieceLists().SelectMany(x => x))
        {
            var s = p.Square;
            totalPieceValue += getPieceValue(p.PieceType, s.File, p.IsWhite ? s.Rank : 7 - s.Rank)
                * (p.IsWhite == weAreWhite ? (board.SquareIsAttackedByOpponent(s) ? 0.1f : 1) : -0.9F);
            //Console.WriteLine(getPieceValue(p.PieceType, s.Rank, p.IsWhite ? s.File : 7 - s.File)
            //    * (p.IsWhite == weAreWhite ? (board.SquareIsAttackedByOpponent(s) ? 0.1f : 1) : -0.9F));
        }
        //for (int x = 0; x <= 7; x++)
        //{
        //    for (int y = 0; y <= 7; y++)
        //    {
        //        var s = new Square(x, y);
        //        var p = board.GetPiece(s);
        //        totalPieceValue += getPieceValue(p.PieceType, x, p.IsWhite ? y : 7 - y)
        //            * (p.IsWhite == weAreWhite ? (board.SquareIsAttackedByOpponent(s) ? 0.1f : 1) : -0.9F);
        //        //Console.WriteLine(getPieceValue(p.PieceType, p.IsWhite ? x : 7 - x, p.IsWhite ? y : 7 - y)
        //        //* (p.IsWhite == weAreWhite ? (board.SquareIsAttackedByOpponent(s) ? 0.1f : 1) : -0.9F));

        //    }
        //}
        Console.WriteLine("total piecevalue is:" + totalPieceValue);
        
        return totalPieceValue;
    }
    private float getPieceValue(PieceType pieceType, int x, int y)
    {
        switch((int)pieceType)
        {
            case 1:  //PieceType.Pawn:
                return 100 + y * 10;
            case 2:  //PieceType.Knight:
                return 300;
            case 3:  //PieceType.Bishop:
                return 300;
            case 4:  //PieceType.Rook:
                return 500;
            case 5:  //PieceType.Queen:
                return 900;
        }
        
        return 0; 
        
    }

    int evaluate(Board board)
    {
        return 0;
    }

    public bool isPieceProtectedAfterMove(Board board, Move move) {
        return !board.SquareIsAttackedByOpponent(move.TargetSquare);
    }
}
