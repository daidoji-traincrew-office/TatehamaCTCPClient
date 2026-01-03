using System.Collections.ObjectModel;
using System.Diagnostics;
using TatehamaCTCPClient.Communications;
using TatehamaCTCPClient.Models;
using TatehamaCTCPClient.Settings;

namespace TatehamaCTCPClient.Buttons
{
    public class SelectionButton : CTCPButton {

        private readonly Dictionary<DestinationButton, List<Route>> routes = [];

        public ReadOnlyDictionary<DestinationButton, List<Route>> Routes { get; init; }

        private readonly Dictionary<DestinationButton, List<Route>> yudoRoutes = [];

        public ReadOnlyDictionary<DestinationButton, List<Route>> YudoRoutes { get; init; }

        public StationSetting Station { get; init; }

        public bool IsWaiting { get; private set; } = false;

        public bool IsYudo { get; private set; } = false;

        /*public override LightingType Lighting {
            get {
                var d = IsYudo ? yudoRoutes : routes;
                if (d.Count <= 0) {
                    return LightingType.NONE;
                }
                if (IsWaiting) {
                    return LightingType.BLINKING_SLOW;
                }
                
                var blinking = false;
                var lighting = false;
                foreach (var routeList in d.Values) {
                    var route = routeList.FirstOrDefault();
                    if (route == null) {
                        continue;
                    }
                    var r = new List<RouteData>(DataToCTCP.Latest.RouteDatas).FirstOrDefault(r => r.TcName == route.RouteName);
                    if (r == null || r.RouteState == null) {
                        return LightingType.NONE;
                    }
                    blinking |= r.RouteState.IsCtcRelayRaised == RaiseDrop.Raise;
                    lighting |= r.RouteState.IsSignalControlRaised == RaiseDrop.Raise;
                }
                return lighting ? LightingType.LIGHTING : (blinking ? LightingType.BLINKING_FAST : LightingType.NONE);

            }
        }*/

        public override bool Enabled => routes.Count > 0;

        public SelectionButton(string name, Point location, ButtonType type, string label, StationSetting station, DestinationButton destination, Route route, Route? yudoRoute = null) : this(name, location, type, label, station) {
            var l = new List<Route> {
                route
            };
            routes.Add(destination, l);
            if(yudoRoute == null) {
                return;
            }
            var y = new List<Route> {
                yudoRoute
            };
            yudoRoutes.Add(destination, y);
        }

        public SelectionButton(string name, int x, int y, ButtonType type, string label, StationSetting station, DestinationButton destination, Route route, Route? yudoRoute = null) : this(name, new(x, y), type, label, station, destination, route, yudoRoute) { }

        public SelectionButton(string name, Point location, ButtonType type, string label, StationSetting station) : base(name, location, type, label) {
            Routes = routes.AsReadOnly();
            YudoRoutes = yudoRoutes.AsReadOnly();
            Station = station;
        }

        public SelectionButton(string name, int x, int y, ButtonType type, string label, StationSetting station) : this(name, new(x, y), type, label, station) { }

        public void AddRoute(DestinationButton destination, Route route, Route? yudoRoute = null) {
            if (routes.TryGetValue(destination, out var l)) {
                l.Add(route);
            }
            else {
                l = [route];
                routes.Add(destination, l);
            }
            if (yudoRoute == null) {
                return;
            }
            if (yudoRoutes.TryGetValue(destination, out var y)) {
                y.Add(yudoRoute);
            }
            else {
                y = [yudoRoute];
                yudoRoutes.Add(destination, y);
            }
        }

        public override bool OnClick() {

            if (CancelButton.Active) {
                if (!CancelWaiting()) {
                    if(Lighting == LightingType.NONE) {
                        return false;
                    }
                    CancelRoute();
                }

                CancelButton.MakeInactive();
                HikipperButton.MakeInactive();
            }
            else {
                if (!Station.Active || !DataToCTCP.Latest.CenterControlStates.TryGetValue(Station.LeverName, out var state) || state == CenterControlState.StationControl) {
                    return false;
                }
                var d = IsYudo ? yudoRoutes : routes;
                if (d.Count <= 0) {
                    return false;
                }
                var c = 0;
                foreach (var k in d.Keys) {
                    Debug.WriteLine($"{Name} {k.Name}");
                    if (k.SetCurrentRoute(this)) {
                        Debug.WriteLine($"{Name} {k.Name}");
                        c++;
                    }
                }
                if (c > 0)  {
                    IsWaiting = true;
                }
            }
            return true;
        }

        public void SetRoute(DestinationButton db) {
            var d = IsYudo ? yudoRoutes : routes;
            if (!IsWaiting || !d.ContainsKey(db)) {
                return;
            }
            IsWaiting = false;

            var c = ServerCommunication.Instance;
            if (c == null) {
                return;
            }
            foreach (var k in d.Keys) { 
                if(k == db) {
                    foreach (var route in d[db]) {
                        var r = route;
                        if (!r.IsHikipper) {
                            r.SetHikipper(HikipperButton.Active);
                        }
                        _ = c.SetCtcRelay(r.RouteName, RaiseDrop.Raise);
                    }
                    HikipperButton.MakeInactive();
                }
                else {
                    k.Cancel(this);
                }
            }
        }

        public void CancelRoute() {
            var c = ServerCommunication.Instance;
            if (c == null) {
                return;
            }
            foreach (var route in (IsYudo ? yudoRoutes : routes).Values) {
                var r = route.FirstOrDefault();
                if(r == null) {
                    continue;
                }
                r.SetHikipper(false);
                _ = c.SetCtcRelay(r.RouteName, RaiseDrop.Drop);
            }
        }

        public bool CancelWaiting() {
            if (IsWaiting) {
                IsWaiting = false;
                foreach (var k in (IsYudo ? yudoRoutes : routes).Keys) {
                    k.Cancel(this);
                }
                return true;
            }
            return false;
        }

        public void SwitchYudo(bool isYudo) {
            if(isYudo != IsYudo) {
                CancelWaiting();
                IsYudo = isYudo;
            }
        }

        protected override LightingType CalculationLighting() {
            var d = IsYudo ? yudoRoutes : routes;
            if (d.Count <= 0) {
                return LightingType.NONE;
            }
            if (IsWaiting) {
                return LightingType.BLINKING_SLOW;
            }

            var blinking = false;
            var lighting = false;
            foreach (var routeList in d.Values) {
                var route = routeList.FirstOrDefault();
                if (route == null) {
                    continue;
                }
                var r = new List<RouteData>(DataToCTCP.Latest.RouteDatas).FirstOrDefault(r => r.TcName == route.RouteName);
                if (r == null || r.RouteState == null) {
                    return LightingType.NONE;
                }
                blinking |= r.RouteState.IsCtcRelayRaised == RaiseDrop.Raise;
                lighting |= r.RouteState.IsSignalControlRaised == RaiseDrop.Raise;
            }
            return lighting ? LightingType.LIGHTING : (blinking ? LightingType.BLINKING_FAST : LightingType.NONE);
        }
    }
}
