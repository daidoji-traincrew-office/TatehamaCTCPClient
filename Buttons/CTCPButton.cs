using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TatehamaCTCPClient.Models;

namespace TatehamaCTCPClient.Buttons
{
    /// <summary>
    /// 仮 そのうち抽象クラスにするかも
    /// </summary>
    public class CTCPButton {
        public string Name { get; init; }

        public Point Location { get; init; }

        public ButtonType Type { get; init; }

        public string Label { get; init; }

        public CTCPButton(string name, Point location, ButtonType type, string label) {
            Name = name;
            Location = location;
            Type = type;
            Label = label;
        }

        public CTCPButton(string name, int x, int y, ButtonType type, string label) : this(name, new(x, y), type, label) { }

        public virtual void OnClick() {

        }
    }

    public class CancelButton : CTCPButton {
        public CancelButton(string name, Point location, ButtonType type) : base(name, location, type, "") {
        }

        public CancelButton(string name, int x, int y, ButtonType type) : base(name, x, y, type, "") {
        }

        public override void OnClick() {

        }
    }

    public class MaintainingButton : CTCPButton {
        public MaintainingButton(string name, Point location, ButtonType type) : base(name, location, type, "") {
        }

        public MaintainingButton(string name, int x, int y, ButtonType type) : base(name, x, y, type, "") {
        }

        public override void OnClick() {

        }
    }


    public class LeverDirectionPair(string leverName, LCR direction) {
        public string LeverName { get; init; } = leverName;

        public LCR Direction { get; init; } = direction;
    }
}
