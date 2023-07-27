using ChessChallenge.API;

public class MyBot : IChessBot
{
    //example code
    //public Move Think(Board board, Timer timer)
    //{
    //    Move[] moves = board.GetLegalMoves();
    //    return moves[0];
    //}

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
