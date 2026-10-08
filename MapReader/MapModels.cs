using System.Collections.Generic;

// Shared wire models; this partial contains no bot, UI or process dependencies.
public partial class MapAreaStruc
{
    public class ServerLevel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public Size Size { get; set; }
        public Offset Offset { get; set; }
        public List<List<int>> Map { get; set; }
        public List<Room> Rooms { get; set; }
        public List<MapObject> Objects { get; set; }
        public string Type { get; set; }
    }

    public class Offset
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Size
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class Room
    {
        public int Area { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Contain(int x, int y)
        {
            return x >= X && x < X + Width && y >= Y && y < Y + Height;
        }
    }

    public class MapObject
    {
        public string Type { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

}
