using System;
using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace DesctopMemo
{
    /// <summary>
    ///  注意書きを表示する常駐型のFromアプリケーション
    /// </summary>
    public partial class Form_Main : Form
    {

        #region Win32のAPI定義
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        [DllImportAttribute("user32.dll")]
        private static extern bool ReleaseCapture();
        #endregion

        /// <summary>
        ///  ウインドウの高さの最小値
        /// </summary>
        public const int MIN_HEIGHT = 100;
        /// <summary>
        ///  ウインドウ枠と表示する文字列との隙間のサイズ
        /// </summary>
        public const int VIEW_SP = 10;
        readonly NotifyIcon icon_Main = new NotifyIcon();

        /// <summary>
        ///  コンストラクタ
        ///  注意書きを表示するウインドウを表示する
        /// </summary>
        public Form_Main()
        {
            InitializeComponent();

            //設定のロード
            CAppSetting.Load();

            //表示するテキストに合わせてFromのサイズ変更
            Form_Main_Resize();

            //初期表示位置を指定
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point((System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width - this.Width)/2, 50);
            this.TransparencyKey = System.Drawing.Color.Green;
            this.BackColor = this.TransparencyKey;

            //タスクトレイに表示
            icon_Main.Icon = this.Icon;
            icon_Main.Visible = true;
            icon_Main.Text = "DesktopGhost";
            icon_Main.ContextMenuStrip = contextMenuStrip_Main;
        }

        /// <summary>
        ///  注意書きのテキスト量に応じてウインドウサイズを変更する
        ///  最少は<see cref="MIN_HEIGHT"/>で指定されているサイズ
        /// </summary>
        public void Form_Main_Resize()
        {
            Bitmap bmp = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (Graphics tmp_Graphics = Graphics.FromImage(bmp))
            using (System.Drawing.Font tmp_Font = new System.Drawing.Font(CAppSetting.Setting.CONFIG.FONT.name, CAppSetting.Setting.CONFIG.FONT.size))
            {
                SizeF strSize = tmp_Graphics.MeasureString(CAppSetting.Setting.NOTE, tmp_Font, this.Width, new StringFormat());
                int height = (strSize.Height > MIN_HEIGHT) ? Convert.ToInt32(strSize.Height) : MIN_HEIGHT;
                this.Height = height + VIEW_SP*2;
            }
        }

        /// <summary>
        ///  イベント：描画処理
        /// </summary>
        /// <param name="sender">イベントの発行元</param>
        /// <param name="e">イベントのパラメータ</param>
        private void Form_Main_Pain(object sender, PaintEventArgs e)
        {
            Bitmap bmp = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (Graphics tmp_Graphics = Graphics.FromImage(bmp))
            using (System.Drawing.Font tmp_Font = new System.Drawing.Font(CAppSetting.Setting.CONFIG.FONT.name, CAppSetting.Setting.CONFIG.FONT.size))
            using (System.Drawing.SolidBrush tmp_Brush = new System.Drawing.SolidBrush(System.Drawing.Color.Black))
            using (System.Drawing.SolidBrush tmp_BgBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White))
            using (System.Drawing.SolidBrush tmp_Transparent = new System.Drawing.SolidBrush(this.TransparencyKey))
            using (Pen tmp_Pen = new Pen(System.Drawing.Color.Black))
            {
                tmp_Graphics.FillRectangle(tmp_Transparent, 0, 0, bmp.Width, bmp.Height);
                Image img_Chara = Properties.Resources.Charactor;
                tmp_Graphics.DrawImage(img_Chara, new Point(bmp.Width - img_Chara.Width, bmp.Height - img_Chara.Height));
                Point[] points = {  new Point(VIEW_SP/2, VIEW_SP/2),
                                    new Point(VIEW_SP/2, bmp.Height - VIEW_SP),
                                    new Point(bmp.Width - img_Chara.Width -VIEW_SP*2, bmp.Height - VIEW_SP),
                                    new Point(bmp.Width - img_Chara.Width -VIEW_SP*2, bmp.Height - (img_Chara.Height/2)),
                                    new Point(bmp.Width - img_Chara.Width -2, bmp.Height - (img_Chara.Height/2) - VIEW_SP),
                                    new Point(bmp.Width - img_Chara.Width -VIEW_SP*2, bmp.Height - (img_Chara.Height/2) - VIEW_SP*2),
                                    new Point(bmp.Width - img_Chara.Width -VIEW_SP*2, VIEW_SP/2) };
                tmp_Graphics.FillPolygon(tmp_BgBrush, points, System.Drawing.Drawing2D.FillMode.Winding);
                tmp_Graphics.DrawPolygon(tmp_Pen, points);
                tmp_Graphics.DrawString(CAppSetting.Setting.NOTE, tmp_Font, tmp_Brush, VIEW_SP, VIEW_SP, new StringFormat());
                e.Graphics.DrawImage(bmp, new Point(0, 0));
            }
        }

        /// <summary>
        ///  イベント：マウス押下処理
        ///  左ボタン押下でタイトルバーを掴む(押下しながらウインドウの移動が可能)
        /// </summary>
        /// <param name="sender">イベントの発行元</param>
        /// <param name="e">イベントのパラメータ</param>
        private void Form_Main_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //タイトルバーを掴む
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, IntPtr.Zero);
            }
        }

        /// <summary>
        ///  イベント：アプリケーションの終了処理
        ///  リソースを開放してアプリケーションを終了する
        /// </summary>
        /// <param name="sender">イベントの発行元</param>
        /// <param name="e">イベントのパラメータ</param>
        private void QuitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            icon_Main.Visible = false;
            icon_Main.Dispose();
            Application.Exit();
        }

        /// <summary>
        ///  イベント：注意書きの編集処理
        ///  <see cref="Form_Edit"/>を開いて注意書きを編集状態になる
        ///  <see cref="Form_Edit"/>のクローズで編集状態を解除する
        ///  編集状態の解除後に、注意書きのテキストを保存しウインドウを再描画する
        /// </summary>
        /// <param name="sender">イベントの発行元</param>
        /// <param name="e">イベントのパラメータ</param>
        private void EditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //テキスト編集可能とする
            using (Form_Edit tmp_Edit = new Form_Edit(CAppSetting.Setting.NOTE))
            {
                tmp_Edit.ShowDialog(this);
                CAppSetting.Setting.NOTE = tmp_Edit.GetText();
                CAppSetting.Save();
                Form_Main_Resize();
                Refresh();
            }
        }

        /// <summary>
        ///  イベント：アプリケーションの情報を表示
        /// </summary>
        /// <param name="sender">イベントの発行元</param>
        /// <param name="e">イベントのパラメータ</param>
        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            string Info =
                "Version\t" + assembly.GetName().Version + Environment.NewLine +
                Environment.NewLine +
                "このアプリケーションは以下のフリー素材を使用しています." + Environment.NewLine +
                "・いらすとや";
            MessageBox.Show(Info, "About", MessageBoxButtons.OK);
        }
    }
}
