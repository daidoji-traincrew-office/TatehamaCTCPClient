using System.Collections.ObjectModel;
using System.Drawing.Imaging;

namespace TatehamaCTCPClient.Models {
    public class CharacterSet {
        public static Image CharacterImage { get; private set; } = Image.FromFile(".\\png\\characters.png");

        public static readonly object syncCharacterImage = new object();

        private Dictionary<char, Character> characters = [];

        private Dictionary<string, MultiCharacter> multiCharacters = [];

        public ReadOnlyDictionary<char, Character> Characters { get; init; }

        public ReadOnlyDictionary<string, MultiCharacter> MultiCharacters { get; init; }

        public CharacterSet(string fileName) {
            Characters = characters.AsReadOnly();
            MultiCharacters = multiCharacters.AsReadOnly();

            try {
                using var sr = new StreamReader($".\\tsv\\{fileName}");
                sr.ReadLine();
                var line = sr.ReadLine();
                while (line != null) {
                    if (line.StartsWith('#')) {
                        line = sr.ReadLine();
                        continue;
                    }
                    var texts = line.Split('\t');
                    line = sr.ReadLine();

                    if (texts.Length < 5 || texts.Any(t => t == "")) {
                        continue;
                    }
                    if (texts[0].Length != 1) {
                        multiCharacters.Add(texts[0], new(texts[0], int.Parse(texts[1]), int.Parse(texts[2]), int.Parse(texts[3]), int.Parse(texts[4])));
                    }
                    else {
                        characters.Add(texts[0][0], new(texts[0][0], int.Parse(texts[1]), int.Parse(texts[2]), int.Parse(texts[3]), int.Parse(texts[4])));
                    }

                }
            }
            catch {
            }
        }

        public bool DrawText(Graphics g, string text, int x, int y, int width, int height, ImageAttributes ia, ContentAlignment align = ContentAlignment.MiddleCenter) {
            if(text.Length <= 0) {
                return true;
            }
            if (multiCharacters.TryGetValue(text, out var mc)) {
                if (mc.Size.Width > width || mc.Size.Height > height) {
                    return false;
                }
                if((int)align >= 256) {
                    y += height - mc.Size.Height;
                }
                else if ((int)align >= 16) {
                    y += (height - mc.Size.Height) / 2;
                }
                switch (align) {
                    case ContentAlignment.TopCenter:
                    case ContentAlignment.MiddleCenter:
                    case ContentAlignment.BottomCenter:
                        x += (width - mc.Size.Width) / 2;
                        break;
                    case ContentAlignment.TopRight:
                    case ContentAlignment.MiddleRight:
                    case ContentAlignment.BottomRight:
                        x += width - mc.Size.Width;
                        break;
                }
                lock (syncCharacterImage) {
                    g.DrawImage(CharacterImage, new Rectangle(x, y, mc.Size.Width, mc.Size.Height), mc.Location.X, mc.Location.Y, mc.Size.Width, mc.Size.Height, GraphicsUnit.Pixel, ia);
                }
                return true;
            }
            var cList = new List<Character>();
            foreach(var c in text) {
                if (characters.TryGetValue(c, out var character)) {
                    cList.Add(character);
                }
                else {
                    return false;
                }
            }
            var w = cList.Sum(c => c.Size.Width + 1) - 1;
            var h = cList.Max(c => c.Size.Height);
            if (w > width || h > height) {
                return false;
            }
            if ((int)align >= 256) {
                y += height - h;
            }
            else if ((int)align >= 16) {
                y += (height - h) / 2;
            }
            switch (align) {
                case ContentAlignment.TopCenter:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.BottomCenter:
                    x += (width - w) / 2;
                    break;
                case ContentAlignment.TopRight:
                case ContentAlignment.MiddleRight:
                case ContentAlignment.BottomRight:
                    x += width - w;
                    break;
            }
            foreach (var c in cList) {
                lock (syncCharacterImage) {
                    g.DrawImage(CharacterImage, new Rectangle(x, y + h - c.Size.Height, c.Size.Width, c.Size.Height), c.Location.X, c.Location.Y, c.Size.Width, c.Size.Height, GraphicsUnit.Pixel, ia);
                }
                x += c.Size.Width + 1;
            }

            return true;
        }

    }
}
