using System.Collections.ObjectModel;
using System.Diagnostics;
using TatehamaCTCPClient.Communications;
using TatehamaCTCPClient.Forms;
using TatehamaCTCPClient.Manager;
using TatehamaCTCPClient.Models;
using TatehamaCTCPClient.Settings;

namespace TatehamaCTCPClient.Buttons
{
    public class DestinationButton : CTCPButton {

        private readonly List<Route> routes = [];

        public ReadOnlyCollection<Route> Routes { get; init; }

        public StationSetting Station { get; init; }

        public SelectionButton? CurrentRoute { get; private set; } = null;

        public bool IsWaiting => CurrentRoute != null;

        /*public override LightingType Lighting {
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
                    var r = new List<RouteData>(DataToCTCP.Latest.RouteDatas).FirstOrDefault(r => r.TcName == route.RouteName);
                    if (r == null || r.RouteState == null) {
                        continue;
                    }
                    blinking |= r.RouteState.IsCtcRelayRaised == RaiseDrop.Raise;
                    lighting |= r.RouteState.IsSignalControlRaised == RaiseDrop.Raise;
                }
                return lighting ? LightingType.LIGHTING : (blinking ? LightingType.BLINKING_FAST : LightingType.NONE);
            }
        }*/

        public DestinationButton(string name, Point location, ButtonType type, string label, StationSetting station, Route route) : this(name, location, type, label, station) {
            routes.Add(route);
        }

        public DestinationButton(string name, int x, int y, ButtonType type, string label, StationSetting station, Route route) : this(name, new(x, y), type, label, station, route) { }

        public DestinationButton(string name, Point location, ButtonType type, string label, StationSetting station) : base(name, location, type, label) {
            Station = station;
            Routes = routes.AsReadOnly();
        }

        public DestinationButton(string name, int x, int y, ButtonType type, string label, StationSetting station) : this(name, new(x, y), type, label, station) { }

        public void AddRoute(Route route) {
            routes.Add(route);
        }

        public override bool OnClick() {
            if (CancelButton.Active) {
                if (!IsWaiting) {
                    var c = ServerCommunication.Instance;
                    if (c == null) {
                        return false;
                    }
                    foreach (var route in routes) {
                        route.SetHikipper(false);
                        _ = c.SetCtcRelay(route.RouteName, RaiseDrop.Drop);
                        Debug.WriteLine($"{DateTime.UtcNow.AddHours(9)} {route.RouteName} を取消しました");
                        NotificationManager.AddNotification($"{route.RouteName} を取消しました", false);
                        NavigationWindow.Instance?.UpdateNotification();
                    }
                    CancelButton.MakeInactive();
                    HikipperButton.MakeInactive();
                }
            }
            else if(IsWaiting) {
                if (!Station.Active || !DataToCTCP.Latest.CenterControlStates.TryGetValue(Station.LeverName, out var state) || state == CenterControlState.StationControl) {
                    return false;
                }
                CurrentRoute?.SetRoute(this);
                CurrentRoute = null;
            }
            return true;

        }

        public bool SetCurrentRoute(SelectionButton b) {
            if (CurrentRoute != null || Lighting != LightingType.NONE) {
                return false;
            }
            CurrentRoute = b;
            return true;
        }


        public void Cancel(SelectionButton b) {
            if(CurrentRoute == b) {
                Debug.WriteLine($"cancel {Name}");
                CurrentRoute = null;
            }
        }

        protected override LightingType CalculationLighting() {
            if (IsWaiting) {
                return LightingType.BLINKING_SLOW;
            }


            if (routes.Count <= 0) {
                return LightingType.NONE;
            }
            var blinking = false;
            var lighting = false;
            foreach (var route in Routes) {
                var r = new List<RouteData>(DataToCTCP.Latest.RouteDatas).FirstOrDefault(r => r.TcName == route.RouteName);
                if (r == null || r.RouteState == null) {
                    continue;
                }
                blinking |= r.RouteState.IsCtcRelayRaised == RaiseDrop.Raise;
                lighting |= r.RouteState.IsSignalControlRaised == RaiseDrop.Raise;
            }
            return lighting ? LightingType.LIGHTING : (blinking ? LightingType.BLINKING_FAST : LightingType.NONE);
        }
    }
}
