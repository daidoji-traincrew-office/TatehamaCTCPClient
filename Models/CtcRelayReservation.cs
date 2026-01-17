using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaCTCPClient.Models {
    public class CtcRelayReservation(string tcName, RaiseDrop raiseDrop) {
        public string TcName { get; init; } = tcName;
        public RaiseDrop RaiseDrop { get; init; } = raiseDrop;
    }
}
