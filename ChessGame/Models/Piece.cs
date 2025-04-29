using System.Drawing;

namespace BlazorApp3.Models
{
    public class Piece
    {
        private string _type;
        private string _color;
        private string _tileColor;
        public string _image;

        public Piece(string type, string color, string tileColor)
        {
            _type = type;
            _color = color;
            _tileColor = tileColor;
            SetImage();
        }

        public Dictionary<string,string> ReturnProperties()
        {
            
            Dictionary<string, string> propertiesDictionary = new Dictionary<string, string>();

            propertiesDictionary.Add("Type",_type);
            propertiesDictionary.Add("Color",_color);
            propertiesDictionary.Add("Image",_image);
            propertiesDictionary.Add("TileColor", _tileColor);

            return (propertiesDictionary);

        }

        public void SetImage()
        {
            _image = $"images/{_color}{_type}{_tileColor}.png";
        }

    }
}
