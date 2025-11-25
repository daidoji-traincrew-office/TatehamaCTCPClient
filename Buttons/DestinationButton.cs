using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TatehamaCTCPClient.Models;

namespace TatehamaCTCPClient.Buttons
{
    public class DestinationButton : CTCPButton {

        private SelectionButton? startingPoint = null;

        public bool IsWaiting => startingPoint != null;

        public DestinationButton(string name, Point location, ButtonType type, string label) : base(name, location, type, label) { }

        public DestinationButton(string name, int x, int y, ButtonType type, string label) : this(name, new(x, y), type, label) { }

        public override void OnClick() {

        }

        public void SetStartingPoint(SelectionButton b) {
            startingPoint = b;
        }

        public void Cancel() {
            startingPoint = null;
        }
    }
}
