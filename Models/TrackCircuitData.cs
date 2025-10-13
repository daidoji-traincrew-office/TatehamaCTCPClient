using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaCTCPClient.Models {

    [Serializable]
    public class TrackCircuitData {
        /// <summary>
        /// 列車が在線しているか
        /// </summary>
        public bool On { get; init; } = false;

        /// <summary>
        /// 鎖錠されているか
        /// </summary>
        public bool Lock { get; set; } = false;

        /// <summary>
        /// 最後に軌道回路を踏んだ列車の名前
        /// </summary>
        public string Last { get; init; } = "";

        /// <summary>
        /// 軌道回路の名称
        /// </summary>
        public string Name { get; init; } = "";

        public override string ToString() {
            return $"{Name}/{Last}/{On}";
        }
    }
}
