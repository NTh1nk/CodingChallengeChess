using ChessChallenge.API;
using System;
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
    int[] pieceValues = {100, 300, 300, 500, 900, 2000 };
    int[] arrCenterDistanceInt;
    public Move Think(Board board, Timer timer)
    {
        
        pieceSqareValues = toPieceArray(new[] { 1010101018181818, 1212141611111215, 1010101411090810, 1112120610101010, 0002040402061010, 0410121304111314, 0410131404111213, 0206101100020404, 0608080808101010, 0810111208111112, 0810121208121212, 0811101006080808, 1010101011121212, 0910101009101010, 0910101009101010, 0910101010101011, 0608080908101010, 0810111109101111, 1010111108111111, 0810111006080809, 0402020004020200, 0402020004020200, 0604040208060606, 1414101014161210 }); // use https://onlinestringtools.com/split-string to split into 16 long parts
        //arrCenterDistanceInt = toPieceArray(arrCenterDistance);
        //Console.WriteLine(pieceSqareValues.Length);
        //Console.WriteLine(getPieceValue(PieceType.King, 7, 7));


        weAreWhite = board.IsWhiteToMove;
        Console.WriteLine(" ------ calculate new move -----" + board.IsWhiteToMove);
        return miniMax(board, 3, weAreWhite ? 1 : -1).Item1;
        //Console.WriteLine(isPieceProtectedAfterMove(board, moves[0]));

    }

    private Tuple<Move, float> miniMax(Board board, int depth, int currentPlayer)
    {
        Move[] moves = board.GetLegalMoves();
        if(moves.Length == 0)
        {
            return new(Move.NullMove, -1000 * currentPlayer);
        }
        Move bMove = moves[0];
        float bMoveMat = float.MinValue;
        foreach (var move in moves)
        {
            // code block to be executed
            board.MakeMove(move);
            float v = (depth > 0 ? miniMax(board, depth - 1, currentPlayer * -1).Item2 : getPieceValues(board)) * currentPlayer;
            //Console.WriteLine(v);
            

            if (v > bMoveMat)
            {
                bMove = move;
                bMoveMat = v;
            }
            board.UndoMove(move);

            if(depth == 1)
            {
                //Console.WriteLine("best move " + move + " with a v of " + v);
            }
           
        }
        return new(bMove, bMoveMat * currentPlayer);
    }

    private float getPieceValues(Board board)
    {

        if(board.IsInCheckmate())
        {
            return 1000000000000000000; // very height number (chose not to use float.MaxValue beacuse it uses more tokens (3 instead of 1)) 
        }
        totalPieceValue = 0;

        //var skipped = board.TrySkipTurn();  // LOOK HERE: this needs to be here so we can if pieces will be atacked in the next round
       

        if (board.IsDraw())
        {
            totalPieceValue -= 100; // try to avoid a draw
        }

        foreach (Piece p in board.GetAllPieceLists().SelectMany(x => x))
        {
            

            var s = p.Square;
            totalPieceValue += getPieceValue(p.PieceType, s.File, p.IsWhite ? s.Rank : 7 - s.Rank)
                * (p.IsWhite ? 1 : -1);

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
        return pieceValues[pieceTypeIndex] + (pieceTypeIndex == 0 ? y * 10 : int.Parse(pieceSqareValues.Substring(((x > 3 ? 7 - x : x /* this mirrors the table*/) + y * 4 + pieceTypeIndex * 32) * 2, 1)) * 5 - 50);

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
