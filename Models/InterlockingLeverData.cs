
namespace TatehamaCTCPClient.Models {

    /// <summary>
    /// 物理てこデータクラス
    /// </summary>
    public class InterlockingLeverData {
        /// <summary>
        /// 物理てこ名称
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 物理てこの状態
        /// </summary>
        public LCR State { get; set; } = LCR.Center;
    }
    public enum LCR {
        Left,
        Center,
        Right,
    }
}
