using TatehamaCTCPClient.Models;

namespace TatehamaCTCPClient.Buttons
{
    /// <summary>
    /// 仮 そのうち抽象クラスにするかも
    /// </summary>
    public abstract class CTCPButton {
        public string Name { get; init; }

        public Point Location { get; init; }

        public ButtonType Type { get; init; }

        public string Label { get; init; }

        public virtual LightingType Lighting => LightingType.NONE;

        public virtual bool NeedsUpdate => false;

        public virtual bool Enabled => true;


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

        public static bool Active { get; private set; } = false;

        public override LightingType Lighting => Active ? LightingType.LIGHTING : LightingType.NONE;

        public override bool NeedsUpdate => true;
        public CancelButton(string name, Point location, ButtonType type) : base(name, location, type, "") {
        }

        public CancelButton(string name, int x, int y, ButtonType type) : base(name, x, y, type, "") {
        }

        public override void OnClick() {
            Active = !Active;
        }

        public static void MakeInactive() {
            Active = false;
        }
    }

    public class HikipperButton : CTCPButton {

        public static bool Active { get; private set; } = false;

        public override LightingType Lighting => Active ? LightingType.LIGHTING : LightingType.NONE;

        public override bool NeedsUpdate => true;
        public HikipperButton(string name, Point location, ButtonType type) : base(name, location, type, "") {
        }

        public HikipperButton(string name, int x, int y, ButtonType type) : base(name, x, y, type, "") {
        }

        public override void OnClick() {
            if (CancelButton.Active) {
                Active = false;
                CancelButton.MakeInactive();
            }
            else {
                Active = !Active;
            }
        }

        public static void MakeInactive() {
            Active = false;
        }
    }


    public class LeverDirectionPair(string leverName, LCR direction) {
        public string LeverName { get; init; } = leverName;

        public LCR Direction { get; init; } = direction;
    }

    public enum LightingType {
        NONE,
        BLINKING_SLOW,
        BLINKING_FAST,
        LIGHTING
    }
}
