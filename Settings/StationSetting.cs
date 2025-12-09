
namespace TatehamaCTCPClient.Settings {
    public class StationSetting(string code, string name, string leverName, Point location) {
        public string Code { get; init; } = code;
        public string Number => Code.Replace("S", "");

        public string Name { get; init; } = name;

        public string LeverName { get; init; } = leverName;

        public Point Location { get; init; } = location;

        public bool Active { get; private set; } = true;
    }
}
