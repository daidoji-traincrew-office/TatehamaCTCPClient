using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaCTCPClient.Models {
    public class RouteData {
        public string TcName { get; set; }
        public RouteType RouteType { get; set; }
        public ulong? RootId { get; set; }
        public RouteData? Root { get; set; }
        public string? Indicator { get; set; }
        public int? ApproachLockTime { get; set; }
        public RouteStateData? RouteState { get; set; }
    }

    public class RouteStateData {
        /// <summary>
        /// てこ反応リレー
        /// </summary>
        public RaiseDrop IsLeverRelayRaised { get; set; }

        /// <summary>
        /// 進路照査リレー
        /// </summary>
        public RaiseDrop IsRouteRelayRaised { get; set; }

        /// <summary>
        /// 信号制御リレー
        /// </summary>
        public RaiseDrop IsSignalControlRaised { get; set; }

        /// <summary>
        /// 接近鎖錠リレー(MR)
        /// </summary>
        public RaiseDrop IsApproachLockMRRaised { get; set; }

        /// <summary>
        /// 接近鎖錠リレー(MS)
        /// </summary>
        public RaiseDrop IsApproachLockMSRaised { get; set; }

        /// <summary>
        /// 進路鎖錠リレー(実在しない)
        /// </summary>
        public RaiseDrop IsRouteLockRaised { get; set; }

        /// <summary>
        /// 総括反応リレー
        /// </summary>
        public RaiseDrop IsThrowOutXRRelayRaised { get; set; }

        /// <summary>
        /// 総括反応中継リレー
        /// </summary>
        public RaiseDrop IsThrowOutYSRelayRaised { get; set; }

        /// <summary>
        /// 転てつ器を除いた進路照査リレー
        /// </summary>
        public RaiseDrop IsRouteRelayWithoutSwitchingMachineRaised { get; set; }

        /// <summary>
        /// xリレー
        /// </summary>
        public RaiseDrop IsThrowOutXRelayRaised { get; set; }

        /// <summary>
        /// Sリレー
        /// </summary>
        public RaiseDrop IsThrowOutSRelayRaised { get; set; }

        /// <summary>
        /// CTCリレー
        /// </summary>
        public RaiseDrop IsCtcRelayRaised { get; set; }
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
