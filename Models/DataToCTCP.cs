
using System.Diagnostics;
using TatehamaCTCPClient.Manager;
using static System.Windows.Forms.AxHost;

namespace TatehamaCTCPClient.Models {

    /// <summary>
    /// CTCP送信用データクラス 
    /// </summary>
    public class DataToCTCP {

        public static DataToCTCP Latest { get; private set; } = new DataToCTCP();

        public static DataToCTCP Previous { get; private set; } = new DataToCTCP();

        public static void SetLatest(DataToCTCP data) {
            Previous = Latest;
            Latest = data;
        }

        public static List<TrackCircuitData> GetDifferenceTrack() {
            var l = new List<TrackCircuitData>();
            var latest = Latest;
            var previous = Previous;
            foreach(var tl in latest.TrackCircuits) {
                var tp = previous.TrackCircuits.FirstOrDefault(tp => tp.Name == tl.Name);
                if (tp == null || tp.On == false && tl.On == true) {
                    l.Add(tl);
                }
            }
            return l;
        }

        public static bool HasDifference(CTCPManager manager) {
            var latest = Latest;
            var previous = Previous;

            var updated = false;
            var stations = manager.StationSettings;
            foreach (var ccslk in latest.CenterControlStates.Keys) {
                var ccsl = latest.CenterControlStates[ccslk];
                if (!previous.CenterControlStates.TryGetValue(ccslk, out var ccsp) || ccsl != ccsp) {
                    updated = true;
                    var s = stations.FirstOrDefault(s => s.LeverName == ccslk);
                    if(s != null) {
                        NotificationManager.AddNotification($"{s.FullName} が {(ccsl == CenterControlState.StationControl ? "駅扱" : "集中扱")} になりました。", s.Active);
                    }
                }
            }
            if (updated) {
                return true;
            }
            foreach(var nwl in latest.Retsubans) {
                var nwp = previous.Retsubans.FirstOrDefault(nw => nw.Name == nwl.Name);
                if(nwp != null && nwp.Retsuban != nwl.Retsuban) {
                    return true;
                }
            }
            var rdp = new List<RouteData>(previous.RouteDatas);
            foreach (var rl in new List<RouteData>(latest.RouteDatas)) {
                var rp = rdp.FirstOrDefault(r => r.TcName == rl.TcName);
                if (rp == null || rp.RouteState?.IsCtcRelayRaised != rl.RouteState?.IsCtcRelayRaised || rp.RouteState?.IsSignalControlRaised != rl.RouteState?.IsSignalControlRaised) {
                    return true;
                }
            }


            return false;
        }


        /// <summary>
        /// 軌道回路情報リスト
        /// </summary>
        public List<TrackCircuitData> TrackCircuits { get; set; } = [];


        /*
                /// <summary>
                /// 信号機情報リスト
                /// </summary>
                public List<SignalData> Signals { get; set; } = [];*/

        /// <summama
        /// CTCてこ情報リスト
        /// </summary>
        /*public List<InterlockingLeverData> CTCLevers { get; set; } = [];*/
        public List<RouteData> RouteDatas { get; set; } = [];

        /*
                /// <summary>
                /// 物理鍵てこ情報リスト
                /// </summary>
                public List<InterlockingKeyLeverData> PhysicalKeyLevers { get; set; } = [];*/

        /// <summary>
        /// 集中・駅扱状態
        /// </summary>
        public Dictionary<string, CenterControlState> CenterControlStates { get; set; } = [];

        /// <summary>
        /// 列番情報リスト
        /// </summary>
        public List<InterlockingRetsubanData> Retsubans { get; set; } = [];

        /// <summary>
        /// 表示灯情報リスト
        /// </summary>
        public Dictionary<string, bool> Lamps { get; set; } = [];


        /// <summary>
        /// TST時差
        /// </summary>
        public int TimeOffset { get; set; } = 14;
    }

    public enum CenterControlState {
        StationControl,
        CenterControl
    }
}
