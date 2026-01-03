using System.Diagnostics;
using TatehamaCTCPClient.Communications;
using TatehamaCTCPClient.Models;
using TatehamaCTCPClient.Settings;

namespace TatehamaCTCPClient.Buttons
{
    public class RouteButton : CTCPButton {

        public Route? Route { get; private set; }

        public StationSetting Station { get; init; }

        /*public override LightingType Lighting {
            get {
                if(Route == null) {
                    return LightingType.NONE;
                }
                var r = new List<RouteData>(DataToCTCP.Latest.RouteDatas).FirstOrDefault(r => r.TcName == Route.RouteName);
                if (r == null || r.RouteState == null) {
                    return LightingType.NONE;
                }
                var blinking = r.RouteState.IsCtcRelayRaised == RaiseDrop.Raise;
                var lighting = r.RouteState.IsSignalControlRaised == RaiseDrop.Raise;
                return lighting ? LightingType.LIGHTING : (blinking ? LightingType.BLINKING_FAST : LightingType.NONE);

            }
        }*/

        public override bool Enabled => Route != null;

        public RouteButton(string name, Point location, ButtonType type, string label, StationSetting station, Route? route = null) : base(name, location, type, label) {
            Route = route;
            Station = station;
        }

        public RouteButton(string name, int x, int y, ButtonType type, string label, StationSetting station, Route? route = null) : this(name, new(x, y), type, label, station, route) { }

        public void AddRoute(Route route) {
            Route ??= route;
        }

        public override bool OnClick() {
            var c = ServerCommunication.Instance;
            if (c == null) {
                return false;
            }
            if (CancelButton.Active) {
                if(Route == null || Lighting == LightingType.NONE) {
                    return false;
                }
                Route.SetHikipper(false);
                _ = c.SetCtcRelay(Route.RouteName, RaiseDrop.Drop);
                CancelButton.MakeInactive();
                HikipperButton.MakeInactive();
            }
            else {
                if (!Station.Active || !DataToCTCP.Latest.CenterControlStates.TryGetValue(Station.LeverName, out var state) || state == CenterControlState.StationControl) {
                    return false;
                }
                if (Route == null || Lighting != LightingType.NONE) {
                    return false;
                }
                if (!Route.IsHikipper) {
                    Route.SetHikipper(HikipperButton.Active);
                }
                HikipperButton.MakeInactive();
                _ = c.SetCtcRelay(Route.RouteName, RaiseDrop.Raise);
            }
            return true;
        }

        protected override LightingType CalculationLighting() {
            if (Route == null) {
                return LightingType.NONE;
            }
            var r = new List<RouteData>(DataToCTCP.Latest.RouteDatas).FirstOrDefault(r => r.TcName == Route.RouteName);
            if (r == null || r.RouteState == null) {
                return LightingType.NONE;
            }
            var blinking = r.RouteState.IsCtcRelayRaised == RaiseDrop.Raise;
            var lighting = r.RouteState.IsSignalControlRaised == RaiseDrop.Raise;
            return lighting ? LightingType.LIGHTING : (blinking ? LightingType.BLINKING_FAST : LightingType.NONE);
        }
    }
}
