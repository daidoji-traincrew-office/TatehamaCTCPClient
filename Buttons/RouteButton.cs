using System.Collections.ObjectModel;
using TatehamaCTCPClient.Models;

namespace TatehamaCTCPClient.Buttons
{
    public class RouteButton : CTCPButton {

        private readonly List<LeverDirectionPair> routes = [];

        public ReadOnlyCollection<LeverDirectionPair> Routes { get; init; }

        public override bool Enabled => routes.Count > 0;

        public RouteButton(string name, Point location, ButtonType type, string label, string leverName, LCR direction) : this(name, location, type, label) {
            routes.Add(new(leverName, direction));
        }

        public RouteButton(string name, int x, int y, ButtonType type, string label, string leverName, LCR direction) : this(name, new(x, y), type, label, leverName, direction) { }

        public RouteButton(string name, Point location, ButtonType type, string label) : base(name, location, type, label) {
            Routes = routes.AsReadOnly();
        }

        public RouteButton(string name, int x, int y, ButtonType type, string label) : this(name, new(x, y), type, label) { }

        public void AddRoute(string leverName, LCR direction) {
            routes.Add(new LeverDirectionPair(leverName, direction));
        }

        public override void OnClick() {

        }
    }
}
