using ChessChallenge.API;
using System;
using static System.Math;
using System.Collections.Generic;
using System.Linq;

public class EvilBot : IChessBot
{
    // right now funktions are seperated. before submision, everything will be compacted into the think function if possible.
    //---this section is variables designated to zobrist hashing and the transportition table---
    int boardHashCounter = 0;
    Dictionary<ulong, (float boardVal, int depth, Move bestMove)> boardHashes = new(); //dict <zobrist key, tuple<total_board_value, depth_iteration, bestMove>>

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

    public bool IsEndgameNoFunction = false;

    //using a variable instead of float.minvalue for BBC saving
    float minFloatValue = float.MinValue;


    float infinity = 1000000; // should work aslong as it's bigger than: 900 + 500 * 2 + 320 * 2 + 300 * 2 + 100 * 8 + 50 * 16 = 4740 (king not included because both colors always has a king

    // debug variables (variables only used for debuging)
    int searchedMoves = 0; //#DEBUG
    int foundCheckMates = 0; //#DEBUG
    int addedZobristKeys = 0; //#DEBUG
    int usedZobristKeys = 0; //#DEBUG
                             // -----------------------------

    Move bestMove;

    int phase = 0;
    int[] phaseValues = { 0, 0, 1, 1, 2, 4, 0 };
    int qd = -20; // quince search depth
    public void updatePhase(Board board) //#DEBUG
    { //#DEBUG
        phase = board.GetAllPieceLists()
            .SelectMany(x => x)
            .Sum(
                p => phaseValues[(int)p.PieceType]);

    }
    public Move Think(Board board, Timer timer)
    {

        pieceSqareValues = toPieceArray(new[] { 4747474776866575, 4649555643514953, 4047465140464645, 3747424147474747, 0022383326366858, 3465586645525363, 4449525141455150, 3932444717413138, 3949243740524244, 4358605946495362, 4651515547525252, 4952524738474341, 5759576255576465, 4653555841444955, 3740444735404343, 3543424542444852, 3947565141364648, 4443495040404343, 4540454543484447, 3745514847424550, 2954524356474245, 4554484343424440, 3347403643434134, 4849452943585132, 4747474799979386, 7476726757545149, 5150474549494648, 5150505047474747, 3137443940454047, 4142505043485454, 4246525541474752, 3542454639334143, 4341444545464944, 4845474747505150, 4648515344475050, 4342454741454146, 5150535251515151, 4949494949485148, 4849504946474647, 4646474845484847, 4554545543535759, 4249506148545460, 4255536143405249, 4141394338394135, 2637424244525152, 5052545245545455, 4246535442475153, 4044495132384144 }); // use https://onlinestringtools.com/split-string to split into 16 long parts
        updatePhase(board);
        Console.WriteLine("Phase: " + phase); //#Debug

        weAreWhite = board.IsWhiteToMove;
        Console.WriteLine("---calculate new move--- " + (weAreWhite ? "W" : "B")); //#DEBUG
        bestMove = Move.NullMove;
        for (int depth = 1; depth <= 30; depth++)
        {
            miniMax(board, depth, weAreWhite ? 1 : -1, -infinity + 10, infinity - 10, getPieceValues(board) * (weAreWhite ? 1 : -1), 0, timer);
            if (timer.MillisecondsElapsedThisTurn > timer.MillisecondsRemaining / 60)
            { // #DEBUG
                Console.WriteLine("reached depth: " + depth); //#DEBUG
                break;
            }
            if (timer.MillisecondsRemaining < 3000)
                qd = 0;
        }


        //if (boardHashes.Count > 9500)
        //{ //#DEBUG
        //    Console.WriteLine("flushing bordhashes buffer"); //#DEBUG
        //} //#DEBUG
        boardHashes.Clear();

        Console.WriteLine("found checkmate: " + foundCheckMates + " times this turn"); //#DEBUG
        foundCheckMates = 0; //#DEBUG

        Console.WriteLine(searchedMoves + " Searched moves"); //#DEBUG

        Console.WriteLine("adding: " + addedZobristKeys + " deep seached zobrist keys this turn"); //#DEBUG
        addedZobristKeys = 0; //#DEBUG

        Console.WriteLine("found: " + usedZobristKeys + " positions already calculated this turn"); //#DEBUG
        usedZobristKeys = 0; //#DEBUG

        Console.WriteLine("dececion took: " + timer.MillisecondsElapsedThisTurn + " ms this turn"); //#DEBUG

        boardHashCounter = +1;
        //foreach (ulong i in boardHashes.Keys) if (boardHashes[i].Item2 < boardHashCounter - maxSearchDepth) boardHashes.Remove(i); 

        return bestMove;
        //Console.WriteLine(isPieceProtectedAfterMove(board, moves[0]));

    }

    private float miniMax(Board board, int depth, int currentPlayer, float min, float max, float prevBase, int ply, Timer timer)
    {
        //bool isMaximizingPlayer = currentPlayer > 0; // could also be called isWhite
        Move[] moves = board.GetLegalMoves(depth <= 0);

        if (moves.Length < 1) // if there are no legal moves we can do
            return depth > 0 && board.IsInCheck() ? // if we are in check
                -infinity // we give it a low score (we cant be doing the checkmate because we don't have any legal moves)
                : prevBase; // if not checkmate we just return the prevBase, because nothing can have changed

        Move bMove = moves[0];
        float bMoveMat = -infinity;
        ulong key = board.ZobristKey;
        var foundTable = boardHashes.TryGetValue(key, out var result);
        if (foundTable && result.depth >= depth)
            return result.boardVal * currentPlayer;
        if (depth < 1)
        {
            bMove = Move.NullMove;
            bMoveMat = prevBase;

            min = Max(min, prevBase);
        }
        var storedBestMove = result.bestMove.RawValue; // this automaticly happens when we do move == otherMove, but it's slighty faster do to only calculating it once. can be removed if needed, token wise
        List<(Move move, float Base)> sortedMoves = moves.Select(m => (m, evaluateBase(m, currentPlayer > 0))).ToList();
        // if(depth < 1) sortedMoves.Add(new (Move.NullMove, prevBase));
        sortedMoves = sortedMoves.OrderByDescending(
            item => foundTable && storedBestMove == item.move.RawValue && result.depth > qd ? infinity
        : item.Base - (item.move.IsCapture ? pieceValues[(int)item.move.MovePieceType - 1] / 3 : 0)).ToList(); // if it's a capture it subtracks the attackers value thereby creating MVV-LVA (Most Valuable Victim - Least Valuable Aggressor)

        // Iterate through sortedMoves and evaluate potential moves
        foreach (var (move, Base) in sortedMoves)
        {
            // if (timer.MillisecondsElapsedThisTurn > timer.MillisecondsRemaining / 30) return infinity;

            float v = 0;
            if (move.IsNull) v = prevBase;
            else
            {
                bool isDraw = board.IsRepeatedPosition() || board.IsFiftyMoveDraw();

                board.MakeMove(move);

                float newBase = move.IsEnPassant || move.IsCastles ? getPieceValues(board) * currentPlayer : (prevBase + Base); // if it is enPassent we recalculate the move



                v = isDraw ? -50 :
                    depth > qd ? //if
                        -miniMax(board, depth - 1, -currentPlayer, -max, -min, -newBase, ply + 1, timer) : //if the depth is bigger than qd (q search depth) use minimax (we swap max and min because the player has changed)
                        newBase
                    ;

                board.UndoMove(move);
            }



            if (v > bMoveMat)
            {
                // improve best move and the best moves result
                bMove = move;
                bMoveMat = v;

                // alpha beta
                min = Max(min, v);
                if (max <= min)
                    break;
            }

        }


        boardHashes[key] = (bMoveMat * currentPlayer, depth, bMove);


        if (ply < 1) bestMove = bMove; // if it's root we want to asign global best move to local best move
        return bMoveMat;
    }

    /* private int ManhattanDistance(Square square1, Square square2)
    {
    int dx = Math.Abs(square1.File - square2.File);
    int dy = Math.Abs(square1.Rank - square2.Rank);
    return dx + dy;
    } */
    float getPieceValues(Board board) =>
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
        int flatPos = (s.File > 3 ? 7 - s.File : s.File) // this mirrors the table to use less BBS
            + (IsWhite ? 7 - s.Rank : s.Rank) * 4 // flip the table if it is white
            + pieceTypeIndex * 32; // choose the correct table depending on what type of piece
        return pieceValues[pieceTypeIndex] + (pieceSqareValues[flatPos] * phase + pieceSqareValues[flatPos + 192] * (24 - phase)) / 24 * 3.5f - 167;
    } //#DEBUG

    int[] toPieceArray(long[] arr) => Array.ConvertAll(arr, element => Enumerable.Range(0, 8).Select(i => int.Parse(element.ToString("D16").Substring(i * 2, 2)))).SelectMany(x => x).ToArray();




    //left in the code for now even tho it's unused might be used in the future
    public bool isPieceProtectedAfterMove(Board board, Move move) => !board.SquareIsAttackedByOpponent(move.TargetSquare); //#DEBUG

    float evaluateBase(Move move, bool isWhite)
    {
        //if (move.IsEnPassant || move.IsCastles) // beause it is a "special" move we just return 0. this is for some reason better than returning below
        //    return 0;
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
