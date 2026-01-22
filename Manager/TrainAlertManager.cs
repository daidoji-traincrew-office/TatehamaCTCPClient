using System.Collections.ObjectModel;
using TatehamaCTCPClient.Buttons;
using TatehamaCTCPClient.Forms;
using TatehamaCTCPClient.Models;
using TatehamaCTCPClient.Settings;

namespace TatehamaCTCPClient.Manager {
    public class TrainAlertManager {

        private static List<TrainAlert> trainAlerts = [];

        public static ReadOnlyCollection<TrainAlert> TrainAlerts { get; private set; } = trainAlerts.AsReadOnly();

        public static bool IsNotEmpty => trainAlerts.Count > 0;

        public static bool HasImportant => trainAlerts.Any(a => a.Important);

        public static bool AddAlert(TrainAlert alert) {
            var l = new List<TrainAlert>(trainAlerts);
            foreach (var a in l) {
                if (a.TrainNumber == alert.TrainNumber) {
                    if (alert.Important && !a.Important || alert.Station != a.Station) {
                        trainAlerts.Remove(a);
                    }
                    else {
                        return false;
                    }
                }
            }
            if (trainAlerts.Count > 0 && alert.Important) {
                trainAlerts.Insert(0, alert);
            }
            else {
                trainAlerts.Add(alert);
            }
            if (alert.Important) {
                CTCPWindow.PlayAprPiSound();
            }
            return true;
        }

        public static bool RemoveAlert(string routeGroup, AlertType? type = null) {
            var l = new List<TrainAlert>(trainAlerts);
            var v = false;
            var i = true;
            foreach(var a in l) {
                if (a.RouteGroup == routeGroup && (type == null || type == a.Type)) {
                    v |= trainAlerts.Remove(a);
                }
                else if(a.Important) {
                    i = false;
                }
            }
            if (i) {
                CTCPWindow.StopAprPiSound();
            }
            return v;
        }

        public static bool RemoveAlert(StationSetting station, AlertType? type = null) {
            var l = new List<TrainAlert>(trainAlerts);
            var v = false;
            var i = true;
            foreach (var a in l) {
                if (a.Station == station && (type == null || type == a.Type)) {
                    v |= trainAlerts.Remove(a);
                }
                else if (a.Important) {
                    i = false;
                }
            }
            if (i) {
                CTCPWindow.StopAprPiSound();
            }
            return v;
        }

        public static bool RemoveAlertTrain(string trainNumber, AlertType? type = null) {
            var l = new List<TrainAlert>(trainAlerts);
            var v = false;
            var i = true;
            foreach (var a in l) {
                if (a.TrainNumber == trainNumber && (type == null || type == a.Type)) {
                    v |= trainAlerts.Remove(a);
                }
                else if (a.Important) {
                    i = false;
                }
            }
            if (i) {
                CTCPWindow.StopAprPiSound();
            }
            return v;
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
