using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaCTCPClient.Models {
    /// <summary>
    /// 仮 そのうち抽象クラスにするかも
    /// </summary>
    public class CTCPButton {
        public string Name { get; init; }

        public Point Location { get; init; }

        public ButtonType Type { get; init; }

        public string Label { get; init; }

        public string LeverName { get; init; }

        public LCR Direction { get; init; }

        public CTCPButton(string name, Point location, ButtonType type, string label, string leverName, LCR direction) {
            Name = name;
            Location = location;
            Type = type;
            Label = label;
            LeverName = leverName;
            Direction = direction;
        }

        public CTCPButton(string name, int x, int y, ButtonType type, string label, string leverName, LCR direction): this(name, new(x, y), type, label, leverName, direction) { }
    }
}
