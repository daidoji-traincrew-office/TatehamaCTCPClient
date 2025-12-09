using System.Collections.ObjectModel;
using System.Diagnostics;
using TatehamaCTCPClient.Communications;
using TatehamaCTCPClient.Models;
using TatehamaCTCPClient.Settings;

namespace TatehamaCTCPClient.Buttons
{
    public class DestinationButton : CTCPButton {

        private readonly List<string> routes = [];

        public ReadOnlyCollection<string> Routes { get; init; }

        public StationSetting Station { get; init; }

        public SelectionButton? CurrentRoute { get; private set; } = null;

        public bool IsWaiting => CurrentRoute != null;

        public override LightingType Lighting {
            get {
                if (IsWaiting) {
                    return LightingType.BLINKING_SLOW;
                }


                if (routes.Count <= 0) {
                    return LightingType.NONE;
                }
                var blinking = false;
                var lighting = false;
                foreach (var route in Routes) {
                    var r = new List<RouteData>(DataToCTCP.Latest.RouteDatas).FirstOrDefault(r => r.TcName == route);
                    if (r == null || r.RouteState == null) {
                        continue;
                    }
                    var b = r.RouteState.IsCtcRelayRaised == RaiseDrop.Raise;
                    blinking |= b;
                    lighting |= b && r.RouteState.IsSignalControlRaised == RaiseDrop.Raise;
                }
                return blinking ? (lighting ? LightingType.LIGHTING : LightingType.BLINKING_FAST) : LightingType.NONE;
            }
        }

        public DestinationButton(string name, Point location, ButtonType type, string label, StationSetting station, string leverName) : this(name, location, type, label, station) {
            routes.Add(leverName);
        }

        public DestinationButton(string name, int x, int y, ButtonType type, string label, StationSetting station, string leverName) : this(name, new(x, y), type, label, station, leverName) { }

        public DestinationButton(string name, Point location, ButtonType type, string label, StationSetting station) : base(name, location, type, label) {
            Station = station;
            Routes = routes.AsReadOnly();
        }

        public DestinationButton(string name, int x, int y, ButtonType type, string label, StationSetting station) : this(name, new(x, y), type, label, station) { }

        public void AddRoute(string leverName) {
            routes.Add(leverName);
        }

        public override void OnClick() {
            if (CancelButton.Active) {
                if (!IsWaiting) {
                    /*CurrentRoute.CancelRoute();*/
                    var c = ServerCommunication.Instance;
                    if (c == null) {
                        return;
                    }
                    Debug.WriteLine("cancel2");
                    foreach (var route in routes) {
                        _ = c.SetCtcRelay(route, RaiseDrop.Drop);
                    }
                    CancelButton.MakeInactive();
                }
            }
            else if(IsWaiting) {
                if (!Station.Active || !DataToCTCP.Latest.CenterControlStates.TryGetValue(Station.LeverName, out var state) || state == CenterControlState.StationControl) {
                    return;
                }
                CurrentRoute?.SetRoute(this);
                CurrentRoute = null;
            }

        }

        public bool SetCurrentRoute(SelectionButton b) {
            if (CurrentRoute != null || Lighting != LightingType.NONE) {
                return false;
            }
            CurrentRoute = b;
            return true;
        }


        public void Cancel() {
            Debug.WriteLine($"cancel {Name}");
            CurrentRoute = null;
        }
    }
}
