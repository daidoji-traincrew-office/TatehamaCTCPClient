using TatehamaCTCPClient.Settings;

namespace TatehamaCTCPClient.Models {
    public class Route (string routeName, string trackName, StationSetting station) {
        public string RouteName { get; init; } = routeName;

        public string TrackName { get; init; } = trackName;

        public StationSetting Station {  get; init; } = station;

        public bool IsHikipper { get; private set; } = false;

        public void SetHikipper(bool isHikipper) {
            IsHikipper = isHikipper;
        }

    }
}
