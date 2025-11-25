using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TatehamaCTCPClient.Models;

namespace TatehamaCTCPClient.Buttons
{
    public class RouteButton : CTCPButton {

        private readonly List<LeverDirectionPair> routes = [];

        public ReadOnlyCollection<LeverDirectionPair> Routes { get; init; }

        public RouteButton(string name, Point location, ButtonType type, string label, string leverName, LCR direction) : base(name, location, type, label) {
            routes.Add(new(leverName, direction));
            Routes = routes.AsReadOnly();
        }

        public RouteButton(string name, int x, int y, ButtonType type, string label, string leverName, LCR direction) : this(name, new(x, y), type, label, leverName, direction) { }

        public void AddRoute(string leverName, LCR direction) {
            routes.Add(new LeverDirectionPair(leverName, direction));
        }

        public override void OnClick() {

        }
    }
}
