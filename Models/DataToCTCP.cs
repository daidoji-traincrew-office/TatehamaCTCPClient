using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public List<RouteData> RouteDatas { get; set; }

        /*
                /// <summary>
                /// 物理鍵てこ情報リスト
                /// </summary>
                public List<InterlockingKeyLeverData> PhysicalKeyLevers { get; set; } = [];*/

        /// <summary>
        /// 集中・駅扱状態
        /// </summary>
        public Dictionary<string, CenterControlState> CenterControlStates { get; set; }

        /// <summary>
        /// 列番情報リスト
        /// </summary>
        public List<InterlockingRetsubanData> Retsubans { get; set; } = [];

        /// <summary>
        /// 表示灯情報リスト
        /// </summary>
        public Dictionary<string, bool> Lamps { get; set; } = [];
    }

    public enum CenterControlState {
        StationControl,
        CenterControl
    }
}
