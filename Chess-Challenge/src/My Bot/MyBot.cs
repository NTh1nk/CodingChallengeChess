using ChessChallenge.API;
using System;
using static System.Math;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    // right now funktions are seperated. before submision, everything will be compacted into the think function if possible.

    //---this section is variables designated to zobrist hashing and the transportition table---
    int randomBoardHashCounter = 0;
    Dictionary<ulong,Tuple<float,int,float>> boardHashes = new(); //dict <zobrist key, tuple<total_board_value, depth_iteration, maxBranchValue>>

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
    int[] pieceSqareValues;
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
    //Queue<int> foundDrawMovesPerTurn = new();
    int maxSearchDepth = 7;

    public bool IsEndgame(Board board, bool white) //#DEBUG
    { //#DEBUG


        if (board.GetAllPieceLists().SelectMany(x => x).Sum(p =>
            p.IsWhite != white ? pieceValues[(int)p.PieceType - 1] : 0) < 2900)
        {
            pieceValues = new[] {
                160, // Pawn
                320, // Knight
                345, // Bishop
                530, // Rook
                940, // Queen
                2000 // King
                };
            return true;
        };
        return false;
        //float totalPieceValue = 0;
        //for (int x = 0; x <= 7; x++)
        //{
        //    for (int y = 0; y <= 7; y++)
        //    {
        //        var s = new Square(x, y);
        //        var p = board.GetPiece(s); // quite slow
        //        if (p.IsNull || white != p.IsWhite) continue;

        //        totalPieceValue += pieceValues[(int)p.PieceType - 1];

        //    }
        //}
        //if (totalPieceValue < 2900)
        //{

        //    pieceValues = new[] {
        //        160, // Pawn
        //        320, // Knight
        //        345, // Bishop
        //        530, // Rook
        //        940, // Queen
        //        2000 // King

        //    };
        //    return true;
        //}
        //return false;
    } //#DEBUG
    public Move Think(Board board, Timer timer)
    {

        pieceSqareValues = toPieceArray(new[] { 1010101018181818, 1212141611111215, 1010101411090810, 1112120610101010, 0002040402061010, 0410121304111314, 0410131404111213, 0206101100020404, 0608080808101010, 0810111208111112, 0810121208121212, 0811101006080808, 1010101011121212, 0910101009101010, 0910101009101010, 0910101010101011, 0608080908101010, 0810111109101111, 1010111108111111, 0810111006080809, 0402020004020200, 0402020004020200, 0604040208060606, 1414060630341207,
                                                1010101036303230, 2015181412121413, 1212121211111111, 0909090910101010, 0002040402061010, 0410121304111314, 0410131404111213, 0206101100020404, 0608080808101010, 0810111208111112, 0810121208121212, 0811101006080808, 1010101011121212, 0910101009101010, 0910101009101010, 0910101010101011, 0608080908101010, 0810111109101111, 1010111108111111, 0810111006080809, 0402020004020200, 0402020004020200, 0604040208060606, 1414060630341207 }); // use https://onlinestringtools.com/split-string to split into 16 long parts
        //Botton is endgame
        //arrCenterDistanceInt = toPieceArray(arrCenterDistance);                                                                                                                                                                                                                                                                                                                                                                                                                                       
        //Console.WriteLine(pieceSqareValues.Length);
        //Console.WriteLine(getPieceValue(PieceType.King, 7, 7));
        //IsEndgameNoFunction = true;
        //Console.WriteLine(getPieceValue(PieceType.Pawn, 0, 7 - 6));
        weAreWhite = board.IsWhiteToMove;
        Console.WriteLine("---calculate new move---" + board.IsWhiteToMove); //#DEBUG
        var bestMove = miniMax(board, timer.MillisecondsRemaining < 20000 ? timer.MillisecondsRemaining < 5000 ? 2 : 3 : maxSearchDepth, weAreWhite ? 1 : -1, minFloatValue, float.MaxValue, getPieceValues(board, weAreWhite ? 1 : -1)).Item1;
        bestMove.ToList().ForEach(move => { Console.WriteLine("predicted move: " + move); });
        if (IsEndgame(board, !weAreWhite))
        {
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

        Console.WriteLine("found checkmate: " + foundCheckMates + " times this turn"); //#DEBUG
        foundCheckMates = 0; //#DEBUG

        Console.WriteLine("found: " + foundDublicateDrawMoves + " dublicate draw moves this turn"); //#DEBUG
        foundDublicateDrawMoves = 0; //#DEBUG

        Console.WriteLine("found these draw moves: " + foundDrawMoves + " this turn"); //#DEBUG
        foundDrawMoves = ""; //#DEBUG

        Console.WriteLine(searchedMoves + " Searched moves"); //#DEBUG
        
        Console.WriteLine("adding: "+addedZobristKeys+" deep seached zobrist keys this turn"); //#DEBUG
        addedZobristKeys = 0; //#DEBUG

        Console.WriteLine("found: " + usedZobristKeys + " positions already calculated this turn"); //#DEBUG
        usedZobristKeys = 0; //#DEBUG

        Console.WriteLine("dececion took: "+timer.MillisecondsElapsedThisTurn+" ms this turn"); //#DEBUG
        
        randomBoardHashCounter=+maxSearchDepth;
        foreach (ulong i in boardHashes.Keys) if (boardHashes[i].Item2 < randomBoardHashCounter - maxSearchDepth) boardHashes.Remove(i); 

        return bestMove[bestMove.Length - 1];
        //Console.WriteLine(isPieceProtectedAfterMove(board, moves[0]));

    }

    private Tuple<Move[], float> miniMax(Board board, int depth, int currentPlayer, float min, float max, float prevBase)
    {

        Move[] moves = board.GetLegalMoves(depth < 1);

        if (moves.Length == 0)
        {
            return new(new[] { Move.NullMove }, prevBase + evaluateTop(board, currentPlayer)); //if possible removing the getpieceValue would be preferable, but for now it's better with it kept there
        }
        Move bMove = moves[0];
        float bMoveMat = minFloatValue * currentPlayer; // how good the best move is for the current player
        Tuple<Move[], float> bR = new(new[] { bMove }, bMoveMat);

        List<(Move move, float Base)> sortedMoves = moves.Select(m => (m, evaluateBase(prevBase, m, currentPlayer, board) )).ToList();
        sortedMoves = sortedMoves.OrderByDescending(item => item.Base - (item.move.IsCapture ? pieceValues[(int)item.move.MovePieceType - 1] / 3 : 0)).ToList(); // if it's a capture it subtracks the attackers value thereby creating MVV-LVA (Most Valuable Victim - Least Valuable Aggressor)
        
        foreach (var (move, Base) in sortedMoves)
        {

            board.MakeMove(move);

            ulong zobristKey = board.ZobristKey;

            bool t = boardHashes.TryGetValue(zobristKey, out var StoredTable);
            if(t) usedZobristKeys++; //#DEBUG
            
            float newBase = move.IsEnPassant || move.IsCastles ? getPieceValues(board, currentPlayer) : (prevBase + Base * currentPlayer); // if it is enPassent we recalculate the move

            float total = t ? StoredTable.Item1 : newBase + evaluateTop(board, currentPlayer);

            bool isDraw = board.IsRepeatedPosition() || board.IsFiftyMoveDraw();

            Tuple<Move[], float> r = 
                (
                depth > 0 ? 
                miniMax(board, depth - 1, -currentPlayer, min, max, newBase) : // use minimax if the depth is bigger than 0
                new(new[] { move }, total) // use the stored value or get piece values new
                );
            
            if (/*!boardHashes.ContainsKey(zobristKey) &&*/ depth < maxSearchDepth)
            { //#DEBUG
                bool AB = boardHashes.TryAdd(zobristKey, new Tuple<float, int, float>(total, randomBoardHashCounter+(maxSearchDepth-depth),0.0f)); ///using tryadd instead of checking if it exist and using add as it seems to be 600-800ms faster.
                if (AB) addedZobristKeys++; //#DEBUG
            } //#DEBUG

            //if (boardHashes.ContainsKey(board.ZobristKey)) usedZobristKeys++; //#DEBUG
            //Console.WriteLine(v);
            float v = r.Item2;
            /*if(depth < 1)
            {
                //Console.Write(v + ", ");
            }*/

            if ((currentPlayer == 1 ? v > bMoveMat : v < bMoveMat) && !isDraw) //using isdraw bool instead of board.isdraw as it doesnt check for insufficient material and is considerbly faster
            {
                bR = r;
                bMove = move;
                bMoveMat = v;
            }
            if(depth == maxSearchDepth) //#DEBUG
            {//#DEBUG
             //Console.WriteLine($"{move}: {v}");//#DEBUG
                Console.WriteLine($"{v}");//#DEBUG
            }//#DEBUG

            //if (!boardHashes.ContainsKey(board.ZobristKey))
            //{
            //    addedZobristKeys++;
            //    boardHashes.Add(board.ZobristKey, v); // makes ram usage hight but speeds up a little bit
            //}

            board.UndoMove(move);

            if (currentPlayer >0 ? v >= bMoveMat : v <= bMoveMat)
            {
                //if (!draw_moves.Contains(move))
                //{
                if (!isDraw)
                {
                    bR = r;
                    bMove = move;
                    bMoveMat = v;
                    if (currentPlayer > 0)
                    {
                        min = Max(min, v);
                        if (v > max) break;

                    }
                    else
                    {
                        max = Min(max, v);
                        if (v < min) break;
                    }
                }
                //else printErrorDraw(move); //#DEBUG

                //else if(board.IsDraw()) //#DEBUG
                //{ //#DEBUG
                //foundDublicateDrawMoves++; //#DEBUG
                //} //#DEBUG
            }

            if (depth == maxSearchDepth) //#DEBUG
            {//#DEBUG
                //Console.WriteLine($"{v}");//#DEBUG
            }//#DEBUG




            //if(depth == 1)
            //{
            //Console.WriteLine("best move " + move + " with a v of " + v);
            //}

        }

        if(depth == maxSearchDepth)
        {
            Console.WriteLine("best moves mat was: " + bMoveMat);
        }


        return new(bR.Item1.Append(bMove).ToArray(), bR.Item2);
    }

    void printErrorDraw(Move move) //#DEBUG
    {  //#DEBUG
        draw_moves.Add(move);
        foundDrawMoves += "\"" + move + "\" "; //#DEBUG
    } //#DEBUG

    /* private int ManhattanDistance(Square square1, Square square2)
     {
     int dx = Math.Abs(square1.File - square2.File);
     int dy = Math.Abs(square1.Rank - square2.Rank);
     return dx + dy;
     } */
                private float getPieceValues(Board board, int currentPlayer) =>
        board.GetAllPieceLists().SelectMany(x => x).Sum(p =>
            getPieceValue(p.PieceType, p.Square, p.IsWhite) * (p.IsWhite ? 1 : -1));



    //the DEBUGS are in place even tho it's called twice becaus in the end it shouldt be called more than once
    private float getPieceValue(PieceType pieceType, Square s, bool IsWhite) //#DEBUG
    { //#DEBUG
        //float endGameBonus = 0; //commenting out the endgame bonus for now as it is unused
        int pieceTypeIndex = (int)pieceType - 1;
        if (pieceTypeIndex < 0) return 0;
        //Console.WriteLine(((x > 3 ? 7 - x : x /* this mirrors the table*/) + y * 4 + pieceTypeIndex * 32) * 2);
        //if(IsEndgameNoFunction && pieceTypeIndex == 6)
        //{
        //     //int distanceBonus = 10 * (7 - distanceToEnemyKing); // Adjust the bonus factor as needed
        //}    
        return pieceValues[pieceTypeIndex] + pieceSqareValues[
            (s.File > 3 ? 7 - s.File : s.File ) // this mirrors the table to use less BBS
            + (IsWhite ? 7 - s.Rank : s.Rank) * 4 + pieceTypeIndex * 32 // flip the table if it is white
            + (IsEndgameNoFunction ? 192 : 0)] // use endgame values if we are in the endgame
                * 5 - 50;
    } //#DEBUG

    int[] toPieceArray(long[] arr) => Array.ConvertAll(arr, element => Enumerable.Range(0, 8).Select(i => int.Parse(element.ToString("D16").Substring(i * 2, 2)))).SelectMany(x => x).ToArray();

        


    //left in the code for now even tho it's unused might be used in the future
    public bool isPieceProtectedAfterMove(Board board, Move move) => !board.SquareIsAttackedByOpponent(move.TargetSquare); //#DEBUG

    float evaluateBase(float prevBase, Move move, int currentPlayer, Board board)
    {
        bool isWhite = currentPlayer > 0; // doesn't matter if it a variable or called each time BBS-wise
        if (move.IsEnPassant || move.IsCastles) // beause it is a "special" move we just return 0. this is for some reason better than returning below
            return 0;

        return
            -getPieceValue(move.MovePieceType, move.StartSquare, isWhite)  // remove the old piece 
            + getPieceValue(move.IsPromotion ? move.PromotionPieceType : move.MovePieceType, move.TargetSquare, isWhite) // add the new piece (move piece type if it is't promotion. if it is use the promotion piece type)
            + getPieceValue(move.CapturePieceType, move.TargetSquare, !isWhite) // remove the captured piece (plus beacuse we capture the oponements piece wich is good for the current player)
            ;
    }

    float evaluateTop(Board board, int currentPlayer)
    {

        if (board.IsInCheckmate())
        { //#DEBUG
            foundCheckMates++; //#DEBUG
            return 1000000000000 * -currentPlayer; // very height number (chose not to use float.MaxValue beacuse it uses more tokens (3 instead of 1)) 
        } //#DEBUG

        return 0;
    }

    //ulong prevSeed = 0;
    //ulong smallRandomNumberGenerator(ulong seed = 0, int maxSizeRange = 100)
    //{
    //    if (seed == 0) seed = prevSeed;
    //    prevSeed = (ulong)Abs(Cos(seed * 10) * maxSizeRange);
    //    return prevSeed;
    //}
}
