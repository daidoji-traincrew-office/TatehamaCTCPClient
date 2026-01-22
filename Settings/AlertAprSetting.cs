using System.Collections.ObjectModel;

namespace TatehamaCTCPClient.Settings {
    public class AlertAprSetting {
        public StationSetting Station { get; init; }
        public string RouteGroup { get; init; }
        public TrainDirection Direction { get; init; }
        

        private readonly List<AlertAprSTPair> settings = [];

        public ReadOnlyCollection<AlertAprSTPair> Settings { get; init; }

        public AlertAprSetting(StationSetting station, string routeGroup, TrainDirection direction, bool nearest, List<string> signals, string raisingTrackCircuit, List<string> dropingTrackCircuits) {
            Station = station;
            RouteGroup = routeGroup;
            Direction = direction;
            settings.Add(new AlertAprSTPair(nearest, signals, raisingTrackCircuit, dropingTrackCircuits));
            Settings = settings.AsReadOnly();
        }

        public void AddSetting(bool nearest, List<string> signals, string raisingTrackCircuit, List<string> dropingTrackCircuits) {
            settings.Add(new AlertAprSTPair(nearest, signals, raisingTrackCircuit, dropingTrackCircuits));
        }

        public class AlertAprSTPair {
            public bool Nearest { get; }

            private readonly List<string> signals;

            public string RaisingTrackCircuit { get; init; }

            private readonly List<string> dropingTrackCircuits;

            public ReadOnlyCollection<string> Signals { get; init; }
            public ReadOnlyCollection<string> DropingTrackCircuits { get; init; }

            public AlertAprSTPair(bool nearest, List<string> signals, string raisingTrackCircuit, List<string> dropingTrackCircuits) {
                Nearest = nearest;
                this.signals = new List<string>(signals);
                this.dropingTrackCircuits = dropingTrackCircuits;
                Signals = this.signals.AsReadOnly();
                RaisingTrackCircuit = raisingTrackCircuit;
                DropingTrackCircuits = this.dropingTrackCircuits.AsReadOnly();
            }
        }
    }

    public enum TrainDirection {
        UP,
        DOWN
    }
}
