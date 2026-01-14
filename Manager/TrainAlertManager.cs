using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TatehamaCTCPClient.Buttons;
using TatehamaCTCPClient.Models;

namespace TatehamaCTCPClient.Manager {
    public class TrainAlertManager {

        private static List<TrainAlert> trainAlerts = [];

        public static ReadOnlyCollection<TrainAlert> TrainAlerts { get; private set; } = trainAlerts.AsReadOnly();

        public static bool IsNotEmpty => trainAlerts.Count > 0;

        public static void AddAlert(TrainAlert alert) {
            var l = new List<TrainAlert>(trainAlerts);
            foreach (var a in l) {
                if (a.TrainNumber == alert.TrainNumber) {
                    trainAlerts.Remove(a);
                }
            }
            if (trainAlerts.Count > 0 && alert.Important) {
                trainAlerts.Insert(0, alert);
            }
            else {
                trainAlerts.Add(alert);
            }
        }

        public static void RemoveAlert(string routeGroup) {
            var l = new List<TrainAlert>(trainAlerts);
            foreach(var a in l) {
                if (a.RouteGroup == routeGroup) {
                    trainAlerts.Remove(a);
                }
            }
        }

        public static LightingType GetLightingType(string train) {
            foreach (var a in trainAlerts) {
                if (a.TrainNumber != train) {
                    continue;
                }
                return a.Important ? LightingType.BLINKING_FAST : LightingType.BLINKING_SLOW;
            }
            return LightingType.LIGHTING;
        }


    }
}
