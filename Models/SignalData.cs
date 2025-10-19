using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaCTCPClient.Models {

    public class SignalData {
        public string Name { get; init; } = "";
        public Phase phase { get; init; } = Phase.None;
    }

    public enum Phase {
        None,
        R,
        YY,
        Y,
        YG,
        G
    }
}
