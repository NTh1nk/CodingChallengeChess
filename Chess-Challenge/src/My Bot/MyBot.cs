using ChessChallenge.API;
using System;
using static System.Math;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

public class MyBot : IChessBot
{
    // right now funktions are seperated. before submision, everything will be compacted into the think function if possible.

    //---this section is variables designated to zobrist hashing and the transportition table---
    byte[] currentBoardHash = new byte[8];
    Dictionary<ulong,float> boardHashes = new();

    //right now this funktion is not needed as it seems board has a funktion to get the zobrist key but it might need to be reintruduced if the api funktion is to slow
    //ulong hashBoard(Board board)
    //{
    //    PieceList[] PL = board.GetAllPieceLists();
    //    Piece[] PA = new Piece[28];
    //    int PAI = 0;
    //    foreach (PieceList PL2 in PL)
    //    {
    //        for(int i=0;i<PL2.Count;i++)
    //        {
    //            PA[PAI] = PL2.GetPiece(i);
    //            PAI++;
    //        }
    //    }
    //    return 0;
    //}

    //---end---

    bool weAreWhite;
    string pieceSqareValues;
    // how much each piece is worth
    int[] pieceValues = {
        100, // Pawn
        300, // Knight
        320, // Bishop
        500, // Rook
        900, // Queen
        2000 }; // King

    int[] arrCenterDistanceInt;
    List<Move> draw_moves = new();

    public bool IsEndgameNoFunction = false;

    //using a variable instead of float.minvalue for BBC saving
    float minFloatValue = float.MinValue;


    // debug variables (variables only used for debuging)
    int searchedMoves = 0; //#DEBUG
    int foundCheckMates = 0; //#DEBUG
    int foundDublicateDrawMoves = 0; //#DEBUG
    string foundDrawMoves; //#DEBUG
    int addedZobristKeys = 0; //#DEBUG
    int usedZobristKeys = 0; //#DEBUG
    // -----------------------------
    Queue<int> foundDrawMovesPerTurn = new();
    int maxSearchDepth = 5;

    public bool IsEndgame(Board board, bool white) //#DEBUG
    { //#DEBUG


        float totalPieceValue = 0;
        for (int x = 0; x <= 7; x++)
        {
            for (int y = 0; y <= 7; y++)
            {
                var s = new Square(x, y);
                var p = board.GetPiece(s); // quite slow
                if (p.IsNull || white != p.IsWhite) continue;

                totalPieceValue += pieceValues[(int)p.PieceType - 1];
                
            }
        }
        if (totalPieceValue < 2900)
            {

                pieceValues = new [] {
                160, // Pawn
                320, // Knight
                345, // Bishop
                530, // Rook
                940, // Queen
                2000 // King
                
            };
            return true;
            }

        
        return false;
    } //#DEBUG
    public Move Think(Board board, Timer timer)
    {

        pieceSqareValues = toPieceArray(new[] { 1010101018181818, 1212141611111215, 1010101411090810, 1112120610101010, 0002040402061010, 0410121304111314, 0410131404111213, 0206101100020404, 0608080808101010, 0810111208111112, 0810121208121212, 0811101006080808, 1010101011121212, 0910101009101010, 0910101009101010, 0910101010101011, 0608080908101010, 0810111109101111, 1010111108111111, 0810111006080809, 0402020004020200, 0402020004020200, 0604040208060606, 1414060630341207,
                                                1010101036303230, 2015181412121413, 1212121211111111, 0909090910101010, 0002040402061010, 0410121304111314, 0410131404111213, 0206101100020404, 0608080808101010, 0810111208111112, 0810121208121212, 0811101006080808, 1010101011121212, 0910101009101010, 0910101009101010, 0910101010101011, 0608080908101010, 0810111109101111, 1010111108111111, 0810111006080809, 0402020004020200, 0402020004020200, 0604040208060606, 1414060630341207 }); // use https://onlinestringtools.com/split-string to split into 16 long parts
        //Botton is endgame
        //arrCenterDistanceInt = toPieceArray(arrCenterDistance);                                                                                                                                                                                                                                                                                                                                                                                                                                       
        //Console.WriteLine(pieceSqareValues.Length);
        Console.WriteLine("cewefefrw " + (getPieceValues(board, -1) + evaluateTop(board, -1)));
        //Console.WriteLine(getPieceValue(PieceType.King, 7, 7));
        //IsEndgameNoFunction = true;
        //Console.WriteLine(getPieceValue(PieceType.Pawn, 0, 7 - 6));               
        weAreWhite = board.IsWhiteToMove;
        Console.WriteLine("---calculate new move---" + board.IsWhiteToMove); //#DEBUG
        var bestMove = miniMax(board, timer.MillisecondsRemaining < 20000 ? timer.MillisecondsRemaining < 5000 ? 2 : 3 : maxSearchDepth, weAreWhite ? 1 : -1, minFloatValue, float.MaxValue, getPieceValues(board, weAreWhite ? 1 : -1)).Item1;
        bestMove.ToList().ForEach(move => { Console.WriteLine("predicted move: " + move); });
        if (IsEndgame(board, !weAreWhite)){
            IsEndgameNoFunction = true;
            Console.WriteLine("We are in the endgame"); //#DEBUG
        }
        
        if (boardHashes.Count > 9500)
        { //#DEBUG
            Console.WriteLine("flushing bordhashes buffer"); //#DEBUG
            boardHashes.Clear();
        } //#DEBUG
        if (draw_moves.Count > 125)
        { //#DEBUG
            Console.WriteLine("flushing draw move bufffer"); //#DEBUG
            draw_moves.Clear();
        } //#DEBUG
        
        Console.WriteLine("found checkmate: "+foundCheckMates+" times this turn"); //#DEBUG
        foundCheckMates = 0; //#DEBUG
        
        Console.WriteLine("found: "+foundDublicateDrawMoves+" dublicate draw moves this turn"); //#DEBUG
        foundDublicateDrawMoves = 0; //#DEBUG
        
        Console.WriteLine("found these draw moves: "+foundDrawMoves+" this turn"); //#DEBUG
        foundDrawMoves = ""; //#DEBUG
        
        Console.WriteLine(searchedMoves + " Searched moves"); //#DEBUG
        
        Console.WriteLine("adding: "+addedZobristKeys+" deep seached zobrist keys this turn"); //#DEBUG
        addedZobristKeys = 0;

        Console.WriteLine("found: " + usedZobristKeys + " positions already calculated this turn"); //#DEBUG
        usedZobristKeys = 0;

        Console.WriteLine("dececion took: "+timer.MillisecondsElapsedThisTurn+" ms this turn"); //#DEBUG

        return bestMove[bestMove.Length - 1];
        //Console.WriteLine(isPieceProtectedAfterMove(board, moves[0]));
        
    }

    private Tuple<Move[], float> miniMax(Board board, int depth, int currentPlayer, float min, float max, float prevBase)
    {
        
        Move[] moves = board.GetLegalMoves(depth < 1);
        
        if (moves.Length == 0)
        {
            return new(new[] { Move.NullMove }, prevBase); //if possible removing the getpieceValue would be preferable, but for now it's better with it kept there
        }
        Move bMove = moves[0];
        float bMoveMat = minFloatValue * currentPlayer;
        Tuple<Move[], float> bR = new(new[] { bMove }, bMoveMat);

        List<(Move move, float Base)> sortedMoves = moves.Select(m => (m, evaluateBase(prevBase, m, currentPlayer, board))).ToList();
        sortedMoves = sortedMoves.OrderByDescending(item => item.Base).ToList();

        foreach (var (move, Base) in sortedMoves)
        {
            
            
            board.MakeMove(move);
            float newBase = prevBase + Base * currentPlayer;

            //if (boardHashes.ContainsKey(board.ZobristKey)) usedZobristKeys++; //#DEBUG


            Tuple<Move[], float> r = 
                (depth > 0 ? 
                    miniMax(board, depth - 1, currentPlayer * -1, currentPlayer == 1 ? bMoveMat : minFloatValue, currentPlayer == -1 ? bMoveMat : float.MaxValue, newBase)  : // use minimax if the depth is bigger than 0
                    new(new[] { move }, /*boardHashes.ContainsKey(board.ZobristKey) ? boardHashes[board.ZobristKey] : */newBase + evaluateTop(board, currentPlayer))); // use the stored value or get piece values new
            //Console.WriteLine(v);
            float v = r.Item2;

            //if (!boardHashes.ContainsKey(board.ZobristKey))
            //{
            //    addedZobristKeys++;
            //    boardHashes.Add(board.ZobristKey, v); // makes ram usage hight but speeds up a little bit
            //}
            if (currentPlayer == 1 ? v > bMoveMat : v < bMoveMat)
            {
                if (!draw_moves.Contains(move))
                {
                    if (!board.IsDraw())
                    {
                        bR = r;
                        bMove = move;
                        bMoveMat = v;

                        if(v >= max || v <= min)
                        {
                            board.UndoMove(move);
                            break;
                        }
                    }
                    else printErrorDraw(move); //#DEBUG
                } 
                else if(board.IsDraw()) //#DEBUG
                { //#DEBUG
                    foundDublicateDrawMoves++; //#DEBUG
                } //#DEBUG
            }
            if(depth == maxSearchDepth) //#DEBUG
            {//#DEBUG
                //Console.WriteLine($"{move}: {v}");//#DEBUG
                Console.WriteLine($"{v}");//#DEBUG
            }//#DEBUG

            board.UndoMove(move);

            //if(depth == 1)
            //{
            //Console.WriteLine("best move " + move + " with a v of " + v);
            //}

        }



        return new(bR.Item1.Append(bMove).ToArray(), bR.Item2);
    }
    
    void printErrorDraw(Move move) //#DEBUG
    {  //#DEBUG
        draw_moves.Add(move);
        foundDrawMoves += "\""+move+"\" "; //#DEBUG
    } //#DEBUG

   /* private int ManhattanDistance(Square square1, Square square2)
    {
    int dx = Math.Abs(square1.File - square2.File);
    int dy = Math.Abs(square1.Rank - square2.Rank);
    return dx + dy;
    } */
    private float getPieceValues(Board board, int currentPlayer)
    {
        //var skipped = board.TrySkipTurn();  // LOOK HERE: this needs to be here so we can if pieces will be atacked in the next round


        //if (board.IsDraw()) // seems to be slow
        //{
        //    totalPieceValue -= 100 * currentPlayer; // try to avoid a draw
        //}

        //foreach (Piece p in board.GetAllPieceLists().SelectMany(x => x)) // 49.7  left (3 seconds faster than looping over them all) (depth 6)
        //{


        //    var s = p.Square;
        //    totalPieceValue += getPieceValue(p.PieceType, s.File, p.IsWhite ? s.Rank : 7 - s.Rank)
        //        * (p.IsWhite ? 1 : -1);

        //}

        return board.GetAllPieceLists().SelectMany(x => x).Sum(p =>
        {
            var s = p.Square;
            return getPieceValue(p.PieceType, s, p.IsWhite) * (p.IsWhite ? 1 : -1);
        });

        //foreach (PieceList plist in board.GetAllPieceLists()) // seems to be about 100 ms faster than using .SelectMany()
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
    }

    //the DEBUGS are in place even tho it's called twice becaus in the end it shouldt be called more than once
    private float getPieceValue(PieceType pieceType, Square s, bool IsWhite) //#DEBUG
    { //#DEBUG
        
        float endGameBonus = 0;
        int pieceTypeIndex = (int)pieceType - 1;

        if (pieceTypeIndex < 0) return 0;
        //Console.WriteLine(((x > 3 ? 7 - x : x /* this mirrors the table*/) + y * 4 + pieceTypeIndex * 32) * 2);
        //if(IsEndgameNoFunction && pieceTypeIndex == 6)
        //{

        //    //Square enemyKingSquare = board.GetKingSquare(!weAreWhite);
        //    int distanceToNearestCorner = Math.Min(x, 7 - x) + Math.Min(y, 7 - y);

            

        //    endGameBonus = 10000 * (distanceToNearestCorner);
        //     //int distanceToEnemyKing = ManhattanDistance(board.GetKingSquare(weAreWhite), board.GetKingSquare(!weAreWhite));
        //     //int distanceBonus = 10 * (7 - distanceToEnemyKing); // Adjust the bonus factor as needed

        //}    
        return pieceValues[pieceTypeIndex] + (int.Parse(pieceSqareValues.Substring(((s.File > 3 ? 7 - s.File : s.File /* this mirrors the table*/) + (IsWhite ? 7 - s.Rank : s.Rank) * 4 + pieceTypeIndex * 32) * 2 + (IsEndgameNoFunction ? 384 : 0), 2)) * 5 - 50) + endGameBonus;
    } //#DEBUG



    string toPieceArray(long[] arr) => string.Join("", Array.ConvertAll(arr, element => element.ToString("D16")));

    float evaluateBase(float prevBase, Move move, int currentPlayer, Board board)
    {
        bool isWhite = currentPlayer > 0; // doesn't matter if it a variable or called each time BBS-wise
        //if(move.IsEnPassant || move.IsCastles)
        //{
        //    return (getPieceValues(board, currentPlayer) - prevBase) * currentPlayer;
        //}
        return
            -getPieceValue(move.MovePieceType, move.StartSquare, isWhite)  // remove the old piece 
            +getPieceValue(move.IsPromotion ? move.PromotionPieceType : move.MovePieceType, move.TargetSquare, isWhite) // add the new piece (move piece type if it is't promotion. if it is use the promotion piece type)
            +getPieceValue(move.CapturePieceType, move.TargetSquare, !isWhite) // remove the captured piece (plus beacuse we capture the oponements piece wich is good for the current player)
            ;

    }

    float evaluateTop(Board board, int currentPlayer)
    {
        //bool isWhite = currentPlayer > 0; // doesn't matter if it a variable or called each time BBS-wise

        if (board.IsInCheckmate())
        { //#DEBUG
            foundCheckMates++; //#DEBUG
            return 1000000000000000 * -currentPlayer; // very height number (chose not to use float.MaxValue beacuse it uses more tokens (3 instead of 1)) 
        } //#DEBUG
        return (board.HasKingsideCastleRight(true) ? 22 : 0)
             + (board.HasKingsideCastleRight(false) ? -22 : 0)
             + (board.HasQueensideCastleRight(true) ? 10 : 0)
             + (board.HasQueensideCastleRight(false) ? -10 : 0);



    }

    



    //left in the code for now even tho it's unused might be used in the future
    public bool isPieceProtectedAfterMove(Board board, Move move) => !board.SquareIsAttackedByOpponent(move.TargetSquare); //#DEBUG
 

    ulong prevSeed = 0;
    //ulong smallRandomNumberGenerator(ulong seed = 0, int maxSizeRange = 100)
    //{
    //    if (seed == 0) seed = prevSeed;
    //    prevSeed = (ulong)Abs(Cos(seed * 10) * maxSizeRange);
    //    return prevSeed;
    //}
}
