using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;


namespace MyUtils
{
    
    /// <summary>
    /// 自作汎用関数群
    /// </summary>
    public static class MyMethods
    {

        /// <summary>
        /// コレクションの中身を指定文字区切りで結合して返す。自作してみた
        /// </summary>
        /// <param name="str_list">文字列のリスト</param>
        /// <param name="divchar">区切り文字</param>
        /// <returns>連結文字</returns>
        public static string ListToStrLine(List<string> collection, char divchar)
        {
            int N = collection.Count;
            string line_str = "";
            for (int n = 0; n < N - 1; n++)
                line_str += (collection[n] + divchar);
            return line_str + collection[N - 1];
        }
        public static List<string> StrLineToList(string text, char divchar)
        {
            List<string> ret_list = new List<string>();
            string[] words = text.Split(divchar);

            foreach (string s in words)
                ret_list.Add(s);

            return ret_list;
        }

        public static string DictionaryToStrLine(Dictionary<string, int> collection, char divchar1, char divchar2, bool containkey=true)
        {
            int N = collection.Count, count = 0;
            string line_str = "";
            foreach (KeyValuePair<string, int> pair in collection)
            {
                count++;
                if (containkey)
                {
                    if (count != N)
                        line_str += (pair.Key + divchar1 + (pair.Value).ToString() + divchar2);
                    else
                        line_str += (pair.Key + divchar1 + (pair.Value).ToString());
                }
                else
                {
                    if (count != N)
                        line_str += ((pair.Value).ToString() + divchar1);
                    else
                        line_str += (pair.Value).ToString();
                }
            }
            return line_str;
        }
        public static Dictionary<string, int> StrLineToDictionaryInt(string text, char divchar1, char divchar2)
        {
            Dictionary<string, int> ret_dict = new Dictionary<string, int>();
            string[] words1 = text.Split(divchar1);
            foreach (string w in words1)
            {
                string[] words2 = w.Split(divchar2);
                if (words2.Length != 2)
                    return ret_dict;
                else
                {
                    int val;
                    if (int.TryParse(words2[1], out val))
                        ret_dict.Add(words2[0], val);
                    else
                        return ret_dict;
                }
            }
            return ret_dict;
        }

        public static string DictionaryToStrLine(Dictionary<string, string> collection, char divchar1, char divchar2, bool containkey=true)
        {
            int N = collection.Count, count = 0;
            string line_str = "";
            foreach (KeyValuePair<string, string> pair in collection)
            {
                count++;
                if (containkey)
                {
                    if (count != N)
                        line_str += (pair.Key + divchar1 + pair.Value + divchar2);
                    else
                        line_str += (pair.Key + divchar1 + pair.Value);
                }
                else
                {
                    if (count != N)
                        line_str += ((pair.Value).ToString() + divchar1);
                    else
                        line_str += ((pair.Value).ToString());
                }
            }
            return line_str;
        }
        public static Dictionary<string, string> StrLineToDictionary(string text, char divchar1, char divchar2)
        {
            Dictionary<string, string> ret_dict = new Dictionary<string, string>();
            string[] words1 = text.Split(divchar1);
            foreach (string w in words1)
            {
                string[] words2 = w.Split(divchar2);
                if (words2.Length != 2)
                    return ret_dict;
                else
                    ret_dict.Add(words2[0], words2[1]);
            }
            return ret_dict;
        }

        public static string DictionaryToStrLine(Dictionary<string, List<string>> collection, char[] divchar, bool containkey = true)
        {
            if (divchar.Length != 3)
                return "";

            int N = collection.Count, count = 0;
            string line_str = "";
            foreach (KeyValuePair<string, List<string>> pair in collection)
            {
                string conectvalue = "";
                int N2 = pair.Value.Count, count2 = 0;
                foreach(string s in pair.Value)
                {
                    count2++;
                    if (count2 != N2)
                        conectvalue += (s + divchar[1]);
                    else
                        conectvalue += s;
                }

                count++;
                if (containkey)
                {
                    if (count != N)
                        line_str += (pair.Key + divchar[0] + conectvalue + divchar[2]);
                    else
                        line_str += (pair.Key + divchar[0] + conectvalue);
                }
                else
                {
                    if (count != N)
                        line_str += (conectvalue + divchar[2]);
                    else
                        line_str += conectvalue;
                }
            }
            return line_str;
        }
        public static Dictionary<string, List<string>> StrLineToDictionaryList(string text, char[] divchar)
        {
            Dictionary<string, List<string>> ret_dict = new Dictionary<string, List<string>>();

            if (divchar.Length != 3)
                return ret_dict;

            string[] words = text.Split(divchar[2]);

            List<string> tmp_list;
            foreach(string s1 in words)
            {
                string[] words2 = s1.Split(divchar[0]);

                if (words2.Length != 2)
                    return ret_dict;

                tmp_list = new List<string>();
                string[] words3 = words2[1].Split(divchar[1]);
                foreach(string s2 in words3)
                    tmp_list.Add(s2);

                ret_dict.Add(words2[0], new List<string>(tmp_list));
            }

            return ret_dict;
        }


        /// <summary>
        /// デバッグ用コンソール表示
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="list"></param>
        // リストの中身をコンソール表示
        public static void ShowStringList<Type>(List<Type> list)
        {
            Console.Write("[");
            foreach (Type str in list)
                Console.Write("{0}, ", str);
            Console.WriteLine("]");
            return;
        }
        // スタックの中身をコンソール表示
        public static void ShowStringStack<Type>(Stack<Type> stack)
        {
            Stack<Type> copy_stack = new Stack<Type>(stack);
            Console.Write("[");
            while (copy_stack.Count != 0)
                Console.Write("{0}, ", copy_stack.Pop());
            Console.WriteLine("]");
            return;
        }


        /// <summary>
        /// テキストボックスの入力内容が自然数に変換可能か調べる
        /// </summary>
        /// <param name="text">テキスト</param>
        /// <returns>自然数, -1は無効な入力</returns>
        public static int CheckTextBoxAmount(string text)
        {
            // 未入力は0を返す
            if (text == "")
                return 0;

            // ','区切りで合計値を返す
            int sum = 0, val;
            string[] words = text.Split(',');
            for (int n = 0; n < words.Length; n++)
            {
                val = MyUtils.StringToFormula.OutValue(words[n]);
                if (val < 0)
                    return -1; // 無効な入力は-1を返す
                else
                    sum += val;
            }
            return sum;
        }


        /// <summary>
        /// ファイル関係
        /// </summary>
        /// <param name="stSourcePath"></param>
        /// <param name="stDestPath"></param>
        public static void CopyDirectory(string stSourcePath, string stDestPath)
        {
            CopyDirectory(stSourcePath, stDestPath, false);
        }
        public static void CopyDirectory(string stSourcePath, string stDestPath, bool bOverwrite)
        {
            // コピー先のディレクトリがなければ作成する
            if (!System.IO.Directory.Exists(stDestPath))
            {
                System.IO.Directory.CreateDirectory(stDestPath);
                System.IO.File.SetAttributes(stDestPath, System.IO.File.GetAttributes(stSourcePath));
                bOverwrite = true;
            }

            // コピー元のディレクトリにあるすべてのファイルをコピーする
            if (bOverwrite)
            {
                foreach (string stCopyFrom in System.IO.Directory.GetFiles(stSourcePath))
                {
                    string stCopyTo = System.IO.Path.Combine(stDestPath, System.IO.Path.GetFileName(stCopyFrom));
                    System.IO.File.Copy(stCopyFrom, stCopyTo, true);
                }

                // 上書き不可能な場合は存在しない時のみコピーする
            }
            else
            {
                foreach (string stCopyFrom in System.IO.Directory.GetFiles(stSourcePath))
                {
                    string stCopyTo = System.IO.Path.Combine(stDestPath, System.IO.Path.GetFileName(stCopyFrom));

                    if (!System.IO.File.Exists(stCopyTo))
                    {
                        System.IO.File.Copy(stCopyFrom, stCopyTo, false);
                    }
                }
            }

            // コピー元のディレクトリをすべてコピーする (再帰)
            foreach (string stCopyFrom in System.IO.Directory.GetDirectories(stSourcePath))
            {
                string stCopyTo = System.IO.Path.Combine(stDestPath, System.IO.Path.GetFileName(stCopyFrom));
                CopyDirectory(stCopyFrom, stCopyTo, bOverwrite);
            }
        }


        /// <summary>
        /// Excel用:列インデックスをExcelの列名に変換
        /// </summary>
        /// <param name="row">行番号</param>
        /// <param name="col">列番号</param>
        /// <returns>セル名</returns>
        public static string GetCellName(int row, int col)
        {
            col = col - 1;
            string str = "";
            do
            {
                str = Convert.ToChar(col % 26 + 0x41) + str;
            } while ((col = col / 26 - 1) != -1);

            return str + row.ToString();
        }


    }

    /// <summary>
    /// 動作ログ書き込み用のクラス
    /// </summary>
    public class Logger
    {

        // ログファイル名
        private static string LOGFILENAME;

        // 最大ファイルサイズ (超えていればインスタンス生成時に一度ファイルが削除される)
        private const int MAXSIZE = 10 * 1000 * 1000; // 10MB(だいたい)

        // 現在時刻の取得
        private static string Now() { return DateTime.Now.ToString("yyyy/MM/dd(HH:mm:ss)"); }
        
        // メソッド名付きで書き込み
        public void WriteWithMethod(string add_info)
        {
            // 関数名、行数取得
            StackFrame sf = new StackFrame(1, true);
            string methodname = sf.GetMethod().ToString();
            string classname = sf.GetMethod().ReflectedType.FullName;

            // 結合
            string log = Now() + classname + "->" + methodname + " " + add_info;

            using (StreamWriter wfile = new StreamWriter(LOGFILENAME, true, Encoding.GetEncoding("Shift_JIS")))
            {
                wfile.WriteLine(log);
            }
            return;
        }

        // 書き込み
        public void Write(string log)
        {
            // 結合
            log = Now() + log;

            using (StreamWriter wfile = new StreamWriter(LOGFILENAME, true, Encoding.GetEncoding("Shift_JIS")))
            {
                wfile.WriteLine(log);
            }
            return;
        }

        // 自主的にリフレッシュ
        public void Refresh()
        {
            using (StreamWriter wfile = new StreamWriter(LOGFILENAME, false, Encoding.GetEncoding("Shift_JIS")))
            {
            }
            return;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="fname">ログファイル名</param>
        public Logger(string fname)
        {
            LOGFILENAME = fname;
            // サイズ超過であれば削除
            if (File.Exists(fname))
            {
                FileInfo fi = new FileInfo(fname);
                if (fi.Length >= MAXSIZE)
                    fi.Delete();
            }

        }


    }

    /// <summary>
    /// ログイン管理（ワークスペース単位）
    /// </summary>
    public class LoginManage
    {

        /// <summary>
        /// ログイン状態
        /// </summary>
        public enum STATE
        {
            SUCCESS, // ログイン成功
            NOTMATCH, // パスワード不一致
            NOREISTERED, // 未登録
            INVALID, // ワークスペースにアクセス中
            DEFAULT, // 未操作状態
        }

        /// <summary>
        ///  ユーザ情報を扱うクラス
        /// </summary>
        private class UserInfo
        {
            public string ID { get; } // ID
            public string PASS { get; } // PassWord
            public string DIR { get; } // Workspace
            public UserInfo(string id, string pass, string dir)
            {
                ID = id;
                PASS = pass;
                DIR = dir;
            }
        }

        /// <summary>
        /// ワークスペースへの多重アクセスを禁止するためのクラス
        /// </summary>
        private class ExclusionCtlr
        {

            private static string LOCKEDFILE = ".locked";
            private string PATH;
            private string STATE = "UNLOCKED";

            public ExclusionCtlr() { }
            public ExclusionCtlr(string rootpath) { PATH = rootpath + LOCKEDFILE; }
            ~ExclusionCtlr()
            {
                Release();
            }
            /// <summary>
            /// ロック
            /// </summary>
            /// <returns></returns>
            public bool Lock()
            {
                if (File.Exists(PATH))
                    return false;
                else
                {
                    File.Create(PATH);
                    STATE = "LOCKED";
                    return true;
                }
            }
            /// <summary>
            /// ロック中かどうかの判定
            /// </summary>
            /// <returns></returns>
            private bool IsLocked()
            {
                if (STATE == "LOCKED")
                    return true;
                else
                    return false;
            }
            /// <summary>
            /// 解放
            /// </summary>
            private void Release()
            {
                if (IsLocked())
                {
                    File.Delete(PATH);
                    STATE = "UNLOCKED";
                }
            }

        }

        // ユーザ情報管理
        public STATE LoginState { get; set; } = STATE.DEFAULT;
        private Dictionary<string, List<string>> userslist = new Dictionary<string, List<string>>();

        private UserInfo uinfo;
        private ExclusionCtlr loginLock;
        private const string USERSFILE = "users.txt";
        private const char USERS_DIVCHAR = ',';
        private const string INVALID_CHARS = ""; // 入力禁止文字（未使用）

        /// <summary>
        /// usrelistの読み込み
        /// </summary>
        /// <returns>true = 成功</returns>
        public bool LoadUsers()
        {
            userslist = new Dictionary<string, List<string>>();
            try
            {
                using (StreamReader rfile = new StreamReader(USERSFILE, Encoding.GetEncoding("Shift_JIS")))
                {
                    string line = "";
                    // test.txtを1行ずつ読み込んでいき、末端(何もない行)までwhile文で繰り返す
                    while ((line = rfile.ReadLine()) != null)
                    {
                        string[] words = line.Split(USERS_DIVCHAR);
                        userslist.Add(words[0], new List<string> { (PassEncryption(words[1], false)), words[2] });
                    }
                }
                return true;
            }
            catch (System.Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Userの追加
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="pass">PASSWARD</param>
        /// <returns>true = 成功</returns>
        public bool AddUser(string id, string pass, string root_dir)
        {
            userslist.Add(id, new List<string> { pass, root_dir });
            return UpdateUsersInfo();
        }

        /// <summary>
        /// User情報の変更
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="pass">PASSWARD</param>
        /// <returns>true = 成功</returns>
        public bool ChangeUsersInfo(string old_id, string id, string pass, string root_dir)
        {
            userslist.Remove(old_id);
            userslist.Add(id, new List<string> { pass, root_dir });
            return UpdateUsersInfo();
        }

        /// <summary>
        /// ログイン実施
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="pass">PASSWARD</param>
        /// <returns>true:成功, false:失敗。同時に状態が書き換わる</returns>
        public void Login(string id, string pass)
        {
            if (userslist.ContainsKey(id))
            {
                string register_pass = userslist[id][0];
                if (pass == register_pass)
                {
                    uinfo = new UserInfo(id, userslist[id][0], userslist[id][1]);
                    loginLock = new ExclusionCtlr(uinfo.DIR);
                    if (!loginLock.Lock())
                    {
                        LoginState = STATE.INVALID;
                        return;
                    }
                    LoginState = STATE.SUCCESS;
                    return;
                }
                else
                {
                    LoginState = STATE.NOTMATCH;
                    return;
                }
            }
            else
            {
                LoginState = STATE.NOREISTERED;
                return;
            }
        }

        /// <summary>
        /// ユーザーリストの取得
        /// </summary>
        /// <returns></returns>
        public List<string> GetUsers()
        {
            return new List<string>(userslist.Keys);
        }

        /// <summary>
        /// ログイン中ユーザーのIDを返す
        /// </summary>
        /// <returns></returns>
        public string GetID()
        {
            if (LoginState == STATE.SUCCESS)
                return uinfo.ID;
            else
                return "";
        }

        /// <summary>
        /// ログイン中ユーザーのパスワードを返す
        /// </summary>
        /// <returns></returns>
        public string GetPASS()
        {
            if (LoginState == STATE.SUCCESS)
                return uinfo.PASS;
            else
                return "";
        }

        /// <summary>
        /// ログイン中ユーザーのワークスペースのパスを返す
        /// </summary>
        /// <returns></returns>
        public string GetDIR()
        {
            if (LoginState == STATE.SUCCESS)
                return uinfo.DIR;
            else
                return "";
        }

        /// <summary>
        /// ユーザの登録確認
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ExistUser(string id)
        {
            return userslist.ContainsKey(id);
        }

        /// <summary>
        /// ユーザー情報の更新
        /// </summary>
        /// <returns>true=成功</returns>
        private bool UpdateUsersInfo()
        {
            try
            {
                using (StreamWriter wfile = new StreamWriter(USERSFILE, false, Encoding.GetEncoding("Shift_JIS")))
                {
                    foreach (KeyValuePair<string, List<string>> pair in userslist)
                        wfile.WriteLine(pair.Key + USERS_DIVCHAR + PassEncryption(pair.Value[0], true) + USERS_DIVCHAR + pair.Value[1]);
                }
                return true;
            }
            catch (System.Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// パスワードの暗号化 ※未実装
        /// </summary>
        /// <param name="pass">パスワード</param>
        /// <param name="opt">true=encode, false=decode</param>
        /// <returns></returns>
        private string PassEncryption(string pass, bool opt)
        {
            string ret_pass;

            if (opt)
            {
                ret_pass = pass;
            }
            else
            {
                ret_pass = pass;
            }

            return ret_pass;
        }

    }

    /// <summary>
    /// 文字列を四則演算に置き換えるクラス
    /// </summary>
    public static class StringToFormula
    {
        // デバッグ用コンソール出力の有無
        private const bool CONSOLE_WRITE_ON = false;
        // 演算子と優先度の定義
        private static Dictionary<string, int> OPERATORS = new Dictionary<string, int> { { "(", 1 }, { ")", 2 }, { "+", 3 }, { "-", 4 }, { "*", 5 }, { "/", 6 } };

        // 演算子の優先度比較
        private static bool IsLowerPriority(string op1, string op2)
        {
            return OPERATORS[op2] > OPERATORS[op1];
        }

        // 四則演算
        private static int DoArithmethic(int val1, int val2, string op)
        {
            switch (op)
            {
                case "+":
                    return val1 + val2;
                case "-":
                    return val1 - val2;
                case "*":
                    return val1 * val2;
                case "/":
                    return val1 / val2;
                default:
                    return -1;
            }
        }

        // 1トークンずつに分割
        private static List<string> DivideByToken(string input)
        {
            List<string> tokens = new List<string>();
            string tmp = input;

            // 演算子, 修飾子の前後にスペースを追加する
            foreach (string tk in OPERATORS.Keys)
                tmp = tmp.Replace(tk, " " + tk + " ");

            // 空白文字で分割 ⇒ 数値, 演算子, 修飾子のみをリストに追加
            string[] words = tmp.Split(' ');
            for (int i = 0; i < words.Length; i++)
                if (words[i] != "") tokens.Add(words[i]);

            return tokens;
        }

        // 逆ポーランド記法に変換
        private static int RPConv(List<string> tokens, ref List<string> rpcbuff)
        {
            int count = 0;
            rpcbuff = new List<string>();
            Stack<string> tmpbuff = new Stack<string>();
            string tmpstr;

            // 各種トークンの処理
            foreach (string token in tokens)
            {
                count++;
                switch (token)
                {
                    case "+":
                    case "-":
                    case "*":
                    case "/":
                        while (tmpbuff.Count != 0)
                        {
                            if (IsLowerPriority(token, tmpbuff.Peek()))
                                rpcbuff.Add(tmpbuff.Pop());
                            else
                                break;
                        }
                        tmpbuff.Push(token);
                        break;
                    case "(":
                        // そのままスタックへ追加
                        tmpbuff.Push(token);
                        break;
                    case ")":
                        // "("までスタックをポップし、バッファへ追加
                        bool invalidcheck = true;
                        while (true)
                        {
                            tmpstr = tmpbuff.Pop();
                            if (tmpstr == "(")
                            {
                                invalidcheck = false;
                                break;
                            }
                            else
                                rpcbuff.Add(tmpstr);
                        }
                        if (invalidcheck) return -1; // "("がなければリターン
                        break;
                    default:
                        // そのままバッファへ追加
                        if (int.TryParse(token, out int result))
                            rpcbuff.Add(token);
                        else
                            return -1; // 数値変換不可ならリターン
                        break;
                }
                if (CONSOLE_WRITE_ON)
                {
                    Console.WriteLine(">> Phase {0}", count);
                    Console.Write("RPC_BUFFER : ");
                    MyMethods.ShowStringList<string>(rpcbuff);
                    Console.Write("TMP_BUFFER : ");
                    MyMethods.ShowStringStack<string>(tmpbuff);
                }

            }
            // 残りのスタックをバッファへ追加
            while (tmpbuff.Count != 0)
                rpcbuff.Add(tmpbuff.Pop());

            return 0;
        }

        // 逆ポーランド記法に基づいて計算
        private static int CalcRPFormula(List<string> rp)
        {
            int val, tmp1, tmp2, count = 0;
            Stack<int> buff = new Stack<int>();
            foreach (string str in rp)
            {
                count++;
                if (!OPERATORS.ContainsKey(str))
                    buff.Push(int.Parse(str)); // 演算子でなければスタック
                else
                {
                    // 2つ以上値がスタックされていなければエラーリターン(-1)
                    if (buff.Count < 2)
                        return -1;
                    // 四則演算を実行し、再スタック
                    tmp2 = buff.Pop();
                    tmp1 = buff.Pop();
                    buff.Push(DoArithmethic(tmp1, tmp2, str));
                }

                Console.WriteLine(">> Phase {0} token is {1}", count, str);
                Console.Write("BUFFER : ");
                MyMethods.ShowStringStack<int>(buff);

            }
            // 最終的に残った値を格納、スタックに余計な物が残っている場合はエラーリターン(-1)
            val = (buff.Count == 1) ? buff.Pop() : -1;
            return val;
        }

        /// <summary>
        /// string形式の計算式の結果を返す
        /// </summary>
        /// <param name="input">入力文字列</param>
        /// <returns>結果</returns>
        public static int OutValue(string input)
        {
            if (CONSOLE_WRITE_ON) Console.WriteLine("Input << {0}", input);

            List<string> tokens = DivideByToken(input);

            if (CONSOLE_WRITE_ON)
            {
                Console.Write("Divide by token >> ");
                MyMethods.ShowStringList<string>(tokens);
            }

            int ret_state;
            List<string> rp = new List<string>();
            ret_state = RPConv(tokens, ref rp);
            if (ret_state == -1) return ret_state;

            if (CONSOLE_WRITE_ON)
            {
                Console.Write("Reverse Polish Convert >> ");
                MyMethods.ShowStringList<string>(rp);
                Console.WriteLine("Calc Reverse Polish Fomula >> ");
            }

            return CalcRPFormula(rp);
        }

    }


}
