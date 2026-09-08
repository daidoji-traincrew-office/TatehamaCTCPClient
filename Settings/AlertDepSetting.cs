

namespace TatehamaCTCPClient.Settings {
    public class AlertDepSetting(string trackName, StationSetting station, string posName, string routeGroup) {
        public string TrackName { get; init; } = trackName;

        public StationSetting Station { get; init; } = station;

        public string PosName { get; init; } = posName;

        public string RouteGroup { get; init; } = routeGroup;
    }
}
