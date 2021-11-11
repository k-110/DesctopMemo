using System;
using System.Windows.Forms;

namespace DesctopMemo
{
    /// <summary>
    ///  テキストを表示／編集するためのウインドウ
    /// </summary>
    public partial class Form_Edit : Form
    {
        /// <summary>
        ///  コンストラクタ
        ///  Fromウインドウを生成し引数で渡されたテキストを表示エリアにセットする
        /// </summary>
        /// <param name="buf">表示／編集するテキストの初期値</param>
        public Form_Edit(string buf)
        {
            InitializeComponent();

            //textBoxでは\rがないと改行表示されないため\n→\rn変換する
            //(XMLのシリアライズ化する際に\rが消えたりするので・・・)
            string tmp_buf = buf.Replace("\r\n", "\n");
            textBox_Note.Text = tmp_buf.Replace("\n", "\r\n");
        }

        /// <summary>
        ///  表示／編集されたテキストを取得する
        /// </summary>
        /// <returns>表示／編集されたテキスト</returns>
        public string GetText()
        {
            return textBox_Note.Text;
        }
    }
}
