
namespace TatehamaCTCPClient.Models {

    /// <summary>
    /// 物理鍵てこデータクラス
    /// </summary>
    public class InterlockingKeyLeverData {
        /// <summary>
        /// 物理鍵てこ名称
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 物理鍵てこの状態
        /// </summary>
        public LNR State { get; set; } = LNR.Normal;
        /// <summary>
        /// 物理鍵てこの鍵挿入状態
        /// </summary>
        public bool IsKeyInserted { get; set; } = false;
    }

    public enum LNR {
        Left,
        Normal,
        Right
    }
}
