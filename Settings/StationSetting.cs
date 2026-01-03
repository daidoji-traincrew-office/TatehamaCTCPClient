
namespace TatehamaCTCPClient.Settings {
    public class StationSetting(string code, string name, string fullName, string leverName, Point labelLocation, Point areaLocation, Size areaSize) {
        public string Code { get; init; } = code;
        public string Number => Code.Replace("S", "");

        public string Name { get; init; } = name;

        public string FullName { get; init; } = fullName;

        public string LeverName { get; init; } = leverName;

        public Point LabelLocation { get; init; } = labelLocation;

        public Point AreaLocation { get; init; } = areaLocation;

        public Size AreaSize { get; init; } = areaSize;

        public bool Active { get; private set; } = false;

        public void SetActive(bool v) {
            Active = v;
        }
    }
}
