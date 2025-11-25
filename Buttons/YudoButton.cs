using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TatehamaCTCPClient.Models;

namespace TatehamaCTCPClient.Buttons {
    public class YudoButton : CTCPButton {

        private List<SelectionButton> targets = [];

        public ReadOnlyCollection<SelectionButton> Targets { get; init; }

        public YudoButton(string name, Point location, ButtonType type, List<SelectionButton> targets) : base(name, location, type, "") {
            this.targets.AddRange(targets);
            Targets = this.targets.AsReadOnly();
        }

        public YudoButton(string name, int x, int y, ButtonType type, List<SelectionButton> targets) : this(name, new(x, y), type, targets) {
        }

        public override void OnClick() {

        }
    }
}
