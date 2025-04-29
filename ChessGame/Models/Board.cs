namespace BlazorApp3.Models
{
    public class Board
    {
        private Piece[,] _tiles;

        public Board()
        {
            _tiles = new Piece[8, 8];

            Piece firstPiece = new Piece("Pawn", "Black", "White");

            _tiles[0, 0] = firstPiece;

        }

        public Piece[,] ReturnTiles()
        {
            return _tiles;
        }

        public bool MovePiece(Board chessBoard, int[] currentPosition, int[] targetPosition)
        {

            int cX = currentPosition[0];
            int cY = currentPosition[1];

            int tX = targetPosition[0];
            int tY = targetPosition[1];

            string tileColor = string.Empty;
            string pieceType = _tiles[cX, cY].ReturnProperties()["Type"];
            string pieceColor = _tiles[cX, cY].ReturnProperties()["Color"];

            if (_tiles[tX, tY].ReturnProperties()["Image"].EndsWith("Black.png"))
            {
                tileColor = "Black";
            }
            else
            {
                tileColor = "White";
            }

            // Get the piece that is moving.
            Piece movingPiece = new Piece(pieceType, pieceColor, tileColor);

            if (TestPiece(movingPiece, currentPosition, targetPosition) == true)
            {

                _tiles[tX, tY] = movingPiece;
                Piece replacementBlank = new Piece("Blank", "", _tiles[cX, cY].ReturnProperties()["TileColor"]);
                _tiles[cX, cY] = replacementBlank;


                return true;
            }
            else
            {
                return false;
            }

        }

        public bool TestPiece(Piece movingPiece, int[] currentTile, int[] targetTile)
        {
            if (_tiles[targetTile[0], targetTile[1]].ReturnProperties()["Color"] != _tiles[currentTile[0], currentTile[1]].ReturnProperties()["Color"])
            {
                // Knight Movement
                if (movingPiece.ReturnProperties()["Type"] == "Knight")
                {
                    if (
                       (targetTile[0] == currentTile[0] + 1 && targetTile[1] == currentTile[1] - 2) == true || // Far Up Left
                       (targetTile[0] == currentTile[0] + 1 && targetTile[1] == currentTile[1] + 2) == true || // Far Down Left
                       (targetTile[0] == currentTile[0] - 1 && targetTile[1] == currentTile[1] - 2) == true || // Far Up Right
                       (targetTile[0] == currentTile[0] - 1 && targetTile[1] == currentTile[1] + 2) == true || // Far Down Right
                       (targetTile[0] == currentTile[0] + 2 && targetTile[1] == currentTile[1] - 1) == true || // Up Far Left
                       (targetTile[0] == currentTile[0] + 2 && targetTile[1] == currentTile[1] + 1) == true || // Down Far Left
                       (targetTile[0] == currentTile[0] - 2 && targetTile[1] == currentTile[1] - 1) == true || // Down Far Right
                       (targetTile[0] == currentTile[0] - 2 && targetTile[1] == currentTile[1] + 1) == true // Down Far Right
                       )
                    {
                        Console.WriteLine("No");
                        return true;
                    }
                }
                // King Movement
                else if (movingPiece.ReturnProperties()["Type"] == "King")
                {
                    List<int> space = new List<int>() { -1, 0, 1 };
                    if (space.Contains((targetTile[0] - currentTile[0])) && space.Contains((targetTile[1] - currentTile[1])))
                    {
                        return true;
                    }
                }
                // Rook Movement
                else if (movingPiece.ReturnProperties()["Type"] == "Rook")
                {
                    if (
                       (targetTile[0] - currentTile[0] == 0) ||
                       (targetTile[1] - currentTile[1] == 0)
                       )
                    {
                        return true;
                    }
                }
                // Bishop Movement
                else if (movingPiece.ReturnProperties()["Type"] == "Bishop")
                {
                    if (
                       (Math.Abs(targetTile[0] - currentTile[0]) == Math.Abs(targetTile[1] - currentTile[1]))

                       )
                    {
                        Console.WriteLine("No");
                        return true;
                    }
                }
                // Queen Movement
                else if (movingPiece.ReturnProperties()["Type"] == "Queen")
                {
                    if (Math.Abs(targetTile[0] - currentTile[0]) == Math.Abs(targetTile[1] - currentTile[1]) || (targetTile[0] - currentTile[0] == 0) || (targetTile[1] - currentTile[1] == 0))
                    {
                        Console.WriteLine("No");
                        return true;
                    }
                }
                // Pawn Movement
                else if (movingPiece.ReturnProperties()["Type"] == "Pawn")
                {
                    List<int> space = new List<int>() { -1, 1 };

                    if (movingPiece.ReturnProperties()["Color"] == "Black")
                    {
                        // If the pawn moves one up.
                        if ( (targetTile[1] == currentTile[1] + 1) && (targetTile[0] == currentTile[0]) && _tiles[targetTile[0], targetTile[1]].ReturnProperties()["Type"] == "Blank")
                        {
                            return true;
                        }

                        // If the pawn moves two up on the first move.
                        else if ((targetTile[1] == currentTile[1] + 2) && (currentTile[1]==1) && (targetTile[0] == currentTile[0]) && _tiles[targetTile[0], targetTile[1]].ReturnProperties()["Type"] == "Blank")
                        {
                            return true;
                        }

                        // If the pawn attacks a piece.
                        else if ((targetTile[1] == currentTile[1] + 1) && (space.Contains(targetTile[0] - currentTile[0])) && (_tiles[targetTile[0], targetTile[1]].ReturnProperties()["Type"] != "Blank"))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        // If the pawn moves one up.
                        if ((targetTile[1] == currentTile[1] - 1) && (targetTile[0] == currentTile[0]) && _tiles[targetTile[0], targetTile[1]].ReturnProperties()["Type"] == "Blank")
                        {
                            return true;
                        }

                        // If the pawn moves two up on the first move.
                        else if ((targetTile[1] == currentTile[1] - 2) && (currentTile[1] == 6) && (targetTile[0] == currentTile[0]) && _tiles[targetTile[0], targetTile[1]].ReturnProperties()["Type"] == "Blank")
                        {
                            return true;
                        }

                        // If the pawn attacks a piece.
                        else if ((targetTile[1] == currentTile[1] - 1) && (space.Contains(targetTile[0] - currentTile[0])) && (_tiles[targetTile[0], targetTile[1]].ReturnProperties()["Type"] != "Blank"))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            return false;
        }
    }
}
