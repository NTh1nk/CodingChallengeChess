# board = [[[[5, 6], [7, 4, 5]], [[3]]], [[[6], [6, 9]], [[7]]], [[[5]], [[9, 8], [6]]]]
board = [[[-1, 3], [5, 1]], [[-6, -4], [0, 9]]]
infinity = float("inf")
maxDepth = 2

def minimax(board, depth, currentPlayer, _min, _max):
    moves = board
    bMoveMat = -infinity * currentPlayer;
    notEval = False
    for newBoard in moves:
        
            
        v = 0
        if depth > 0:
            v = minimax(newBoard, depth - 1, currentPlayer * -1, _min, _max)
        else:
            print("evaluted move to be ", newBoard)
            v = newBoard # evaluate
        if(depth == maxDepth): #debug
            print("ffe ", v) #debug
        if(currentPlayer > 0):
            bMoveMat = max(bMoveMat, v)
            _min = max(_min, v)
            if(v >= _max):
                print("cut off max")                
                break

        else:
            bMoveMat = min(bMoveMat, v)
            _max = min(_max, v)
            if(v <= _min):
                print("cut off min")
                break

    return bMoveMat


print(minimax(board, maxDepth, 1, -infinity, infinity))