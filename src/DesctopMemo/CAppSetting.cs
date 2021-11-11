using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace DesctopMemo
{
    /// <summary>
    ///  設定値
    /// </summary>
    public struct MySetting
    {
        /// <summary>
        ///  アプリケーションのコンフィグ
        /// </summary>
        public MyCONFIG CONFIG;
        /// <summary>
        ///  注意書きの文字列
        /// </summary>
        public string NOTE;
        /// <summary>
        ///  <see cref="MySetting"/>の初期化処理
        /// </summary>
        public void Init()
        {
            CONFIG.Init();
            NOTE = "";
        }

        /// <summary>
        ///  アプリケーションのコンフィグ
        /// </summary>
        public struct MyCONFIG
        {
            /// <summary>
            ///  表示用のフォントを指定するための情報
            /// </summary>
            public MyFONT FONT;
            /// <summary>
            ///  <see cref="MyFONT"/>の初期化処理
            /// </summary>
            public void Init()
            {
                FONT.Init();
            }
        }

        /// <summary>
        ///  フォントを指定するための情報
        /// </summary>
        public struct MyFONT
        {
            /// <summary>
            ///  名前
            /// </summary>
            public string name;
            /// <summary>
            ///  サイズ
            /// </summary>
            public int size;
            /// <summary>
            ///  <see cref="MyFONT"/>の初期化処理
            /// </summary>
            public void Init()
            {
                name = "ＭＳ ゴシック";
                size = 9;
            }
        }
    }

    /// <summary>
    ///  設定値管理機能
    ///  設定値はxmlファイルの形式で扱う
    /// </summary>
    public static partial class CAppSetting
    {
        /// <summary>
        ///  設定値のファイルパス(デフォルトは実行ファイルのあるフォルダ内のAppSetting.xml)
        /// </summary>
        private static readonly string FilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DesctopMemo.xml");
        /// <summary>
        ///  アクセスを排他するためのロックオブジェクト
        /// </summary>
        private static readonly object SettingLock = new object();
        /// <summary>
        ///  設定値
        /// </summary>
        public static MySetting Setting;

        /// <summary>
        ///  <see cref="FilePath"/>で指定されたファイルから設定値をロードする処理
        /// </summary>
        /// <returns>処理の実行結果</returns>
        public static bool Load()
        {
            bool result = false;
            lock (SettingLock)
            {
                if (File.Exists(FilePath))
                {
                    try
                    {
                        XmlSerializer se = new XmlSerializer(typeof(MySetting));
                        using (StreamReader sr = new StreamReader(FilePath, new System.Text.UTF8Encoding(false)))
                        {
                            Setting = (MySetting)se.Deserialize(sr);
                            sr.Close();
                            result = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
                if (!result)
                {
                    Setting.Init();
                }
            }
            return result;
        }

        /// <summary>
        ///  <see cref="FilePath"/>で指定されたファイルへ設定値をセーブする処理
        /// </summary>
        /// <returns>処理の実行結果</returns>
        public static bool Save()
        {
            lock (SettingLock)
            {
                try
                {
                    XmlSerializer se = new XmlSerializer(typeof(MySetting));
                    using (StreamWriter sw = new StreamWriter(FilePath, false, new System.Text.UTF8Encoding(false)))
                    {
                        //名前空間のプレフィックスを出力しなようにする
                        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                        ns.Add(String.Empty, String.Empty);

                        se.Serialize(sw, Setting, ns);
                        sw.Close();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            return false;
        }

        /// <summary>
        ///  XMLファイルから読み取った文字列をテキスト表示できる文字列に変換する処理
        /// </summary>
        /// <param name="buf">XMLで読み取った文字列</param>
        /// <returns>表示用の文字列</returns>
        public static string XmlStringToViewString(string buf)
        {
            //\rがないと改行表示されないことがあるため\n→\rn変換する
            //(XMLのシリアライズ化する際に\rが消えたりするので・・・)
            string tmp_buf = buf.Replace("\r\n", "\n");
            return tmp_buf.Replace("\n", "\r\n");
        }

        /// <summary>
        ///  垂直スクロールバーの幅を取得
        /// </summary>
        /// <returns></returns>
        public static int GetVScrollBarWidth()
        {
            VScrollBar vscroll = new System.Windows.Forms.VScrollBar();
            return vscroll.Width;
        }

        /// <summary>
        ///  水平スクロールバーのサイズ高さを取得
        /// </summary>
        /// <returns></returns>
        public static int GetHScrollBarHight()
        {
            HScrollBar hscroll = new System.Windows.Forms.HScrollBar();
            return hscroll.Height;
        }
    }
}
