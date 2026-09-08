
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
