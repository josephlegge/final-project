using BlazorApp3.Models;

namespace BlazorApp3.Services
{
    public class PieceService
    {
        public Piece GeneratePiece(string type, string color, string currentColor)
        {
            Piece newPiece = new Piece(type, color, currentColor);

            return newPiece;
        }
    }
}
