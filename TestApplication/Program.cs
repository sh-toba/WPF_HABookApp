using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication
{

    class StringToFomula
    {
        // デバッグ用コンソール出力の有無
        private static bool CONSOLE_WRITE_ON;
        // 演算子と優先度の定義
        private Dictionary<string, int> OPERATORS = new Dictionary<string, int> { { "(", 1 }, { ")", 2 }, { "+", 3 }, { "-", 4 }, { "*", 5 }, { "/", 6 } };

        // 演算子の優先度比較
        private bool IsLowerPriority(string op1, string op2)
        {
            return OPERATORS[op2] > OPERATORS[op1];
        }

        // 四則演算
        private int DoArithmethic(int val1, int val2, string op)
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
        private List<string> DivideByToken(string input)
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
        private int RPConv(List<string> tokens, ref List<string> rpcbuff)
        {
            int count = 0;
            rpcbuff = new List<string>();
            Stack<string> tmpbuff = new Stack<string>();
            string tmpstr;

            // 各種トークンの処理
            foreach(string token in tokens)
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
                    ShowStringList(rpcbuff);
                    Console.Write("TMP_BUFFER : ");
                    ShowStringStack(tmpbuff);
                }
            }
            // 残りのスタックをバッファへ追加
            while (tmpbuff.Count != 0)
                rpcbuff.Add(tmpbuff.Pop());

            return 0;
        }

        // 逆ポーランド記法に基づいて計算
        private int CalcRPFomula(List<string> rp)
        {
            int val, tmp1, tmp2, count = 0;
            Stack<int> buff = new Stack<int>();
            foreach(string str in rp)
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

                if (CONSOLE_WRITE_ON)
                {
                    Console.WriteLine(">> Phase {0} token is {1}", count, str);
                    Console.Write("BUFFER : ");
                    ShowStringStack(buff);
                }

            }
            // 最終的に残った値を格納、スタックに余計な物が残っている場合はエラーリターン(-1)
            val = (buff.Count == 1) ? buff.Pop() : -1;
            return val;
        }

        // stringリストのコンソール表示（Debug用）
        private void ShowStringList(List<string> list)
        {
            foreach (string str in list)
                Console.Write("{0}, ", str);
            Console.WriteLine("");
            return;
        }
        // stringスタックのコンソール表示（Debug用）
        private void ShowStringStack(Stack<string> stack)
        {
            Stack<string> copy_stack = new Stack<string>(stack);
            while(copy_stack.Count != 0)
                Console.Write("{0}, ", copy_stack.Pop());
            Console.WriteLine("");
            return;
        }
        // intスタックのコンソール表示（Debug用）
        private void ShowStringStack(Stack<int> stack)
        {
            Stack<int> copy_stack = new Stack<int>(stack);
            while (copy_stack.Count != 0)
                Console.Write("{0}, ", copy_stack.Pop());
            Console.WriteLine("");
            return;
        }

        // コンストラクタ
        public StringToFomula(bool debug_mode)
        {
            CONSOLE_WRITE_ON = debug_mode;
        }

        /// <summary>
        /// string形式の計算式の結果を返す
        /// </summary>
        /// <param name="input">入力文字列</param>
        /// <returns>結果</returns>
        public int OutValue(string input)
        {
            if(CONSOLE_WRITE_ON) Console.WriteLine("Input << {0}", input);

            List<string> tokens = DivideByToken(input);
            if (CONSOLE_WRITE_ON)
            {
                Console.Write("Divide by token >> ");
                ShowStringList(tokens);
            }

            int ret_state;
            List<string> rp = new List<string>();
            ret_state = RPConv(tokens, ref rp);
            if (ret_state == -1) return ret_state;
            if (CONSOLE_WRITE_ON)
            {
                Console.Write("Reverse Polish Convert >> ");
                ShowStringList(rp);
            }

            if (CONSOLE_WRITE_ON) Console.WriteLine("Calc Reverse Polish Fomula >> ");
            return CalcRPFomula(rp);
        }

        static void Main(string[] args)
        {
            do
            {
                string test_set1 = "10+20*(30-20)+50";
                string test_set2 = "((1 + 4) * (12 - 2)) / 5";
                string test_set3 = "3333";

                StringToFomula STF = new StringToFomula(true);
                int ret;
                ret = STF.OutValue(test_set3);

                Console.WriteLine("Finish!!\n Result is {0}", ret);

                //コンソールループ用
                Console.Write("End of Main Func (Push r for Retry）");
            } while (Console.ReadLine() == "r");
        }
    }
}
