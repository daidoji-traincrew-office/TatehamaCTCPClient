using System.Collections.ObjectModel;
using TatehamaCTCPClient.Models;

namespace TatehamaCTCPClient.Buttons {
    public class YudoButton : CTCPButton {

        public bool Active { get; private set; } = false;

        public override LightingType Lighting => Active ? LightingType.LIGHTING : LightingType.NONE;

        public override bool NeedsUpdate => true;

        private List<SelectionButton> targets = [];

        public ReadOnlyCollection<SelectionButton> Targets { get; init; }

        public YudoButton(string name, Point location, ButtonType type, List<SelectionButton> targets) : base(name, location, type, "") {
            this.targets.AddRange(targets);
            Targets = this.targets.AsReadOnly();
        }

        public YudoButton(string name, int x, int y, ButtonType type, List<SelectionButton> targets) : this(name, new(x, y), type, targets) {
        }

        public override void OnClick() {
            Active = !Active;
        }
    }
}
