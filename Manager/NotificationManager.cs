using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TatehamaCTCPClient.Manager {
    public class NotificationManager {

        private static readonly StringBuilder log = new();

        private static bool updated = false;

        public static bool Updated {
            get {
                var v = updated;
                updated = false;
                return v;
            }
        }

        public static void AddNotification(string text, bool notifyUpdate = true) {
            log.Append($"{DateTime.Now.ToString()} ");
            log.AppendLine(text);
            updated |= notifyUpdate;
        }

        public static string GetNotification() {
            return log.ToString();
        }
    }
}
