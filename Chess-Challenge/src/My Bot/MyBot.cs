using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    // right now funktions are seperated. before submision, everything will be compacted into the think function.

    //example code
    //public Move Think(Board board, Timer timer)
    //{
    //    Move[] moves = board.GetLegalMoves();
    //    return moves[0];
    //}

    int[] pieceValue = new int[5] {100,300,300,500,900};

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        Console.WriteLine(isPieceProtectedAfterMove(board, moves[0]));
        return moves[0];
    }
    private float getPieceValue(Board board, int legalMoveCounter)
    {
        return 0.0f;
    }

    int evaluate(Board board)
    {
        return 0;
    }

    public bool isPieceProtectedAfterMove(Board board, Move move) {
        return board.SquareIsAttackedByOpponent(move.TargetSquare);
    }
}
