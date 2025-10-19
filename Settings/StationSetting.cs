using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaCTCPClient.Settings {
    public class StationSetting(string code, string name, string leverName, Point location) {
        public string Code { get; init; } = code;
        public string Number => Code.Replace("S", "");

        public string Name { get; init; } = name;

        public string LeverName { get; init; } = leverName;

        public Point Location { get; init; } = location;
    }
}
