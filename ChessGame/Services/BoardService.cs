using System.Drawing;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using BlazorApp3.Components.Pages;
using BlazorApp3.Models;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;

namespace BlazorApp3.Services
{
    public class BoardService
    {

        public Board chessBoard = new Board();

        public bool gameBegan = false;
        public bool pieceChosen = false;
        public bool spinBoard = false;
        public bool targetChosen = false;

        public List<Piece> takenPieces = new List<Piece>();

        public string currentColor = "Black";
        public string currentSelectedPiece = string.Empty;
        public string image = "images/BlankSpaceBlack.png";
        public string checkMessage = string.Empty;
        public string playerTurn = "White";
        public string winMessage = string.Empty;

        public int[] currentPosition = new int[2];
        public int[] targetPosition = new int[2];

        public void SwitchColor()
        {
            if (playerTurn == "White")
            {
                playerTurn = "Black";
            }
            else
            {
                playerTurn = "White";
            }
        }

        // this might go in a service later
        public void SwitchBoardColor()
        {
            if (image == "images/WhiteBlank.png")
            {
                image = "images/BlackBlank.png";
                currentColor = "Black";
            }
            else
            {
                image = "images/WhiteBlank.png";
                currentColor = "White";
            }
        }

        /// <summary>
        /// Starts the game.
        /// </summary>
        public void Start()
        {
            winMessage = string.Empty;
            playerTurn = "White";
            gameBegan = true;

            foreach (int[] move in AvailableMoves(0, 0))
            {
                Console.WriteLine(move[0] + " " + move[1]);
            }
        }

        /// <summary>
        /// Moves a piece.
        /// </summary>
        /// <param name="boardService"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Move(BoardService boardService, int x, int y)
        {

            if (boardService.pieceChosen == false && boardService.targetChosen == false && (chessBoard.ReturnTiles()[x, y].ReturnProperties()["Color"] == boardService.playerTurn))
            {
                boardService.currentPosition[0] = x;
                boardService.currentPosition[1] = y;

                if (chessBoard.ReturnTiles()[x, y].ReturnProperties()["Type"] != "Blank" && boardService.targetPosition != boardService.currentPosition)
                {
                    // Display a message on screen with information about the piece. This can help them locate which piece is selected incase they want to put it down.
                    currentSelectedPiece = chessBoard.ReturnTiles()[x, y].ReturnProperties()["Color"] + " " + chessBoard.ReturnTiles()[x, y].ReturnProperties()["Type"] + " [" + (boardService.currentPosition[0] + 1) + "," + (boardService.currentPosition[1] + 1) + "] ";

                    boardService.pieceChosen = true;
                }
            }
            else if (boardService.pieceChosen == true && boardService.targetChosen == false)
            {

                boardService.targetPosition[0] = x;
                boardService.targetPosition[1] = y;

                // Check if it's the same place or pieces are blocking it.
                if ((boardService.targetPosition[0] == boardService.currentPosition[0] && boardService.targetPosition[1] == boardService.currentPosition[1]) == false)
                {
                    Piece movingPiece = chessBoard.ReturnTiles()[currentPosition[0], currentPosition[1]];
                    if (BlockTest(chessBoard.ReturnTiles()[currentPosition[0], currentPosition[1]].ReturnProperties()["Type"], currentPosition, targetPosition))
                    {
                        Piece targetPiece = chessBoard.ReturnTiles()[targetPosition[0], targetPosition[1]];
                        if (chessBoard.MovePiece(chessBoard, boardService.currentPosition, boardService.targetPosition))
                        {


                            boardService.pieceChosen = false;
                            boardService.targetChosen = false;
                            currentSelectedPiece = string.Empty;

                            // If the piece is a pawn turning that reached the end.
                            if (movingPiece.ReturnProperties()["Type"] == "Pawn")
                            {
                                // If it's black, it must reach the bottom of the board.
                                if (movingPiece.ReturnProperties()["Color"] == "Black")
                                {
                                    if (targetPosition[1]==7)
                                    {
                                        Piece newQueen = new Piece("Queen", "Black", movingPiece.ReturnProperties()["TileColor"]);
                                        chessBoard.ReturnTiles()[targetPosition[0], targetPosition[1]] = newQueen;
                                    }
                                }
                                // If it's white, it must reach the top of the board.
                                else if (movingPiece.ReturnProperties()["Color"] == "White")
                                {
                                    if (targetPosition[1] == 0)
                                    {
                                        Piece newQueen = new Piece("Queen", "White", movingPiece.ReturnProperties()["TileColor"]);
                                        chessBoard.ReturnTiles()[targetPosition[0], targetPosition[1]] = newQueen;
                                    }
                                }
                            }

                            // Don't let the king move himself into check.
                            if (movingPiece.ReturnProperties()["Type"] == "King" && CheckCheck() != string.Empty)
                            {
                                chessBoard.ReturnTiles()[currentPosition[0], currentPosition[1]] = movingPiece;
                                chessBoard.ReturnTiles()[targetPosition[0], targetPosition[1]] = targetPiece;
                                checkMessage = "Don't move yourself into check!";
                            }
                            else if (movingPiece.ReturnProperties()["Type"] != "King" && CheckCheck() == movingPiece.ReturnProperties()["Color"])
                            {
                                chessBoard.ReturnTiles()[currentPosition[0], currentPosition[1]] = movingPiece;
                                chessBoard.ReturnTiles()[targetPosition[0], targetPosition[1]] = targetPiece;
                                checkMessage = "You need to protect your king!";
                            }
                            else
                            {
                                // If it's not blank, add the piece to the taken pieces list.
                                if (targetPiece.ReturnProperties()["Type"] != "Blank")
                                {
                                    takenPieces.Add(targetPiece);
                                }
                                boardService.SwitchColor();
                                CheckCheck();
                            }
                        }
                    }
                }
                else
                {
                    boardService.pieceChosen = false;
                    boardService.targetChosen = false;
                    currentSelectedPiece = string.Empty;
                }
            }
            else
            {
                boardService.pieceChosen = false;
                boardService.targetChosen = false;
                currentSelectedPiece = string.Empty;
            }

        }

        /// <summary>
        /// Checks to see if any pieces are blocking a rook, bishop or queen. Returns true if nothing is blocking it.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="currentPosition"></param>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        public bool BlockTest(string type, int[] currentPosition, int[] targetPosition)
        {

            if ((currentPosition[0] == targetPosition[0] && currentPosition[1] == targetPosition[1]) == false)
            {
                List<Piece> PiecesInWay = new List<Piece>();
                List<Piece> RookBlockers = new List<Piece>();
                List<Piece> BishopBlockers = new List<Piece>();

                List<int> ints = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };

                // If Type is a rook.
                bool CheckLine()
                {
                    if (currentPosition[0] == targetPosition[0] || currentPosition[1] == targetPosition[1])
                    {
                        // Check if the X is the same. (The rook is going up or down.)
                        int direction;
                        if (currentPosition[0] == targetPosition[0])
                        {
                            direction = 1;
                        }
                        else
                        {
                            direction = 0;
                        }
                        // Make variables for the larger and smaller Y positions.
                        int smaller;
                        int larger;
                        // Get the smaller Y position.
                        if (targetPosition[direction] > currentPosition[direction])
                        {
                            smaller = currentPosition[direction];
                            larger = targetPosition[direction];
                        }
                        else
                        {
                            smaller = targetPosition[direction];
                            larger = currentPosition[direction];
                        }
                        // Loop through each number between them.

                        foreach (int i in ints.GetRange(smaller, larger - smaller - 1))
                        {

                            Piece checking;
                            if (direction == 1)
                            {
                                checking = chessBoard.ReturnTiles()[currentPosition[0], i];
                            }
                            else
                            {
                                checking = chessBoard.ReturnTiles()[i, currentPosition[1]];
                            }
                            // Get the piece and then check if it's blank or not.
                            if (checking.ReturnProperties()["Type"] != "Blank")
                            {
                                // If it's not blank, then add it to a list.
                                RookBlockers.Add(checking);
                            }
                        }
                        if (RookBlockers.Count > 0)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }

                bool CheckSlant()
                {
                    if ((currentPosition[0] == targetPosition[0] || currentPosition[1] == targetPosition[1]) == false)
                    {
                        // Check if the X is the same. (The rook is going up or down.)
                        int direction;

                        int smallerX;
                        int smallerY;
                        int largerX;
                        int largerY;

                        int xDirection;
                        int yDirection;

                        // Set the largest X
                        if (targetPosition[0] > currentPosition[0])
                        {
                            smallerX = currentPosition[0];
                            largerX = targetPosition[0];

                            xDirection = 1;
                        }
                        else
                        {
                            smallerX = targetPosition[0];
                            largerX = currentPosition[0];

                            xDirection = -1;
                        }

                        // Set the largest Y
                        if (targetPosition[1] > currentPosition[1])
                        {
                            smallerY = currentPosition[1];
                            largerY = targetPosition[1];

                            yDirection = 1;
                        }
                        else
                        {
                            smallerY = targetPosition[1];
                            largerY = currentPosition[1];

                            yDirection = -1;
                        }

                        //
                        int distance = largerX - smallerX;
                        int tempX = currentPosition[0] + (1 * xDirection);
                        int tempY = currentPosition[1] + (1 * yDirection);

                        // Move 
                        for (int i = 0; i < distance - 1; i++)
                        {
                            // If 
                            if (chessBoard.ReturnTiles()[tempX, tempY].ReturnProperties()["Type"] != "Blank")
                            {
                                BishopBlockers.Add(chessBoard.ReturnTiles()[tempX, tempY]);
                            }
                            tempX = tempX + (1 * xDirection);
                            tempY = tempY + (1 * yDirection);
                        }
                        if (BishopBlockers.Count > 0)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }

                if (type == "Rook")
                {
                    if (CheckLine() == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (type == "Bishop")
                {
                    if (CheckSlant() == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (type == "Queen")
                {
                    if (CheckSlant() == true && CheckLine() == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }



        }

        /// <summary>
        /// Spins the board of the rotate setting is on.
        /// </summary>
        public void SpinBoard()
        {
            if (spinBoard == false)
            {
                spinBoard = true;
            }
            else
            {
                spinBoard = false;
            }
        }

        /// <summary>
        /// Quits the game so you can start a new one.
        /// </summary>
        public void QuitGame()
        {
            checkMessage = " ";
            playerTurn = "White";
            gameBegan = false;
        }

        /// <summary>
        /// Saves the game
        /// </summary>
        public void SaveGame()
        {

            // The File.
            string file = @"savedgame.txt";

            // Delete the file if it already exists.
            if (File.Exists(file))
            {
                File.Delete(file);
            }

            // Using the streamwriter.
            using (StreamWriter sw = File.AppendText(file))
            {
                // Get every tile on the board.
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        // Write the piece's X and Y coordinate, then the type, color, and tyle color. 
                        sw.WriteLine($"{x},{y},{chessBoard.ReturnTiles()[x, y].ReturnProperties()["Type"]},{chessBoard.ReturnTiles()[x, y].ReturnProperties()["Color"]},{chessBoard.ReturnTiles()[x, y].ReturnProperties()["TileColor"]}");
                    }
                }
                // Then write whose turn it is and the check message.
                sw.WriteLine($"Turn,{playerTurn}");
                sw.WriteLine($"Check, {checkMessage}");

            }
        }

        /// <summary>
        /// Loads the game.
        /// </summary>
        public void LoadGame()
        {
            gameBegan = true;

            string file = @"savedgame.txt";

            // My exception handling.
            try
            {
                using (StreamReader sw = File.OpenText(file))
                {
                    for (int i = 0; i < 66; i++)
                    {
                        string[] line = sw.ReadLine().Split(",");
                        if (line[0] != "Turn" && line[0] != "Check")
                        {
                            int x = (int.Parse(line[0]));
                            int y = (int.Parse(line[1]));
                            Piece newPiece = new Piece(line[2], line[3], line[4]);

                            chessBoard.ReturnTiles()[x, y] = newPiece;
                        }
                        else if (line[0] == "Turn")
                        {
                            playerTurn = line[1];
                        }
                        else if (line[0] == "Check")
                        {
                            checkMessage = line[1];
                        }
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("No!");
            }

            CheckCheck();
        }

        public string CheckCheck()
        {

            checkMessage = " ";

            string check = string.Empty;

            int kingCount = 0;
            int kingX = 0;
            int kingY = 0;

            // Look through each X and Y.
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    // If the X and Y on the board is a king.
                    if (chessBoard.ReturnTiles()[x, y].ReturnProperties()["Type"] == "King")
                    {
                        kingCount++;
                        kingX = x;
                        kingY = y;

                        List<int[]> tilesOnBoard = new List<int[]>();
                        List<int[]> threateningPieces = new List<int[]>();
                        bool checkMate = true;

                        // Get other pieces.
                        for (int y2 = 0; y2 < 8; y2++)
                        {
                            for (int x2 = 0; x2 < 8; x2++)
                            {
                                if (CheckForDanger(kingX, kingY, x2, y2))
                                {
                                    checkMessage = "Check!";
                                    return chessBoard.ReturnTiles()[kingX, kingY].ReturnProperties()["Color"];
                                    // See if the tiles around the king are actually on the board.
                                    // Left

                                }
                            }
                        }
                        foreach (int[] tile in GetThreateningPieces(kingX, kingY))
                        {
                            Console.WriteLine(tile[0] + " " + tile[1]);
                            threateningPieces.Add([tile[0], tile[1]]);
                            if (GetThreateningPieces(tile[0], tile[1]).Count() > 0)
                            {
                                checkMessage = "Check!";
                                return chessBoard.ReturnTiles()[kingX, kingY].ReturnProperties()["Color"];
                            }
                        }
                    }
                }
            }

            // Check if one of the kings is missing for some reason.
            if (kingCount < 2)
            {

                // Check each piece in each Y and X to see if it's the remaining king.
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        if (chessBoard.ReturnTiles()[x, y].ReturnProperties()["Type"] == "King")
                        {
                            winMessage = $"{chessBoard.ReturnTiles()[x, y].ReturnProperties()["Color"]} player wins!";
                            gameBegan = false;
                        }
                    }
                }

            }

            return string.Empty;

        }

        // might replace this with GetThreateningPieces
        public bool CheckForDanger(int testingX, int testingY, int x2, int y2)
        {

            // New King temporary variable.
            Piece king = chessBoard.ReturnTiles()[testingX, testingY];
            // Temp piece.
            Piece tempPiece = chessBoard.ReturnTiles()[x2, y2];

            // Narrow it down to parts of the opposite color.
            if (tempPiece.ReturnProperties()["Color"] != king.ReturnProperties()["Color"])
            {

                // If the piece is a rook.
                if (chessBoard.ReturnTiles()[x2, y2].ReturnProperties()["Type"] == "Rook")
                {
                    if (testingX == x2 || testingY == y2)
                    {
                        if (BlockTest("Rook", [testingX, testingY], [x2, y2]) == false)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }

                // If the piece is a knight.
                if (chessBoard.ReturnTiles()[x2, y2].ReturnProperties()["Type"] == "Knight")
                {
                    if ((testingX == x2 + 1 && testingY == y2 - 2) ||
                    (testingX == x2 + 1 && testingY == y2 + 2) ||
                    (testingX == x2 - 1 && testingY == y2 - 2) ||
                    (testingX == x2 - 1 && testingY == y2 + 2) ||
                    (testingX == x2 + 2 && testingY == y2 - 1) ||
                    (testingX == x2 + 2 && testingY == y2 + 1) ||
                    (testingX == x2 - 2 && testingY == y2 - 1) ||
                        (testingX == x2 - 2 && testingY == y2 + 1))
                    {
                        return true;
                    }
                }

                // If the piece is a pawn.
                if (chessBoard.ReturnTiles()[x2, y2].ReturnProperties()["Type"] == "Pawn")
                {
                    if (chessBoard.ReturnTiles()[x2, y2].ReturnProperties()["Color"] == "Black")
                    {
                        if (((testingX == x2 + 1) || (testingX == x2 - 1)) && testingY == y2 + 1)
                        {
                            return true;
                        }
                    }
                    else if (chessBoard.ReturnTiles()[x2, y2].ReturnProperties()["Color"] == "White")
                    {
                        if (((testingX == x2 + 1) || (testingX == x2 - 1)) && testingY == y2 - 1)
                        {
                            return true;
                        }
                    }
                }

                // If the piece is the other king.
                if (chessBoard.ReturnTiles()[x2, y2].ReturnProperties()["Type"] == "King")
                {
                    if (Math.Abs(testingX - x2) < 2 && Math.Abs(testingY - y2) < 2)
                    {
                        return true;
                    }
                }

                // If the piece is a bishop.
                if (chessBoard.ReturnTiles()[x2, y2].ReturnProperties()["Type"] == "Bishop")
                {

                    if (Math.Abs(testingX - x2) == Math.Abs(testingY - y2))
                    {
                        if (BlockTest("Bishop", [testingX, testingY], [x2, y2]) == false)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }

                // If the piece is a queen.
                if (chessBoard.ReturnTiles()[x2, y2].ReturnProperties()["Type"] == "Queen")
                {
                    // If it's diagonal like a bishop.
                    if (Math.Abs(testingX - x2) == Math.Abs(testingY - y2))
                    {
                        if (BlockTest("Bishop", [testingX, testingY], [x2, y2]) == false)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    // If it's straight across like a rook.
                    else if (testingX == x2 || testingY == y2)
                    {
                        if (BlockTest("Rook", [testingX, testingY], [x2, y2]) == false)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            return false;

        }

        public List<int[]> GetThreateningPieces(int testingX, int testingY)
        {
            // New King temporary variable.
            Piece testPiece = chessBoard.ReturnTiles()[testingX, testingY];

            List<int[]> threateningPieces = new List<int[]>();

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {

                    // Temp piece.
                    Piece tempPiece = chessBoard.ReturnTiles()[x, y];

                    // Narrow it down to parts of the opposite color.
                    if (tempPiece.ReturnProperties()["Color"] != testPiece.ReturnProperties()["Color"])
                    {

                        // If the piece is a rook.
                        if (chessBoard.ReturnTiles()[x, y].ReturnProperties()["Type"] == "Rook")
                        {
                            if (testingX == x || testingY == y)
                            {
                                if (BlockTest("Rook", [testingX, testingY], [x, y]) == true)
                                {
                                    threateningPieces.Add([x, y]);
                                }
                            }
                        }

                        // If the piece is a knight.
                        if (chessBoard.ReturnTiles()[x, y].ReturnProperties()["Type"] == "Knight")
                        {
                            if ((testingX == x + 1 && testingY == y - 2) ||
                            (testingX == x + 1 && testingY == y + 2) ||
                            (testingX == x - 1 && testingY == y - 2) ||
                            (testingX == x - 1 && testingY == y + 2) ||
                            (testingX == x + 2 && testingY == y - 1) ||
                            (testingX == x + 2 && testingY == y + 1) ||
                            (testingX == x - 2 && testingY == y - 1) ||
                                (testingX == x - 2 && testingY == y + 1))
                            {
                                threateningPieces.Add([x, y]);
                            }
                        }

                        // If the piece is a pawn.
                        if (chessBoard.ReturnTiles()[x, y].ReturnProperties()["Type"] == "Pawn")
                        {
                            if (chessBoard.ReturnTiles()[x, y].ReturnProperties()["Color"] == "Black")
                            {
                                if (((testingX == x + 1) || (testingX == x - 1)) && testingY == y + 1)
                                {
                                    threateningPieces.Add([x, y]);
                                }
                            }
                            else if (chessBoard.ReturnTiles()[x, y].ReturnProperties()["Color"] == "White")
                            {
                                if (((testingX == x + 1) || (testingX == x - 1)) && testingY == y - 1)
                                {
                                    threateningPieces.Add([x, y]);
                                }
                            }
                        }

                        // If the piece is the other king.
                        if (chessBoard.ReturnTiles()[x, y].ReturnProperties()["Type"] == "King")
                        {
                            if (Math.Abs(testingX - x) < 2 && Math.Abs(testingY - y) < 2)
                            {
                                threateningPieces.Add([x, y]);
                            }
                        }

                        // If the piece is a bishop.
                        if (chessBoard.ReturnTiles()[x, y].ReturnProperties()["Type"] == "Bishop")
                        {

                            if (Math.Abs(testingX - x) == Math.Abs(testingY - y))
                            {
                                if (BlockTest("Bishop", [testingX, testingY], [x, y]) == true)
                                {
                                    threateningPieces.Add([x, y]);
                                }
                            }
                        }

                        // If the piece is a queen.
                        if (chessBoard.ReturnTiles()[x, y].ReturnProperties()["Type"] == "Queen")
                        {
                            // If it's diagonal like a bishop.
                            if (Math.Abs(testingX - x) == Math.Abs(testingY - y))
                            {
                                if (BlockTest("Bishop", [testingX, testingY], [x, y]) == true)
                                {
                                    threateningPieces.Add([x, y]);
                                }
                            }
                            // If it's straight across like a rook.
                            else if (testingX == x || testingY == y)
                            {
                                if (BlockTest("Rook", [testingX, testingY], [x, y]) == true)
                                {
                                    threateningPieces.Add([x, y]);
                                }
                            }
                        }
                    }
                }
            }
            return threateningPieces;
        }

        public List<int[]> AvailableMoves(int testingX, int testingY)
        {
            List<int[]> availableTiles = new List<int[]>();
            Piece testingPiece = chessBoard.ReturnTiles()[testingX, testingY];

            // X and Y will be the target this time.
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {

                    Piece tempPiece = chessBoard.ReturnTiles()[x, y];

                    if (testingPiece.ReturnProperties()["Color"] != tempPiece.ReturnProperties()["Color"])
                    {
                        // If the piece is a rook.
                        if (testingPiece.ReturnProperties()["Type"] == "Rook")
                        {
                            if ((testingX == x || testingY == y) && BlockTest("Rook", [testingX, testingX], [x, y]))
                            {
                                availableTiles.Add([x, y]);
                            }
                        }

                        // If the piece is a pawn.
                        if (testingPiece.ReturnProperties()["Type"] == "Pawn")
                        {
                            List<int> space = new List<int>() { -1, 1 };

                            if (testingPiece.ReturnProperties()["Color"] == "Black")
                            {

                                // If the pawn moves one up.
                                if ((y == testingY + 1) && (x == testingX) && chessBoard.ReturnTiles()[x, y].ReturnProperties()["Type"] == "Blank")
                                {

                                    availableTiles.Add([x, y]);


                                }
                                // If the pawn moves two up on the first move.
                                else if ((y == testingY + 2) && (testingY == 1) && (x == testingX) && chessBoard.ReturnTiles()[x, y].ReturnProperties()["Type"] == "Blank")
                                {
                                    if ((testingX == x && testingY == y) == false)
                                    {
                                        availableTiles.Add([x, y]);
                                    }
                                }

                                // If the pawn attacks a piece.
                                else if ((y == testingY + 1) && (space.Contains(x - testingX)) && (chessBoard.ReturnTiles()[x, y].ReturnProperties()["Type"] != "Blank"))
                                {
                                    if ((testingX == x && testingY == y) == false)
                                    {
                                        availableTiles.Add([x, y]);
                                    }
                                }
                            }
                            else
                            {
                                // If the pawn moves one up.
                                if ((y == testingY - 1) && (x == testingX) && chessBoard.ReturnTiles()[x, y].ReturnProperties()["Type"] == "Blank")
                                {
                                    if ((testingX == x && testingY == y) == false)
                                    {
                                        availableTiles.Add([x, y]);
                                    }
                                }

                                // If the pawn moves two up on the first move.
                                else if ((y == testingY - 2) && (testingY == 6) && (x == testingX) && chessBoard.ReturnTiles()[x, y].ReturnProperties()["Type"] == "Blank")
                                {
                                    if ((testingX == x && testingY == y) == false)
                                    {
                                        availableTiles.Add([x, y]);
                                    }
                                }

                                // If the pawn attacks a piece.
                                else if ((y == testingY - 1) && (space.Contains(x - testingX)) && (chessBoard.ReturnTiles()[x, y].ReturnProperties()["Type"] != "Blank"))
                                {
                                    if ((testingX == x && testingY == y) == false)
                                    {
                                        availableTiles.Add([x, y]);
                                    }
                                }
                            }
                        }

                        // If the piece is a bishop.
                        if (testingPiece.ReturnProperties()["Type"] == "Bishop")
                        {
                            if ((Math.Abs(testingX - x) == Math.Abs(testingY - y)) && BlockTest("Bishop", [testingX, testingX], [x, y]))
                            {
                                availableTiles.Add([x, y]);
                            }
                        }
                    }

                }
            }

            foreach (int[] tiles in availableTiles)
            {
                Console.WriteLine(tiles[0] + " " + tiles[1]);
            }

            return availableTiles;
        }
    }
}
