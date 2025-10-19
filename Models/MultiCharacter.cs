using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaCTCPClient.Models {
    public class MultiCharacter {

        public string Name { get; init; }

        public Point Location { get; init; }

        public Size Size { get; init; }

        public MultiCharacter(string name, Point location, Size size) {
            Name = name;
            Location = location;
            Size = size;
        }

        public MultiCharacter(string name, Point location, int width, int height) : this(name, location, new Size(width, height)) { }

        public MultiCharacter(string name, int x, int y, Size size) : this(name, new Point(x, y), size) { }

        public MultiCharacter(string name, int x, int y, int width, int height) : this(name, new Point(x, y), new Size(width, height)) { }
    }
}
