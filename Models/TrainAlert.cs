using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TatehamaCTCPClient.Settings;

namespace TatehamaCTCPClient.Models {
    public class TrainAlert (StationSetting station, string text, string trainNumber, string routeGroup) {

        public StationSetting Station { get; init; } = station;

        public string Text { get; init; } = text;

        public string TrainNumber { get; init; } = trainNumber;

        public string RouteGroup { get; init; } = routeGroup;
    }
}
