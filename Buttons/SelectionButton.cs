using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TatehamaCTCPClient.Models;

namespace TatehamaCTCPClient.Buttons
{
    public class SelectionButton : CTCPButton {

        private readonly Dictionary<DestinationButton, List<LeverDirectionPair>> routes = [];

        public ReadOnlyDictionary<DestinationButton, List<LeverDirectionPair>> Routes { get; init; }

        public SelectionButton(string name, Point location, ButtonType type, string label, DestinationButton destination, string leverName, LCR direction) : base(name, location, type, label) {
            var l = new List<LeverDirectionPair> {
                new(leverName, direction)
            };
            routes.Add(destination, l);
            Routes = routes.AsReadOnly();
        }

        public SelectionButton(string name, int x, int y, ButtonType type, string label, DestinationButton destination, string leverName, LCR direction) : this(name, new(x, y), type, label, destination, leverName, direction) { }

        public void AddRoute(DestinationButton destination, string leverName, LCR direction) {
            if (routes.TryGetValue(destination, out var l)) {
                l.Add(new LeverDirectionPair(leverName, direction));
            }
            else {
                l = [new(leverName, direction)];
                routes.Add(destination, l);
            }
        }

        public override void OnClick() {

        }
    }
}
