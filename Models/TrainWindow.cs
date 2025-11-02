using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaCTCPClient.Models {
    public class TrainWindow {
        public string Name { get; init; }

        public Point Location { get; init; }

        private List<string> ttcNames = [];

        public ReadOnlyCollection<string> TtcNames { get; init; }

        public TrainWindow(string name, Point location, string ttcName) {
            Name = name;
            Location = location;
            ttcNames.Add(ttcName);
            TtcNames = ttcNames.AsReadOnly();
        }

        public TrainWindow(string name, int x, int y, string ttcName) : this(name, new(x, y), ttcName) {
        }

        public void AddTtcName(string ttcName) {
            ttcNames.Add(ttcName);
        }

    }
}
