
namespace TatehamaCTCPClient.Forms {
    public partial class SubWindowName : Form {

        private SubWindow window;

        public SubWindowName(SubWindow window) {
            this.window = window;
            InitializeComponent();

        }



        private void buttonCancel_Click(object sender, EventArgs e) {
            Close();
        }

        private void buttonDecision_Click(object sender, EventArgs e) {
            DecideName();
        }

        private void DecideName() {
            var name = textBox1.Text;
            if (name.Length <= 0) {
                TaskDialog.ShowDialog(this, new TaskDialogPage {
                    Caption = $"使用できないウィンドウ名 | {window.DisplayManager.Window.SystemNameLong} - ダイヤ運転会",
                    Heading = "使用できないウィンドウ名",
                    Icon = TaskDialogIcon.Warning,
                    Text = CTCPWindow.IsAprilFool ? "何してんのよ...？\n早くウィンドウ名、入れてちょうだい！" :
                        $"ウィンドウ名を入力してください。"
                });
            }
            else if (name == "全線CTCP") {
                TaskDialog.ShowDialog(this, new TaskDialogPage {
                    Caption = $"使用できないウィンドウ名 | {window.DisplayManager.Window.SystemNameLong} - ダイヤ運転会",
                    Heading = "使用できないウィンドウ名",
                    Icon = TaskDialogIcon.Warning,
                    Text = CTCPWindow.IsAprilFool ? "このウィンドウ名、使えないみたいよ。" :
                        $"このウィンドウ名は使用できません。"
                });
            }
            else {
                window.SetWindowName(name);
                Close();
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                DecideName();
            }
        }
    }
}
