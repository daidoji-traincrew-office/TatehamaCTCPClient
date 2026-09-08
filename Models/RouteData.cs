
namespace TatehamaCTCPClient.Models {
    public class RouteData {
        public string TcName { get; set; } = string.Empty;
        public RouteType RouteType { get; set; }
        public ulong? RootId { get; set; }
        public RouteData? Root { get; set; }
        public string? Indicator { get; set; }
        public int? ApproachLockTime { get; set; }
        public RouteStateData? RouteState { get; set; }
    }

    public enum RaiseDrop {
        Drop,
        Raise
    }

    public enum RouteType {
        Arriving,       // 場内
        Departure,      // 出発
        Guide,          // 誘導
        SwitchSignal,   // 入換信号
        SwitchRoute     // 入換標識
    }
}
