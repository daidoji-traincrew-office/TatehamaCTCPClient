using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml.Linq;
using TatehamaCTCPClient.Communications;
using TatehamaCTCPClient.Models;
using TatehamaCTCPClient.Settings;

namespace TatehamaCTCPClient.Buttons
{
    public class SelectionButton : CTCPButton {

        private readonly Dictionary<DestinationButton, List<string>> routes = [];

        public ReadOnlyDictionary<DestinationButton, List<string>> Routes { get; init; }

        private readonly Dictionary<DestinationButton, List<string>> yudoRoutes = [];

        public ReadOnlyDictionary<DestinationButton, List<string>> YudoRoutes { get; init; }

        public StationSetting Station { get; init; }

        public bool IsWaiting { get; private set; } = false;

        public bool IsYudo { get; private set; } = false;

        /*public DestinationButton? CurrentRoute { get; private set; } = null;*/

        public override LightingType Lighting {
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
                foreach (var route in d.Values) {
                    var tcName = route.FirstOrDefault();
                    if (tcName == null) {
                        continue;
                    }
                    var r = new List<RouteData>(DataToCTCP.Latest.RouteDatas).FirstOrDefault(r => r.TcName == tcName);
                    if (r == null || r.RouteState == null) {
                        return LightingType.NONE;
                    }
                    blinking |= r.RouteState.IsCtcRelayRaised == RaiseDrop.Raise;
                    lighting |= r.RouteState.IsSignalControlRaised == RaiseDrop.Raise;
                }
                return blinking ? (lighting ? LightingType.LIGHTING : LightingType.BLINKING_FAST) : LightingType.NONE;

            }
        }

        public override bool Enabled => routes.Count > 0;

        public SelectionButton(string name, Point location, ButtonType type, string label, StationSetting station, DestinationButton destination, string routeName, string yudoRouteName = "") : this(name, location, type, label, station) {
            var l = new List<string> {
                routeName
            };
            routes.Add(destination, l);
            if(yudoRouteName.Length <= 0) {
                return;
            }
            var y = new List<string> {
                yudoRouteName
            };
            yudoRoutes.Add(destination, y);
        }

        public SelectionButton(string name, int x, int y, ButtonType type, string label, StationSetting station, DestinationButton destination, string routeName, string yudoRouteName = "") : this(name, new(x, y), type, label, station, destination, routeName, yudoRouteName) { }

        public SelectionButton(string name, Point location, ButtonType type, string label, StationSetting station) : base(name, location, type, label) {
            Routes = routes.AsReadOnly();
            YudoRoutes = yudoRoutes.AsReadOnly();
            Station = station;
        }

        public SelectionButton(string name, int x, int y, ButtonType type, string label, StationSetting station) : this(name, new(x, y), type, label, station) { }

        public void AddRoute(DestinationButton destination, string routeName, string yudoRouteName = "") {
            if (routes.TryGetValue(destination, out var l)) {
                l.Add(routeName);
            }
            else {
                l = [routeName];
                routes.Add(destination, l);
            }
            if (yudoRouteName.Length <= 0) {
                return;
            }
            if (yudoRoutes.TryGetValue(destination, out var y)) {
                y.Add(yudoRouteName);
            }
            else {
                y = [yudoRouteName];
                yudoRoutes.Add(destination, y);
            }
        }

        public override void OnClick() {

            if (CancelButton.Active) {
                if (!CancelWaiting()) {
                    if(Lighting == LightingType.NONE) {
                        return;
                    }
                    CancelRoute();
                }

                CancelButton.MakeInactive();
            }
            else {
                if (!Station.Active || !DataToCTCP.Latest.CenterControlStates.TryGetValue(Station.LeverName, out var state) || state == CenterControlState.StationControl) {
                    return;
                }
                var d = IsYudo ? yudoRoutes : routes;
                if (d.Count <= 0) {
                    return;
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
                        _ = c.SetCtcRelay(r, RaiseDrop.Raise);
                    }
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
                _ = c.SetCtcRelay(r, RaiseDrop.Drop);
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
    }
}
