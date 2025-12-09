using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TatehamaCTCPClient.Communications;
using TatehamaCTCPClient.Models;
using TatehamaCTCPClient.Settings;

namespace TatehamaCTCPClient.Buttons
{
    public class SelectionButton : CTCPButton {

        private readonly Dictionary<DestinationButton, List<string>> routes = [];

        public ReadOnlyDictionary<DestinationButton, List<string>> Routes { get; init; }

        public StationSetting Station { get; init; }

        public bool IsWaiting { get; private set; } = false;

        /*public DestinationButton? CurrentRoute { get; private set; } = null;*/

        public override LightingType Lighting {
            get {

                if (routes.Count <= 0) {
                    return LightingType.NONE;
                }
                if (IsWaiting) {
                    return LightingType.BLINKING_SLOW;
                }
                /*var cr = CurrentRoute;
                if (cr == null) { 
                    return LightingType.NONE;
                }*/
                
                var blinking = false;
                var lighting = false;
                foreach (var route in Routes.Values) {
                    var tcName = route.FirstOrDefault();
                    if (tcName == null) {
                        continue;
                    }
                    var r = new List<RouteData>(DataToCTCP.Latest.RouteDatas).FirstOrDefault(r => r.TcName == tcName);
                    if (r == null || r.RouteState == null) {
                        return LightingType.NONE;
                    }
                    var b = r.RouteState.IsCtcRelayRaised == RaiseDrop.Raise;
                    blinking |= b;
                    lighting |= b && r.RouteState.IsSignalControlRaised == RaiseDrop.Raise;
                }
                return blinking ? (lighting ? LightingType.LIGHTING : LightingType.BLINKING_FAST) : LightingType.NONE;

            }
        }

        public override bool Enabled => routes.Count > 0;

        public SelectionButton(string name, Point location, ButtonType type, string label, StationSetting station, DestinationButton destination, string leverName) : this(name, location, type, label, station) {
            var l = new List<string> {
                leverName
            };
            routes.Add(destination, l);
        }

        public SelectionButton(string name, int x, int y, ButtonType type, string label, StationSetting station, DestinationButton destination, string leverName) : this(name, new(x, y), type, label, station, destination, leverName) { }

        public SelectionButton(string name, Point location, ButtonType type, string label, StationSetting station) : base(name, location, type, label) {
            Routes = routes.AsReadOnly();
            Station = station;
        }

        public SelectionButton(string name, int x, int y, ButtonType type, string label, StationSetting station) : this(name, new(x, y), type, label, station) { }

        public void AddRoute(DestinationButton destination, string leverName) {
            if (routes.TryGetValue(destination, out var l)) {
                l.Add(leverName);
            }
            else {
                l = [leverName];
                routes.Add(destination, l);
            }
        }

        public override void OnClick() {

            if (CancelButton.Active) {
                if (IsWaiting) {
                    IsWaiting = false;
                    foreach (var k in routes.Keys) {
                        k.Cancel();
                    }
                }
                else if (Lighting != LightingType.NONE) {
                    CancelRoute();
                }
                else {
                    return;
                }

                CancelButton.MakeInactive();
            }
            else {
                if (!Station.Active || !DataToCTCP.Latest.CenterControlStates.TryGetValue(Station.LeverName, out var state) || state == CenterControlState.StationControl) {
                    return;
                }
                if (routes.Count <= 0) {
                    return;
                }
                var c = 0;
                foreach (var k in routes.Keys) {
                    if (k.SetCurrentRoute(this)) {
                        Debug.WriteLine($"{Name} {k.Name}");
                        c++;
                    }
                }
                if (c > 0)  {
                    IsWaiting = true;
                }
            }
        }

        public void SetRoute(DestinationButton db) {
            if(!IsWaiting || !routes.ContainsKey(db)) {
                return;
            }
            IsWaiting = false;

            var c = ServerCommunication.Instance;
            if (c == null) {
                return;
            }
            foreach (var k in routes.Keys) { 
                if(k != db) {
                    k.Cancel();
                }
            }

            foreach (var route in Routes[db]) {

                _ = c.SetCtcRelay(route, RaiseDrop.Raise);
            }
        }

        public void CancelRoute() {
            var c = ServerCommunication.Instance;
            if (c == null) {
                return;
            }
            foreach (var route in Routes.Values) {
                var r = route.FirstOrDefault();
                if(r == null) {
                    continue;
                }
                _ = c.SetCtcRelay(r, RaiseDrop.Drop);
            }
        }
    }
}
