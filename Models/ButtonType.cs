using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaCTCPClient.Models {
    public class ButtonType {
        public string Name { get; init; }

        public Point Location { get; init; }

        public Size Size { get; init; }

        public Point LabelPosition { get; init; }

        public Size LabelSize { get; init; }

        public ButtonType(string name, Point location, Size size, Point labelPosition, Size labelSize) {
            Name = name;
            Location = location;
            Size = size;
            LabelPosition = labelPosition;
            LabelSize = labelSize;
        }

        public ButtonType(string name, int x, int y, int width, int height, int labelX, int labelY, int labelWidth, int labelHeight)
            : this(name, new Point(x, y), new Size(width, height), new Point(labelX, labelY), new Size(labelWidth, labelHeight)){ }

    }
}
