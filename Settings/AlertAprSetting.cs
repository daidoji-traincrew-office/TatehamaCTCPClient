using System.Collections.ObjectModel;

namespace TatehamaCTCPClient.Settings {
    public class AlertAprSetting {
        public StationSetting Station { get; init; }
        public string RouteGroup { get; init; }
        public TrainDirection Direction { get; init; }
        /*public bool Nearest { get; init; }
        public List<string> Signals { get; init; }
        public List<string> TrackCircuits { get; init; }*/

        private readonly List<AlertAprSTPair> settings = [];

        public ReadOnlyCollection<AlertAprSTPair> Settings { get; init; }

        public AlertAprSetting(StationSetting station, string routeGroup, TrainDirection direction, bool nearest, List<string> signals, List<string> trackCircuits) {
            Station = station;
            RouteGroup = routeGroup;
            Direction = direction;
            /*Nearest = nearest;
            Signals = new List<string>(signals);
            TrackCircuits = trackCircuits;*/
            settings.Add(new AlertAprSTPair(nearest, signals, trackCircuits));
            Settings = settings.AsReadOnly();
        }

        public void AddSetting(bool nearest, List<string> signals, List<string> trackCircuits) {
            settings.Add(new AlertAprSTPair(nearest, signals, trackCircuits));
        }

        public class AlertAprSTPair {
            public bool Nearest { get; }

            private readonly List<string> signals;

            private readonly List<string> trackCircuits;

            public ReadOnlyCollection<string> Signals { get; init; }
            public ReadOnlyCollection<string> TrackCircuits { get; init; }

            public AlertAprSTPair(bool nearest, List<string> signals, List<string> trackCircuits) {
                Nearest = nearest;
                this.signals = new List<string>(signals);
                this.trackCircuits = trackCircuits;
                Signals = this.signals.AsReadOnly();
                TrackCircuits = this.trackCircuits.AsReadOnly();
            }
        }
    }

    public enum TrainDirection {
        UP,
        DOWN
    }
}
