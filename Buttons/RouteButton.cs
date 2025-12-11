using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TatehamaCTCPClient.Communications;
using TatehamaCTCPClient.Models;
using TatehamaCTCPClient.Settings;

namespace TatehamaCTCPClient.Buttons
{
    public class RouteButton : CTCPButton {

        public string Route { get; private set; }

        public StationSetting Station { get; init; }

        public override LightingType Lighting {
            get {
                if(Route.Length <= 0) {
                    return LightingType.NONE;
                }
                var r = new List<RouteData>(DataToCTCP.Latest.RouteDatas).FirstOrDefault(r => r.TcName == Route);
                if (r == null || r.RouteState == null) {
                    return LightingType.NONE;
                }
                var blinking = r.RouteState.IsCtcRelayRaised == RaiseDrop.Raise;
                var lighting = r.RouteState.IsSignalControlRaised == RaiseDrop.Raise;
                return lighting ? LightingType.LIGHTING : (blinking ? LightingType.BLINKING_FAST : LightingType.NONE);

            }
        }

        public override bool Enabled => Route.Length > 0;

        public RouteButton(string name, Point location, ButtonType type, string label, StationSetting station, string routeName = "") : base(name, location, type, label) {
            Route = routeName;
            Station = station;
        }

        public RouteButton(string name, int x, int y, ButtonType type, string label, StationSetting station, string routeName = "") : this(name, new(x, y), type, label, station, routeName) { }

        public void AddRoute(string routeName) {
            if(Route.Length <= 0) {
                Route = routeName;
            }
        }

        public override void OnClick() {
            var c = ServerCommunication.Instance;
            if (c == null) {
                return;
            }
            if (CancelButton.Active) {
                if(Lighting == LightingType.NONE) {
                    return;
                }
                _ = c.SetCtcRelay(Route, RaiseDrop.Drop);
                CancelButton.MakeInactive();
            }
            else {
                if (!Station.Active || !DataToCTCP.Latest.CenterControlStates.TryGetValue(Station.LeverName, out var state) || state == CenterControlState.StationControl) {
                    return;
                }
                if (Lighting != LightingType.NONE) {
                    return;
                }
                _ = c.SetCtcRelay(Route, RaiseDrop.Raise);
            }
        }
    }
}
