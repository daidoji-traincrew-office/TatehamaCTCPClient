using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TatehamaCTCPClient.Models {

    /// <summary>
    /// 列番データクラス
    /// </summary>
    public class InterlockingRetsubanData {
        /// <summary>
        /// 列番名称
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 列番
        /// </summary>
        public string Retsuban { get; set; } = "";
    }
}
