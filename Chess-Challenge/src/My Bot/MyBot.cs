using ChessChallenge.API;
using System;
using static System.Math;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.XPath;

public class MyBot : IChessBot
{
    // right now funktions are seperated. before submision, everything will be compacted into the think function if possible.
    //---this section is variables designated to zobrist hashing and the transportition table---
    int boardHashCounter = 0;
    Dictionary<ulong,(float boardVal,int depth,Move bestMove, int ply)> boardHashes = new(); //dict <zobrist key, tuple<total_board_value, depth_iteration, bestMove>>

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
    int maxSearchDepth = 8;

    public bool IsEndgame(Board board, bool white) //#DEBUG
    { //#DEBUG


        if (board.GetAllPieceLists().SelectMany(x => x).Sum(p =>
            p.IsWhite != white ? pieceValues[(int)p.PieceType - 1] : 0) < 3000)
        {

            // change values to endgame values, to change strategi
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
    } 
    public Move Think(Board board, Timer timer)
    {

        pieceSqareValues = toPieceArray(new[] { 1010101018181818, 1212141611111215, 1010101411090810, 1112120610101010, 0002040402061010, 0410121304111314, 0410131404111213, 0206101100020404, 0608080808101010, 0810111208111112, 0810121208121212, 0811101006080808, 1010101011121212, 0910101009101010, 0910101009101010, 0910101010101011, 0608080908101010, 0810111109101111, 1010111108111111, 0810111006080809, 0402020004020200, 0402020004020200, 0604040208060606, 1414060630341207,
                                                1010101036303230, 2015181412121413, 1212121211111111, 0909090910101010, 0002040402061010, 0410121304111314, 0410131404111213, 0206101100020404, 0608080808101010, 0810111208111112, 0810121208121212, 0811101006080808, 1010101011121212, 0910101009101010, 0910101009101010, 0910101010101011, 0608080908101010, 0810111109101111, 1010111108111111, 0810111006080809, 0002040604060810, 0408141604081618, 0408161804081416, 0404101000040404 }); // use https://onlinestringtools.com/split-string to split into 16 long parts
        //Botton is endgame
        //arrCenterDistanceInt = toPieceArray(arrCenterDistance);                                                                                                                                                                                                                                                                                                                                                                                                                                       
        //Console.WriteLine(pieceSqareValues.Length);
        //Console.WriteLine(getPieceValue(PieceType.King, 7, 7));
        //IsEndgameNoFunction = true;
        //Console.WriteLine(getPieceValue(PieceType.Pawn, 0, 7 - 6));
        weAreWhite = board.IsWhiteToMove;
        Console.WriteLine("---calculate new move---" + board.IsWhiteToMove); //#DEBUG
        Move bestMove = Move.NullMove;
        for (int depth = 1; depth <= 30; depth++)
        {
            
            bestMove = miniMax(board, depth, weAreWhite ? 1 : -1, minFloatValue, float.MaxValue, getPieceValues(board, weAreWhite ? 1 : -1), 0).Item1;
            Console.WriteLine("searched for depth: " + depth); //#DEBUG
            if (timer.MillisecondsElapsedThisTurn > timer.MillisecondsRemaining / 60)
                break;
        }
        //bestMoves.ToList().ForEach(move => { Console.WriteLine("predicted move: " + move); });
        if (IsEndgame(board, !weAreWhite))
        {
            IsEndgameNoFunction = true;
            Console.WriteLine("We are in the endgame"); //#DEBUG
        }

        //if (boardHashes.Count > 9500)
        //{ //#DEBUG
        //    Console.WriteLine("flushing bordhashes buffer"); //#DEBUG
        //    boardHashes.Clear();
        //} //#DEBUG
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
        Console.WriteLine(boardHashes.Count);

        boardHashCounter = +2;
        foreach (var i in boardHashes)
            if (i.Value.ply < boardHashCounter) // if its root
                boardHashes.Remove(i.Key); // we throw it out
        //boardHashes.Clear();
        Console.WriteLine("dececion took: "+timer.MillisecondsElapsedThisTurn+" ms this turn"); //#DEBUG

        return bestMove;
        //Console.WriteLine(isPieceProtectedAfterMove(board, moves[0]));

    }

    private Tuple<Move, float> miniMax(Board board, int depth, int currentPlayer, float min, float max, float prevBase, int ply)
    {
        bool isMaximizingPlayer = currentPlayer > 0; // could also be called isWhite
        Move[] moves = board.GetLegalMoves(depth < 1);
        
        if (moves.Length < 1) 
            return new(Move.NullMove, prevBase + (board.IsInCheckmate() ? (1000000000 + depth * 901) * -currentPlayer : 0)); //if possible removing the getpieceValue would be preferable, but for now it's better with it kept there
        
        Move bMove = moves[0];
        float bMoveMat = minFloatValue * currentPlayer;
        ulong key = board.ZobristKey;
        
        var foundTable = boardHashes.TryGetValue(key, out var result);

        if(foundTable && result.depth >= depth)
            return new(result.bestMove, result.boardVal);

        List<(Move move, float Base)> sortedMoves = moves.Select(m => (m, evaluateBase(m, isMaximizingPlayer) )).ToList();
        if(depth < 1) sortedMoves.Add((Move.NullMove, prevBase));
        sortedMoves = sortedMoves.OrderByDescending( // use OrderByDescending to get the highest first
            item => 
                foundTable && result.bestMove == item.move && result.depth > 0 ? 10000000 : // if we found the table and this is the best move we want to give it a big score else:
                item.Base - (item.move.IsCapture ? pieceValues[(int)item.move.MovePieceType - 1] / 3 : 0)).ToList(); // if it's a capture it subtracks the attackers value thereby creating MVV-LVA (Most Valuable Victim - Least Valuable Aggressor)
        // Iterate through sortedMoves and evaluate potential moves
        foreach (var (move, Base) in sortedMoves)
        {
            float v = 0;
            bool isDraw = false;
            if (!move.IsNull)
            {

                board.MakeMove(move);

                float newBase = move.IsEnPassant || move.IsCastles ? getPieceValues(board, currentPlayer) : (prevBase + Base * currentPlayer); // if it is enPassent we recalculate the move

                //float total = t ? StoredTable.Item1 : newBase + evaluateTop(board, currentPlayer);

                isDraw = board.IsRepeatedPosition() || board.IsFiftyMoveDraw();

                //bool t = true;
                //bool t = boardHashes.TryGetValue(zobristKey, out var StoredTable);
                v =
                    (
                    depth > -3 ?
                    miniMax(board, depth - 1, -currentPlayer, min, max, newBase, ply + 1).Item2 : // use minimax if the depth is bigger than 0
                    newBase + (board.IsInCheckmate() ? (1000000000 + depth * 901) * currentPlayer : 0) // use the stored value or get piece values new
                    );


            }
            else v = Base;

            //if(depth == maxSearchDepth) //#DEBUG
            //{//#DEBUG
            //    //Console.WriteLine($"{move}: {v}");//#DEBUG
            //    //Console.WriteLine($"{v}");//#DEBUG
            //}//#DEBUG

            board.UndoMove(move);

            if (!isDraw && isMaximizingPlayer ? v >= bMoveMat : v <= bMoveMat) // move is better
            {
                bMove = move;
                bMoveMat = v;

                // alpha beta
                if (isMaximizingPlayer) min = Max(min, v);
                else max = Min(max, v);
                if (max < min) break;
            }

        }

        boardHashes[key] = (bMoveMat, depth, bMove, ply+boardHashCounter); ///old comment: using tryadd instead of checking if it exist and using add as it seems to be 600-800ms faster.

        return new(bMove, bMoveMat);
    }

    /* private int ManhattanDistance(Square square1, Square square2)
     {
     int dx = Math.Abs(square1.File - square2.File);
     int dy = Math.Abs(square1.Rank - square2.Rank);
     return dx + dy;
     } */
     float getPieceValues(Board board, int currentPlayer) =>
        board.GetAllPieceLists().SelectMany(x => x).Sum(p =>
            getPieceValue(p.PieceType, p.Square, p.IsWhite) * (p.IsWhite ? 1 : -1));



    //the DEBUGS are in place even tho it's called twice becaus in the end it shouldt be called more than once

    // getPieceValue
    // gets the value of one piece depending on its type and its position on the board
    // pieceType: the type of the piece that should be avaluated
    // s: the sqare the piece is standing on (only used to calculate piece sqare tables)
    // isWhite: if the piece is white. used to flip the board if necessary 
    private float getPieceValue(PieceType pieceType, Square s, bool IsWhite) //#DEBUG 
    { //#DEBUG
        //float endGameBonus = 0; //commenting out the endgame bonus for now as it is unused
        int pieceTypeIndex = (int)pieceType - 1;
        if (pieceTypeIndex < 0) return 0;

        //int x = s.File, y = s.Rank;
        ////Console.WriteLine(((x > 3 ? 7 - x : x /* this mirrors the table*/) + y * 4 + pieceTypeIndex * 32) * 2);
        ////if(IsEndgameNoFunction && pieceTypeIndex == 6)
        ////{
        ////     //int distanceBonus = 10 * (7 - distanceToEnemyKing); // Adjust the bonus factor as needed
        ////}    
        //return pieceValues[pieceTypeIndex] + pieceSqareValues[
        //    (x > 3 ? 7 - x : x) // this mirrors the table to use less BBS
        //    + (IsWhite ? 7 - y : y) * 4 + pieceTypeIndex * 32 // flip the table if it is white
        //    + (IsEndgameNoFunction ? 192 : 0)] // use endgame values if we are in the endgame
        //        * 5 - 50;
        //Console.WriteLine(((x > 3 ? 7 - x : x /* this mirrors the table*/) + y * 4 + pieceTypeIndex * 32) * 2);
        //if(IsEndgameNoFunction && pieceTypeIndex == 6)
        //{
        //     //int distanceBonus = 10 * (7 - distanceToEnemyKing); // Adjust the bonus factor as needed
        //}    
        return pieceValues[pieceTypeIndex] + pieceSqareValues[
            (s.File > 3 ? 7 - s.File : s.File) // this mirrors the table to use less BBS
            + (IsWhite ? 7 - s.Rank : s.Rank) * 4 + pieceTypeIndex * 32 // flip the table if it is white
            + (IsEndgameNoFunction ? 192 : 0)] // use endgame values if we are in the endgame
                * 5 - 50;
    } //#DEBUG

    int[] toPieceArray(long[] arr) => Array.ConvertAll(arr, element => Enumerable.Range(0, 8).Select(i => int.Parse(element.ToString("D16").Substring(i * 2, 2)))).SelectMany(x => x).ToArray();

        


    //left in the code for now even tho it's unused might be used in the future
    public bool isPieceProtectedAfterMove(Board board, Move move) => !board.SquareIsAttackedByOpponent(move.TargetSquare); //#DEBUG

    float evaluateBase(Move move, bool isWhite)
    {
        
        if (move.IsEnPassant || move.IsCastles) // beause it is a "special" move we just return 0. this is for some reason better than returning below
            return 0;
        return
            -getPieceValue(move.MovePieceType, move.StartSquare, isWhite)  // remove the old piece 
            + getPieceValue(move.IsPromotion ? move.PromotionPieceType : move.MovePieceType, move.TargetSquare, isWhite) // add the new piece (move piece type if it is't promotion. if it is use the promotion piece type)
            + getPieceValue(move.CapturePieceType, move.TargetSquare, !isWhite); // remove the captured piece (plus beacuse we capture the oponements piece wich is good for the current player)
            
    }

    //float evaluateTop(Board board, int currentPlayer) => board.IsInCheckmate() ? 1000000000000 * currentPlayer* maxSearchDepth : 0;

    //ulong prevSeed = 0;
    //ulong smallRandomNumberGenerator(ulong seed = 0, int maxSizeRange = 100)
    //{
    //    if (seed == 0) seed = prevSeed;
    //    prevSeed = (ulong)Abs(Cos(seed * 10) * maxSizeRange);
    //    return prevSeed;
    //}
}
