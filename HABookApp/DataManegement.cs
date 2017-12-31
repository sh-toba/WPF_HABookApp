using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings;
using System.Collections;
using System.IO;
using ClosedXML.Excel;

namespace HABookApp
{

    /// <summary>
    /// 現金出費入力内容を扱うクラス
    /// </summary>
    public class InCashData
    {
        public string Date { get; } // 年月日
        public string Item { get; } // 費目
        public int Amount { get; } // 出費額
        public string Detail { get; } // 詳細内容
        // コンストラクタ
        public InCashData(string date, string item, int amount, string detail)
        {
            this.Date = date;
            this.Item = item;
            this.Amount = amount;
            this.Detail = detail;
        }
        public string StringConv()
        {
            return Date + "," + Item + "," + Amount.ToString() + "," + Detail;
        }
    }

    /// <summary>
    /// クレジット利用履歴を扱うクラス
    /// </summary>
    public class InCreditData
    {
        private static char DIVC = ':';
        public string Date { get; } // 利用年月日
        public string ExpItem { get; } // 費目
        public int Amount { get; } // 金額
        public string Detail { get; } // 詳細
        public string CapItem { get; } // 元手
        public string PayDate { get; set; } = "nofixed"; //支払い年月
        // コンストラクタ①
        public InCreditData(string date, string eitem, int amount, string detail, string citem)
        {
            this.Date = date;
            this.ExpItem = eitem;
            this.Amount = amount;
            this.Detail = detail;
            this.CapItem = citem;
        }
        // コンストラクタ②
        public InCreditData(string date, string eitem, int amount, string detail, string citem, string paydate)
        {
            this.Date = date;
            this.ExpItem = eitem;
            this.Amount = amount;
            this.Detail = detail;
            this.CapItem = citem;
            this.PayDate = paydate;
        }
        // コンストラクタ③
        public InCreditData(string data_str)
        {
            string[] words = data_str.Split(DIVC);
            this.Date = words[0];
            this.ExpItem = words[1];
            this.Amount = int.Parse(words[2]);
            this.Detail = words[3];
            this.CapItem = words[4];
            this.PayDate = words[5];
        }
        // 文字列に変換
        public string StringConv()
        {
            return Date + DIVC + ExpItem + DIVC + Amount.ToString() + DIVC + Detail + DIVC + CapItem + DIVC + PayDate;
        }
        // 文字列に変換
        public string StringConv2()
        {
            return Date + DIVC + ExpItem + DIVC + Amount.ToString() + DIVC + Detail + DIVC + CapItem;
        }
    }

    /// <summary>
    /// 口座入出金を扱うクラス
    /// </summary>
    public class InAccountData
    {
        private static char DIVC = ':';
        // Mode一覧：入力項目
        // 振替：Date, Account, Amount, ExpItem, Remarks, Fixflag 
        // 引落：Date, Account, Amount, Remarks, FixFlag (指定口座⇒現金)
        // 預入：Date, Account, Amount, Remarks, FixFlag（現金⇒指定口座）
        // 入金：Date, Account, Amount, Remarks, FixFlag

        // 必須入力項目
        public string AccInMode { get; } // 内容
        public string Date { get; set; } // 日付
        public string Account { get; } // 口座
        public int Amount { get; } = 0;// 金額
        // オプション
        public bool Fixflag { get; set; } = false; // 保護フラグ（毎月、同日同額の場合などtrueに）
        public string ExpItem { get; } = ""; // 費目
        public string Remarks { get; } = ""; // 備考

        // コンストラクタ①
        public InAccountData(string mode, string date, string account, int amount, string eitem, string remark, bool fixflag)
        {
            this.AccInMode = mode;
            this.Date = date;
            this.Account = account;
            this.Amount = amount;
            this.ExpItem = eitem;
            this.Remarks = remark;
            this.Fixflag = fixflag;
        }
        // コンストラクタ②
        public InAccountData(string data_str)
        {
            string[] words = data_str.Split(DIVC);
            this.AccInMode = words[0];
            this.Date = words[1];
            this.Account = words[2];
            this.Amount = int.Parse(words[3]);
            this.ExpItem = words[4];
            this.Remarks = words[5];
            this.Fixflag = bool.Parse(words[6]);
        }
        // 文字列に変換
        public string StringConv()
        {
            return AccInMode + DIVC + Date + DIVC + Account + DIVC + Amount.ToString() + DIVC + ExpItem + DIVC + Remarks + DIVC + Fixflag.ToString();
        }
        // 文字列に変換
        public string StringConv2()
        {
            return AccInMode + DIVC + Date + DIVC + Account + DIVC + Amount.ToString() + DIVC + Remarks + DIVC + ExpItem;
        }
    }

    /// <summary>
    /// 月単位の利用履歴を管理するクラス
    /// </summary>
    public class MonthData
    {
        public string Date { get; set; } // 年月
        public List<string> ExpItemList { get; set; } // 費目リスト
      
        // 各項目の合計値（費目ごと）
        public Dictionary<string, int> CashSum { get; set; } // ①現金利用
        public Dictionary<string, int> CreditSum { get; set; } // ②クレジット利用
        public Dictionary<string, int> AccountSum { get; set; } // ③口座振替
        public Dictionary<string, int> ExpenseSum { get; set; } // ①、②、③合計
        /// <summary>
        /// 指定した項目、費目の値を返す[opt：cash, credit, account, all]
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="item_key">費目</param>
        /// <returns></returns>
        public int GetItemSum(string opt, string item_key)
        {
            // どの項目かを決定
            Dictionary<string, int> tmpDict;
            switch (opt)
            {
                case "cash":
                    tmpDict = CashSum;
                    break;
                case "credit":
                    tmpDict = CreditSum;
                    break;
                case "account":
                    tmpDict = AccountSum;
                    break;
                case "all":
                    tmpDict = ExpenseSum;
                    break;
                default:
                    return -1;
            }
            // 値を取得
            if (tmpDict.ContainsKey(item_key))
                return tmpDict[item_key];
            else
                return 0;
        }

        public int TotalExpense { get; set; }
        public int Income { get; set; }
        public int Profit { get; set; }

        public List<string> CapitalList { get; set; } // 元手リスト
        public Dictionary<string, int> Balance { get; set; } // 元手値（月始め）
        public int TotalBalance { get; set; }

        public MonthData(string date, Dictionary<string, int> cashsum, Dictionary<string, int> creditsum, Dictionary<string, int> accsum, int income, Dictionary<string, int> balance)
        {
            // 代入
            this.Date = date;
            this.CashSum = new Dictionary<string, int>(cashsum);
            this.CreditSum = new Dictionary<string, int>(creditsum);
            this.AccountSum = new Dictionary<string, int>(accsum);
            this.Income = income;
            this.Balance = new Dictionary<string, int>(balance);

            // リスト取得
            this.ExpItemList = new List<string>();
            foreach (string key in this.CashSum.Keys)
                this.ExpItemList.Add(key);
            this.CapitalList = new List<string>();
            foreach (string key in this.Balance.Keys)
                this.CapitalList.Add(key);

            CalcSum();
        }

        public MonthData(MonthData data)
        {
            // 代入
            this.Date = data.Date;
            this.ExpItemList = new List<string>(data.ExpItemList);
            this.CashSum = new Dictionary<string, int>(data.CashSum);
            this.CreditSum = new Dictionary<string, int>(data.CreditSum);
            this.AccountSum = new Dictionary<string, int>(data.AccountSum);
            this.Income = data.Income;

            this.CapitalList = new List<string>(data.CapitalList);
            this.Balance = new Dictionary<string, int>(data.Balance);

            CalcSum();
        }

        // 他のMonthDataを結合する
        public void Plus(MonthData adddata)
        {
            MonthData copydata = new MonthData(this); // 自身のコピー

            this.Date = "dummy"; //dummy
            this.CashSum = new Dictionary<string, int>(CombineDictionary(copydata.CashSum, adddata.CashSum));
            this.CreditSum = new Dictionary<string, int>(CombineDictionary(copydata.CreditSum, adddata.CreditSum));
            this.AccountSum = new Dictionary<string, int>(CombineDictionary(copydata.AccountSum, adddata.AccountSum));
            int income = copydata.Income + adddata.Income;
            this.ExpItemList = new List<string>();
            foreach (KeyValuePair<string, int> pair in this.CashSum)
                this.ExpItemList.Add(pair.Key);

            this.Balance = new Dictionary<string, int>(CombineDictionary(copydata.Balance, adddata.Balance));
            this.CapitalList = new List<string>();
            foreach (string key in this.Balance.Keys)
                this.CapitalList.Add(key);

            CalcSum();

            return;
        }

        // Dictionaryの結合
        private Dictionary<string, int> CombineDictionary(Dictionary<string, int> dict1, Dictionary<string, int> dict2) 
        {
            Dictionary<string, int> outdict = new Dictionary<string, int>(dict1);
            foreach(KeyValuePair<string, int> pair in dict2)
            {
                if (!outdict.ContainsKey(pair.Key))
                    outdict.Add(pair.Key, pair.Value);
                else
                    outdict[pair.Key] += pair.Value;
            }
            return outdict;
        }

        // 合計値の計算
        private void CalcSum()
        {
            // 初期化
            this.ExpenseSum = new Dictionary<string, int>();
            foreach (string item_key in ExpItemList)
            {
                ExpenseSum.Add(item_key, 0);
            }
            this.TotalExpense = 0;
            this.TotalBalance = 0;

            // 収支計算
            foreach (KeyValuePair<string, int> pair in CashSum)
            {
                ExpenseSum[pair.Key] += pair.Value;
                TotalExpense += pair.Value;
            }
            foreach (KeyValuePair<string, int> pair in CreditSum)
            {
                ExpenseSum[pair.Key] += pair.Value;
                TotalExpense += pair.Value;
            }
            foreach (KeyValuePair<string, int> pair in AccountSum)
            {
                ExpenseSum[pair.Key] += pair.Value;
                TotalExpense += pair.Value;
            }
            this.Profit = Income - TotalExpense;

            foreach (KeyValuePair<string, int> pair in Balance)
                TotalBalance += pair.Value;

        }

        // Debug用コンソール出力
        public void OutConsole()
        {
            Console.WriteLine("-----MonthData[" + Date + "]-----");

            Console.WriteLine(">> ExpItemList");
            foreach (string st in ExpItemList)
                Console.Write(st + ",");
            Console.WriteLine("");

            Console.WriteLine(">> CashSum");
            foreach (string st in ExpItemList)
                Console.Write("{0},", CashSum[st]);
            Console.WriteLine("");

            Console.WriteLine(">> CreditSum");
            foreach (string st in ExpItemList)
                Console.Write("{0},", CreditSum[st]);
            Console.WriteLine("");

            Console.WriteLine(">> AccountSum");
            foreach (string st in ExpItemList)
                Console.Write("{0},", AccountSum[st]);
            Console.WriteLine("");

            Console.WriteLine(">> ExpenseSum");
            foreach (string st in ExpItemList)
                Console.Write("{0},", ExpenseSum[st]);
            Console.WriteLine("");

            Console.WriteLine(">> TotalExpense = {0}", TotalExpense);
            Console.WriteLine(">> Income = {0}", Income);
            Console.WriteLine(">> Profit = {0}", Profit);

            Console.WriteLine(">> CapitalList");
            foreach (string st in CapitalList)
                Console.Write(st + ",");
            Console.WriteLine("");

            Console.WriteLine(">> Balance");
            foreach (string st in CapitalList)
                Console.Write("{0},", Balance[st]);
            Console.WriteLine("");

            Console.WriteLine("----------END-----------");
            return;
        }


    }

    /// <summary>
    /// データを全体管理するクラス
    /// </summary>
    public class DataManagement
    {

        private const bool MODE_DEVELOP = false;

        /// <summary>
        /// クラス定義値
        /// </summary>
        private const string DATEFORM = "yyyy/MM/dd";
        private const string SYSLOGFILE = "system.log";
        private const string INITFILE = "setting.init";
        private const string CASHLOG = "_data_cash.log";
        private const string CREDITLOG = "_data_credit.log", CREDITLOG_PAID = "_credit_paid.log";
        private static string ACCOUNTLOG = "_data_account.log", ACCOUNTLOG_PAID = "_account_paid.log";
        // Excel周り
        private const string REPORTXLSX = "_expense.xlsx";
        private const string TEMPLATEXLSX = "template.xlsx";
        private const int ITEMRAW = 4, ITEMCOL = 2, DETAILCOL = 28, CASHRAW = 36, CREDITRAW = 38, ACCOUNTRAW = 40, CAPITEMRAW = 45;
        private const string INCOMECELL = "Z43";

        // FullPath
        public string PATH_USERS_ROOT { get; private set; } // rootディレクトリ
        private string PATH_SAVE = "data/";
        private string PATH_BACKUP = "backup/";

        // 動作記録用
        MyUtils.Logger DMLog; // ログ用クラス
        

        /// <summary>
        /// ①初期設定項目の管理
        /// </summary>
        // データ一覧
        public List<string> ExpItemList { get; private set; } = new List<string>(); // 費目リスト(食費、被服費、など)
        public List<string> CapItemList { get; private set; } = new List<string>(); // 元手リスト(現金、貯金口座、預金口座、など)
        public Dictionary<string, int> InitBalance { get; private set; } = new Dictionary<string, int>(); // 元手リストごとの初期値
        public List<string> AccInputItem { get; private set; } = new List<string> { "振替", "引落", "預入", "入金" }; // 口座管理データ入力項目
        private Dictionary<string, string> CreditPayDay = new Dictionary<string, string>(); // 支払い予定日
        // 操作
        public string StartDate { get; set; } // 運用開始年月
        public string ManageDate { get; set; } // 管理中の年月
        public string LatestDate { get; set; } // 最終更新時刻
        public DateTime bootDate { get; } = DateTime.Today; // DataManegementのインスタンス生成時刻
        public List<string> SelectiveDate; // 管理月中の日付一覧

        // 現在の管理付きの日付一覧を取得する
        private void SetSelectiveDate()
        {
            SelectiveDate = new List<string>();
            DateTime sdate = DateTime.Parse(ManageDate);
            DateTime edate = DateTime.Parse(ManageDate + "/" + DateTime.DaysInMonth(sdate.Year, sdate.Month));
            for (DateTime date = sdate; date <= edate; date = date.AddDays(1))
                SelectiveDate.Add(date.ToString(DATEFORM));
            return;
        }

        // 初期設定ファイルの読み込み
        private bool ReadInitFile()
        {
            string fname = PATH_USERS_ROOT + INITFILE;
            DMLog.WriteWithMethod(" << " + fname);
            try
            {
                using (StreamReader rfile = new StreamReader(fname, Encoding.GetEncoding("Shift_JIS")))
                {
                    string line = "";
                    // test.txtを1行ずつ読み込んでいき、末端(何もない行)までwhile文で繰り返す
                    int line_no = 0;
                    while ((line = rfile.ReadLine()) != null)
                    {
                        line_no++;
                        string[] words = line.Split(',');
                        switch (line_no)
                        {
                            case 1:
                                for (int i = 0; i < words.Length; i++) ExpItemList.Add(words[i]);
                                break;
                            case 2:
                                for (int count = 0; count < words.Length; count += 2)
                                {
                                    CapItemList.Add(words[count]);
                                    InitBalance.Add(words[count], int.Parse(words[count + 1]));
                                }
                                break;
                            case 3:
                                StartDate = words[0];
                                break;
                            case 4:
                                ManageDate = words[0];
                                break;
                            case 5:
                                LatestDate = words[0];
                                break;
                            case 6:
                                for (int count = 0; count < words.Length; count += 2)
                                {
                                    CreditPayDay.Add(words[count], words[count + 1]);
                                }
                                break;
                            case 7:
                                break;
                            default:
                                DMLog.Write("-----ERROR----- Initial Setting Data Reading");
                                return false;
                        }
                    }
                }
                return true;
            }
            catch (System.Exception e)
            {
                // ファイルを開くのに失敗したとき
                DMLog.Write("-----ERROR-----" + e.Message);
                return false;
            }
        }
        // 初期設定項目の保存
        private bool SaveInitFile()
        {
            string fname = PATH_USERS_ROOT + INITFILE;
            DMLog.WriteWithMethod(" << " + fname);
            char divc = ',';
            try
            {
                using (StreamWriter wfile = new StreamWriter(fname, false, Encoding.GetEncoding("Shift_JIS")))
                {
                    wfile.WriteLine(MyUtils.MyMethods.ListToStrLine(ExpItemList, divc));
                    wfile.WriteLine(MyUtils.MyMethods.DictionaryToStrLine(InitBalance, divc, divc));
                    wfile.WriteLine(StartDate);
                    wfile.WriteLine(ManageDate);
                    wfile.WriteLine(LatestDate);
                    wfile.WriteLine(MyUtils.MyMethods.DictionaryToStrLine(CreditPayDay, divc, divc));
                }
                return true;
            }
            catch (System.Exception e)
            {
                // ファイルを開くのに失敗したとき
                DMLog.Write("-----ERROR-----" + e.Message);
                return false;
            }
        }

        /// <summary>
        /// ②現金利用履歴の管理
        /// </summary>

        public Dictionary<string, Dictionary<string, int>> CashData { get; private set; } // 利用履歴（日付毎 -> 費目毎）
        public Dictionary<string, string> CashDataDetail { get; private set; } // 利用履歴詳細（日付毎 -> 費目毎）
        
        public void SetCashData(Dictionary<string, Dictionary<string, int>> data, Dictionary<string, string> data_detail)
        {
            CashData = new Dictionary<string, Dictionary<string, int>>(data);
            CashDataDetail = new Dictionary<string, string>(data_detail);
            SaveCashLog();
            return;
        }
        public int CashSum { get; private set; } = 0; // CashDateの総計
        private Dictionary<string, int> CashSumItem; // CashDateの総計

        private static char DIVCHAR = ':';

        // CashDataの内date_keyの日付のデータを文字列にして返す。※項目のない値はnullとする
        private string StrConvCashData(string date_key)
        {
            string line_str = "", detail = "";
            Dictionary<string, int> tmpDict;
            if (CashData.ContainsKey(date_key))
            {
                tmpDict = CashData[date_key];
                foreach(string item_key in ExpItemList)
                {
                    if (tmpDict.ContainsKey(item_key))
                        line_str += (tmpDict[item_key].ToString() + DIVCHAR);
                    else
                        line_str += ("null" + DIVCHAR);
                }
                detail = CashDataDetail[date_key];
            }
            return date_key + DIVCHAR + line_str + detail;
        }
        // 現金利用履歴を読み取り
        private void ReadCashLog()
        {
            // 保存先のデータを初期化
            CashData = new Dictionary<string, Dictionary<string, int>>();
            CashDataDetail = new Dictionary<string, string>();
  
            // オブジェクト設定
            string fname = PATH_SAVE + CASHLOG;
            Dictionary<int, string> items = new Dictionary<int, string>();
            Dictionary<string, int> tmpDict;

            // ログ取得
            DMLog.WriteWithMethod(" << " + fname);
            try
            {
                using (StreamReader rfile = new StreamReader(fname, Encoding.GetEncoding("Shift_JIS")))
                {
                    string line = "", date_str, detail;
                    int line_count = 0;
                    while ((line = rfile.ReadLine()) != null)
                    {
                        string[] words = line.Split(DIVCHAR);
                        line_count++;
                        if (line_count == 1)　// １行目は費目
                        {
                            for (int wl = 0; wl < words.Length; wl++)
                            {
                                items.Add(wl, words[wl]);
                            }
                        }
                        else
                        {
                            tmpDict = new Dictionary<string, int>();
                            date_str = words[0]; // 最初の項目は日付
                            for (int wl = 1; wl < words.Length-1; wl++)
                            {
                                if(words[wl]!="null")
                                    tmpDict.Add(items[wl-1], int.Parse(words[wl]));
                            }
                            if (words[words.Length - 1] != "null")
                                detail = words[words.Length - 1];
                            else
                                detail = "";

                            // データに追加
                            CashData.Add(date_str, tmpDict);
                            CashDataDetail.Add(date_str, detail);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                // ファイルを開くのに失敗したとき
                DMLog.Write("-----ERROR-----" + e.Message);
                return;
            }
            CashSumUpdate(); // 利用総額の更新
            return;
        }
        // 現金利用履歴を保存
        private void SaveCashLog()
        {
            // オブジェクト設定
            string fname = PATH_SAVE + CASHLOG;
            // ログ取得
            DMLog.WriteWithMethod(" << " + fname);
            try
            {
                using (StreamWriter wfile = new StreamWriter(fname, false, Encoding.GetEncoding("Shift_JIS")))
                {
                    wfile.WriteLine(MyUtils.MyMethods.ListToStrLine(ExpItemList, DIVCHAR));
                    foreach (string date in CashData.Keys)
                        wfile.WriteLine(StrConvCashData(date));
                }
            }
            catch (System.Exception e)
            {
                // ファイルを開くのに失敗したとき
                DMLog.Write("-----ERROR-----" + e.Message);
                return;
            }
            CashSumUpdate(); // 利用総額の更新
            return;
        }
        // 任意の月のCashDataを削除
        private void DeleteCashData()
        {
            DMLog.WriteWithMethod("");
            DateTime date;
            List<string> date_keys = new List<string>(CashData.Keys);
            foreach (string ref_date_str in date_keys)
            {
                date = DateTime.Parse(ref_date_str);
                if (date.ToString("yyyy/MM") == ManageDate)
                {
                    CashData.Remove(ref_date_str);
                    CashDataDetail.Remove(ref_date_str);
                }
            }
            SaveCashLog();
            return;
        }

        // 管理月の利用総額の更新
        private void CashSumUpdate()
        {
            // 初期化
            CashSum = 0;
            CashSumItem = new Dictionary<string, int>();
            // 合計値計算
            foreach (string date_key in CashData.Keys)
            {
                DateTime date = DateTime.Parse(date_key);
                if (date.ToString("yyyy/MM") == ManageDate) // 管理月判定
                {
                    foreach (string item_key in CashData[date_key].Keys)
                    {
                        int value = CashData[date_key][item_key];
                        CashSum += value;
                        if (CashSumItem.ContainsKey(item_key))
                            CashSumItem[item_key] += value;
                        else
                            CashSumItem.Add(item_key, value);
                    }
                }
            }
            return;
        }

        // Cashデータに入力項目を加える
        public void AddCashData(List<InCashData> adddatalist)
        {
            // 変更前データの保存
            //tmpCashData = new Dictionary<string, Dictionary<string, int>>(CashData);
            //tmpCashDataDetail = new Dictionary<string, string>(CashDataDetail);

            DMLog.WriteWithMethod("");
            string datekey, itemkey;
            foreach (InCashData adddata in adddatalist)
            {
                datekey = adddata.Date; // 日付キー
                itemkey = adddata.Item; // 費目キー
                if (CashData.ContainsKey(datekey)) //日付キーの存在チェック
                {
                    if (CashData[datekey].ContainsKey(itemkey)) // 費目キーの存在チェック
                    {
                        CashData[datekey][itemkey] += adddata.Amount;
                        CashDataDetail[datekey] += (adddata.Detail + ", ");
                    }
                    else
                    {
                        CashData[datekey].Add(itemkey, adddata.Amount);
                        CashDataDetail[datekey] += (adddata.Detail + ", ");
                    }
                }
                else
                {
                    CashData.Add(datekey, new Dictionary<string, int>() { { itemkey, adddata.Amount } });
                    CashDataDetail.Add(datekey, adddata.Detail + ", ");
                }
                DMLog.Write("->" + adddata.StringConv());
            }
            SaveCashLog();
            return;
        }


        /// <summary>
        /// ③クレジット利用履歴の管理
        /// </summary>
        public List<InCreditData> CreditData { get; private set; } // クレジット管理データ
        public void SetCreditData(List<InCreditData> data) { CreditData = new List<InCreditData>(data); SaveCreditLog(); return; }

        private Dictionary<string, int> CreditSumItem; // 管理月利用額：key=費目
        private Dictionary<string, int> CreditSumConfirm; // 支払い確定総額：key=元手
        private Dictionary<string, int> CreditSumAll; // クレジット利用総額：key=元手

        // クレジット利用履歴を読み取る
        private void ReadCreditLog()
        {
            string fname = PATH_SAVE + CREDITLOG;
            DMLog.WriteWithMethod(" << " + fname);
            try
            {
                using (StreamReader rfile = new StreamReader(fname, Encoding.GetEncoding("Shift_JIS")))
                {
                    CreditData = new List<InCreditData>();
                    string line = "";
                    // test.txtを1行ずつ読み込んでいき、末端(何もない行)までwhile文で繰り返す
                    while ((line = rfile.ReadLine()) != null)
                    {
                        CreditData.Add(new InCreditData(line));
                    }
                }
            }
            catch (System.Exception e)
            {
                // ファイルを開くのに失敗したとき
                DMLog.Write("-----ERROR-----" + e.Message);
                return;
            }
            CreditSumUpdate(); // 利用総額の更新
            return;
        }
        // クレジット利用履歴の保存
        private void SaveCreditLog()
        {
            DMLog.WriteWithMethod("");
            // ログファイルに新規書き込み
            try
            {
                string fname = PATH_SAVE + CREDITLOG;
                DMLog.Write("-> Open Log File << " + fname);
                using (StreamWriter wfile = new StreamWriter(fname, false, Encoding.GetEncoding("Shift_JIS")))
                {
                    foreach (InCreditData data in CreditData)
                        wfile.WriteLine(data.StringConv());
                }
            }
            catch (System.Exception e)
            {
                // ファイルを開くのに失敗したとき
                DMLog.Write("-----ERROR-----" + e.Message);
                return;
            }
            CreditSumUpdate(); // 利用総額の更新
            return;
        }
        
        // 利用額のアップデート
        private void CreditSumUpdate()
        {
            CalcCreditSum();
            CalcCreditSumItem();
            return;
        }

        // 支払い確定額および総額の計算
        private void CalcCreditSum()
        {
            // 初期化処理
            CreditSumAll = new Dictionary<string, int>();
            CreditSumConfirm = new Dictionary<string, int>();
            foreach (string key in CapItemList)
            {
                if (key != "現金")
                {
                    CreditSumAll.Add(key, 0);
                    CreditSumConfirm.Add(key, 0);
                }
            }
            // 合計値の計算
            foreach (InCreditData data in CreditData)
            {
                CreditSumAll[data.CapItem] += data.Amount;
                if (data.PayDate == ManageDate)
                    CreditSumConfirm[data.CapItem] += data.Amount;
            }
        }
        // 管理月中の項目別合計値を返す
        private void CalcCreditSumItem()
        {
            CreditSumItem = new Dictionary<string, int>();
            foreach (InCreditData data in CreditData)
            {
                DateTime date = DateTime.Parse(data.Date);
                if (date.ToString("yyyy/MM") == ManageDate)
                {
                    if (CreditSumItem.ContainsKey(data.ExpItem))
                        CreditSumItem[data.ExpItem] += data.Amount;
                    else
                        CreditSumItem.Add(data.ExpItem, data.Amount);
                }
            }
            return;
        }

        // 条件を指定して合計値返却
        public int CalcCreditSum(string c_usemonth, string c_acc, string c_item, string c_confmonth)
        {
            if(c_usemonth != "")
                c_usemonth = (DateTime.Parse(c_usemonth)).ToString("yyyy/MM");
            if (c_confmonth != "")
                c_confmonth = (DateTime.Parse(c_confmonth)).ToString("yyyy/MM");

            bool[] conditions = {true, true, true, true};
            int sum = 0;
            foreach (InCreditData data in CreditData)
            {
                // 条件判定
                DateTime date = DateTime.Parse(data.Date);
                if (c_usemonth != "")
                    conditions[0] = (date.ToString("yyyy/MM") == c_usemonth);
                else
                    conditions[0] = true;
                if (c_acc != "")
                    conditions[1] = (data.CapItem == c_acc);
                else
                    conditions[1] = true;
                if (c_item != "")
                    conditions[2] = (data.ExpItem == c_item);
                else
                    conditions[2] = true;
                if (c_confmonth != "")
                    conditions[3] = (data.PayDate == c_confmonth);
                else
                    conditions[3] = true;

                // 合計値計算
                if (conditions[0] && conditions[1] && conditions[2] && conditions[3])
                    sum += data.Amount;
            }
            return sum;
        }

        // 日付でソート
        public void SortCreditData()
        {
            // ソート用にデータ取得
            Dictionary<string, List<InCreditData>> org_data = new Dictionary<string, List<InCreditData>>();
            HashSet<string> date_list_h = new HashSet<string>();
            foreach (InCreditData icdata in CreditData)
            {
                string date_str = icdata.Date;       
                // 日付をハッシュリストに追加
                date_list_h.Add(icdata.Date);
                // 日付キーがなければ空のリストを追加
                if (!org_data.ContainsKey(date_str))
                    org_data.Add(date_str, new List<InCreditData>());
                // データ追加
                org_data[date_str].Add(icdata);
            }
            // 日付リストの並び替え
            List<string> date_list = new List<string>(date_list_h);
            date_list.Sort();
            // ソート
            List<InCreditData> sorted_data = new List<InCreditData>();
            foreach (string ref_date in date_list)
                foreach (InCreditData ref_icdata in org_data[ref_date])
                    sorted_data.Add(ref_icdata);
            // データ更新
            SetCreditData(sorted_data);
            // ログ保存
            SaveCreditLog();
            return;
        }

        // 利用履歴の追加
        public void AddCreditData(List<InCreditData> data_list)
        {
            foreach(InCreditData data in data_list)
                CreditData.Add(data);
            SaveCreditLog();
            return;
        }
        // 指定Noの支払い予定月の状態を変更する(no：履歴番号※1始まり)
        public bool ChangeCreditPayDate(List<int> no_list)
        {
            // エラーチェック
            foreach (int no in no_list)
            {
                if ((1 > no) || (no > CreditData.Count))
                {
                    DMLog.Write("-----ERROR-----Invalid CreditData Number !!");
                    return false;
                }
            }
            // 支払日変更
            foreach (int no in no_list)
            {
                if(CreditData[no - 1].PayDate == "nofixed")
                    CreditData[no - 1].PayDate = ManageDate;
                else
                    CreditData[no - 1].PayDate = "nofixed";
            }
            SaveCreditLog();
            return true;
        }
        
        // 指定Noのデータを削除する
        public bool RemoveCreditData(List<int> no_list)
        {
            // エラーチェック
            foreach (int no in no_list)
            {
                if ((0 > no) || (no >= CreditData.Count))
                {
                    DMLog.Write("-----ERROR-----Invalid CreditData Number !!");
                    return false;
                }
            }
            // 削除
            no_list.Sort((a, b) => b - a);
            foreach (int no in no_list)
                CreditData.RemoveAt(no);

            SaveCreditLog();
            return true;
        }
        // 支払い確定処理_クレジット履歴
        public void FixCreditData()
        {
            DMLog.WriteWithMethod("");
            // 今月支払い分があれば総額をAccountDataに渡す
            List<InAccountData> adddata_list = new List<InAccountData>();
            foreach (string key in CapItemList)
            {
                if ((key != "現金") && (CreditSumConfirm[key] != 0))
                {
                    adddata_list.Add(new InAccountData("振替", ManageDate + "/" + CreditPayDay[key], key, CreditSumConfirm[key],"","クレジット", false));
                }
            }
            AddAccountData(adddata_list);

            string year_str, fname;
            Dictionary<string, List<string>> buffer = new Dictionary<string, List<string>>();
            DMLog.Write(">> remove comfirm credit data");
            try
            {
                // 支払い済み項目を記録して削除
                for (int i = CreditData.Count - 1; i >= 0; i--)
                {
                    if (CreditData[i].PayDate == ManageDate) // 支払い月=管理中の月なら削除
                    {
                        year_str = (DateTime.Parse(CreditData[i].Date)).ToString("yyyy");
                        if (buffer.ContainsKey(year_str))
                            buffer[year_str].Add(CreditData[i].StringConv());
                        else
                        {
                            buffer.Add(year_str, new List<string> { CreditData[i].StringConv()});
                        }
                        CreditData.RemoveAt(i);
                    }
                }
                foreach (string year_key in buffer.Keys)
                {
                    fname = PATH_SAVE + year_key + CREDITLOG_PAID;
                    DMLog.Write("-> Open Log File << " + fname);
                    using (StreamWriter wfile = new StreamWriter(fname, true, Encoding.GetEncoding("Shift_JIS")))
                    {
                        for(int i = buffer[year_key].Count - 1 ; i >= 0 ; i--)
                            wfile.WriteLine(buffer[year_key][i]);
                    }
                }
            }
            catch (System.Exception e)
            {
                // ファイルを開くのに失敗したとき
                DMLog.Write("-----ERROR-----" + e.Message);
                return;
            }
            SaveCreditLog();
            return;
        }


        /// <summary>
        /// ④口座利用履歴の管理
        /// </summary>

        public List<InAccountData> AccountData { get; private set; } // 口座引き落とし管理データ
        public void SetAccountData(List<InAccountData> data) { AccountData = new List<InAccountData>(data); SaveAccountLog(); return; }

        private Dictionary<string, int> AccountSum; // 現時点までの総和
        private Dictionary<string, int> AccountSumItem; // 管理月費目ごとの総和
        private Dictionary<string, int> AccountSumAll; // 全総和
        private int AccountSumIncome; // 入金額の総和

        // クレジット利用履歴を読み取る
        private void ReadAccountLog()
        {
            string fname = PATH_SAVE + ACCOUNTLOG;
            DMLog.WriteWithMethod(" << " + fname);
            try
            {
                using (StreamReader rfile = new StreamReader(fname, Encoding.GetEncoding("Shift_JIS")))
                {
                    AccountData = new List<InAccountData>();
                    string line = "";
                    // test.txtを1行ずつ読み込んでいき、末端(何もない行)までwhile文で繰り返す
                    while ((line = rfile.ReadLine()) != null)
                    {
                        AccountData.Add(new InAccountData(line));
                    }
                }
            }
            catch (System.Exception e)
            {
                // ファイルを開くのに失敗したとき
                DMLog.Write("-----ERROR-----" + e.Message);
                return;
            }
            AccountSumUpdate(); // 利用総額の更新
            return;
        }
        // クレジット利用履歴の保存
        private void SaveAccountLog()
        {

            DMLog.WriteWithMethod("");
            // ログファイルに新規書き込み
            try
            {
                string fname = PATH_SAVE + ACCOUNTLOG;
                DMLog.Write("-> Open Log File << " + fname);
                using (StreamWriter wfile = new StreamWriter(fname, false, Encoding.GetEncoding("Shift_JIS")))
                {
                    foreach (InAccountData data in AccountData)
                        wfile.WriteLine(data.StringConv());
                }
            }
            catch (System.Exception e)
            {
                // ファイルを開くのに失敗したとき
                DMLog.Write("-----ERROR-----" + e.Message);
                return;
            }
            AccountSumUpdate(); // 利用総額の更新
            return;
        }
        // 指定Noのデータを削除する
        public bool RemoveAccountData(List<int> no_list)
        {
            // エラーチェック
            foreach (int no in no_list)
            {
                if ((0 > no) || (no >= AccountData.Count))
                {
                    DMLog.Write("-----ERROR-----Invalid AccountData Number !!");
                    return false;
                }
            }
            // 削除
            no_list.Sort((a, b) => b - a);
            foreach (int no in no_list)
                AccountData.RemoveAt(no);

            SaveAccountLog();
            return true;
        }
        // 支払い済み項目の削除
        private void RemovePaidAccountData()
        {
            DMLog.WriteWithMethod("");
            // 支払い済み項目を記録して削除
            string year_str, fname;
            Dictionary<string, List<string>> buffer = new Dictionary<string, List<string>>();
            
            try
            {
                for (int i = AccountData.Count - 1; i >= 0; i--)
                {
                    InAccountData data = AccountData[i];
                    DateTime date = DateTime.Parse(data.Date);
                    if (date.ToString("yyyy/MM") == ManageDate)  // 支払日が管理中の月なら削除
                    {
                        year_str = date.ToString("yyyy");
                        if (buffer.ContainsKey(year_str))
                            buffer[year_str].Add(data.StringConv());
                        else
                        {
                            buffer.Add(year_str, new List<string> { data.StringConv() });
                        }
                        // 固定項目ならひと月進める、でなければ削除
                        if (data.Fixflag)
                        {
                            AccountData[i].Date = date.AddMonths(1).ToString(DATEFORM);
                        }
                        else
                        {
                            AccountData.RemoveAt(i);
                        }
                    }
                }
                foreach (string year_key in buffer.Keys)
                {
                    fname = PATH_SAVE + year_key + ACCOUNTLOG_PAID;
                    DMLog.Write("-> Open Log File << " + fname);
                    using (StreamWriter wfile = new StreamWriter(fname, true, Encoding.GetEncoding("Shift_JIS")))
                    {
                        for (int i = buffer[year_key].Count - 1; i >= 0; i--)
                            wfile.WriteLine(buffer[year_key][i]);
                    }
                }
            }
            catch (System.Exception e)
            {
                // ファイルを開くのに失敗したとき
                DMLog.Write("-----ERROR-----" + e.Message);
                return;
            }
            SaveAccountLog();
            return;
        }

        // 利用額のアップデート
        private void AccountSumUpdate()
        {
            CalcAccountSum();
            CalcAccountSumItem();
            CalcAccountSumIncome();
            return;
        }

        // 支払い確定額および総額の計算
        private void CalcAccountSum()
        {
            // 初期化処理
            AccountSum = new Dictionary<string, int>();
            AccountSumAll = new Dictionary<string, int>();
            foreach (string key in CapItemList)
            {
                AccountSumAll.Add(key, 0);
                AccountSum.Add(key, 0);
            }
            // 合計値の計算
            string acc;
            int datecheck, value;
            bool datecheckb;
            foreach (InAccountData data in AccountData)
            {
                acc = data.Account;
                datecheck = (data.Date).CompareTo(bootDate.ToString(DATEFORM));
                datecheckb = (datecheck == -1) || (datecheck == 0);
                value = data.Amount;
                switch (data.AccInMode)
                {
                    case "振替": // 口座から差し引く
                        AccountSumAll[acc] -= value;
                        if (datecheckb) AccountSum[acc] -= value;
                        break;
                    case "引落": // 口座から差し引き, 現金に足す
                        AccountSumAll[acc] -= value;
                        AccountSumAll["現金"] += value;
                        if (datecheckb)
                        {
                            AccountSum[acc] -= value;
                            AccountSum["現金"] += value;
                        }
                        break;
                    case "預入": // 現金から引き, 口座に足す
                        AccountSumAll[acc] += value;
                        AccountSumAll["現金"] -= value;
                        if (datecheckb)
                        {
                            AccountSum[acc] += value;
                            AccountSum["現金"] -= value;
                        }
                        break;
                    case "入金": // 口座に足すのみ
                        AccountSumAll[acc] += value;
                        if (datecheckb) AccountSum[acc] += value;
                        break;
                    default:
                        DMLog.Write("-----ERROR-----Calc Account Sum... refering AccountData");
                        return;
                }
            }
        }
        // 管理月中の項目別合計値を計算
        private void CalcAccountSumItem()
        {
            AccountSumItem = new Dictionary<string, int>();
            foreach (InAccountData data in AccountData)
            {
                if ((data.AccInMode == "振替") && (data.ExpItem != ""))
                {
                    DateTime date = DateTime.Parse(data.Date);
                    if (date.ToString("yyyy/MM") == ManageDate)
                    {
                        if (AccountSumItem.ContainsKey(data.ExpItem))
                            AccountSumItem[data.ExpItem] += data.Amount;
                        else
                            AccountSumItem.Add(data.ExpItem, data.Amount);
                    }
                }
            }
            return;
        }
        // 管理月中の入金合計値を計算
        private void CalcAccountSumIncome()
        {
            AccountSumIncome = 0;
            foreach (InAccountData data in AccountData)
            {
                if (data.AccInMode == "入金")
                {
                    DateTime date = DateTime.Parse(data.Date);
                    if (date.ToString("yyyy/MM") == ManageDate)
                    {
                        AccountSumIncome += data.Amount;
                    }
                }
            }
            return;
        }

        // 条件を指定して合計値返却
        public int CalcAccountSum(string c_mode, string c_usemonth, string c_acc, string c_item, string c_paid)
        {
            if (c_usemonth != "")
                c_usemonth = (DateTime.Parse(c_usemonth)).ToString("yyyy/MM");

            bool[] conditions = { true, true, true, true, true };
            int sum = 0;
            foreach (InAccountData data in AccountData)
            {
                // 条件判定
                if (c_mode != "")
                    conditions[0] = (data.AccInMode == c_mode);
                else
                    conditions[0] = true;
                DateTime date = DateTime.Parse(data.Date);
                if (c_usemonth != "")
                    conditions[1] = (date.ToString("yyyy/MM") == c_usemonth);
                else
                    conditions[1] = true;
                if (c_acc != "")
                    conditions[2] = (data.Account == c_acc);
                else
                    conditions[2] = true;
                if (c_item != "")
                    conditions[3] = (data.ExpItem == c_item);
                else
                    conditions[3] = true;
                if (c_paid != "")
                {
                    int datecheck = (data.Date).CompareTo(bootDate.ToString(DATEFORM));
                    if(c_paid == "nopaid")
                        conditions[4] = (datecheck == 1);
                    else if(c_paid == "paid")
                    {
                        conditions[4] = (datecheck == 0 || datecheck == -1);
                    }
                }
                else
                    conditions[4] = true;

                // 合計値計算
                if (conditions[0] && conditions[1] && conditions[2] && conditions[3] && conditions[4])
                    sum += data.Amount;
            }
            return sum;
        }

        // 日付でソート
        public void SortAccountData()
        {
            // ソート用にデータ取得
            Dictionary<string, List<InAccountData>> org_data = new Dictionary<string, List<InAccountData>>();
            HashSet<string> date_list_h = new HashSet<string>();
            foreach (InAccountData iadata in AccountData)
            {
                string date_str = iadata.Date;
                // 日付をハッシュリストに追加
                date_list_h.Add(iadata.Date);
                // 日付キーがなければ空のリストを追加
                if (!org_data.ContainsKey(date_str))
                    org_data.Add(date_str, new List<InAccountData>());
                // データ追加
                org_data[date_str].Add(iadata);
            }
            // 日付リストの並び替え
            List<string> date_list = new List<string>(date_list_h);
            date_list.Sort();
            // ソート
            List<InAccountData> sorted_data = new List<InAccountData>();
            foreach (string ref_date in date_list)
                foreach (InAccountData ref_iadata in org_data[ref_date])
                    sorted_data.Add(ref_iadata);
            // データ更新
            SetAccountData(sorted_data);
            // ログ保存
            SaveAccountLog();
            return;
        }

        // 利用履歴の追加
        public void AddAccountData(List<InAccountData> adddatalist)
        {
            foreach(InAccountData data in adddatalist)
                AccountData.Add(data);
            SaveAccountLog();
            return;
        }

        // 任意の元手(=key)の現在の残金を計算して返す
        public int GetNowBalance(string key)
        {
            //DMLog.WriteWithMethod(" << key = " + key);
            int nbalance;
            if (CapItemList.Contains(key))
            {
                // 初期値から口座入出金の現在までの総和を足す
                nbalance = InitBalance[key] + AccountSum[key];
                // 現金ならば現金利用履歴の総和を引く
                if (key == "現金")
                    nbalance -= CashSum;
                //DMLog.Write(">> return " + nbalance.ToString());
                return nbalance;
            }
            else
            {
                //DMLog.Write("-----ERROR----- Get Now Balance...Invalid Capital Item...key=" + key);
                return -1;
            }
        }

        // 費目ごとの合計値を返す
        public Dictionary<string, int> GetItemsSum(string option)
        {
            Dictionary<string, int> sums = new Dictionary<string, int>();
            switch (option)
            {
                case "cash":
                    return CashSumItem;
                case "credit":
                    return CreditSumItem;
                case "account":
                    return AccountSumItem;
                case "total":
                    // 現金使用分をコピー
                    sums = new Dictionary<string, int>(CashSumItem);
                    // クレジット使用分を足す
                    foreach (KeyValuePair<string, int> pair in CreditSumItem)
                    {
                        if (sums.ContainsKey(pair.Key))
                            sums[pair.Key] += pair.Value;
                        else
                            sums.Add(pair.Key, pair.Value);
                    }
                    // 口座利用分を足す
                    foreach (KeyValuePair<string, int> pair in AccountSumItem)
                    {
                        if (sums.ContainsKey(pair.Key))
                            sums[pair.Key] += pair.Value;
                        else
                            sums.Add(pair.Key, pair.Value);
                    }
                    return sums;
                default:
                    return sums;
            }
        }



        // 管理月以前のデータを読み込む
        public Dictionary<string, MonthData> LoadMonthData()
        {
            DMLog.WriteWithMethod("");

            // 格納用変数定義
            Dictionary<string, MonthData> retData = new Dictionary<string, MonthData>();

            // 読み込み年月、記録開始月～記録済み月
            DateTime sdate = DateTime.Parse(StartDate);
            DateTime edate = DateTime.Parse(ManageDate).AddMonths(-1);

            string fname;
            string celval, date_str;
            int celval_i;

            fname = PATH_SAVE + sdate.ToString("yyyy") + REPORTXLSX;
            DMLog.Write("-> Open Excel File << " + fname);
            var book = new XLWorkbook(fname, XLEventTracking.Disabled);
            bool openflag = false;

            // 読み込み開始
            for (DateTime date = sdate; date <= edate; date = date.AddMonths(1))
            {
                date_str = date.ToString("yyyy/MM");

                // 年が変わった場合のみ新規ブックを開く
                if (openflag)
                {
                    fname = PATH_SAVE + date.ToString("yyyy") + REPORTXLSX;
                    DMLog.Write("-> Open Excel File << " + fname);
                    book = new XLWorkbook(fname, XLEventTracking.Disabled);
                    openflag = false;
                }

                // シートを開く
                if (MODE_DEVELOP) DMLog.Write("->  Open Sheet << " + date.Month.ToString() + "月");
                var sheet = book.Worksheet(date.Month.ToString() + "月");

                // 費目を読み込む
                if (MODE_DEVELOP) DMLog.Write("-> Load Item");
                List<string> itemlist = new List<string>();
                Dictionary<string, int> itemindex = new Dictionary<string, int>();
                int count = ITEMCOL;
                while (true)
                {
                    celval = sheet.Cell(MyUtils.MyMethods.GetCellName(ITEMRAW, count)).GetString();
                    if (celval == "")
                    {
                        if (MODE_DEVELOP) DMLog.Write("--> " + (count-1).ToString() + " items");
                        break;
                    }
                    itemlist.Add(celval);
                    itemindex.Add(celval, count);
                    count++;
                }

                // Cash履歴を読み込む
                if (MODE_DEVELOP) DMLog.Write("-> Load Cash");
                Dictionary<string, int> cashsum = new Dictionary<string, int>();
                foreach (KeyValuePair<string, int> pair in itemindex)
                {
                    celval = sheet.Cell(MyUtils.MyMethods.GetCellName(CASHRAW, pair.Value)).GetString();
                    celval_i = (celval == "") ? 0 : int.Parse(celval);
                    cashsum.Add(pair.Key, celval_i);
                }

                // Credit履歴を読み込む
                if (MODE_DEVELOP) DMLog.Write("-> Load Credit");
                Dictionary<string, int> creditsum = new Dictionary<string, int>();
                foreach (KeyValuePair<string, int> pair in itemindex)
                {
                    celval = sheet.Cell(MyUtils.MyMethods.GetCellName(CREDITRAW, pair.Value)).GetString();
                    celval_i = (celval == "") ? 0 : int.Parse(celval);
                    creditsum.Add(pair.Key, celval_i);
                }
                // Account履歴を読み込む
                if (MODE_DEVELOP) DMLog.Write("-> Load Account");
                Dictionary<string, int> accsum = new Dictionary<string, int>();
                foreach (KeyValuePair<string, int> pair in itemindex)
                {
                    celval = sheet.Cell(MyUtils.MyMethods.GetCellName(ACCOUNTRAW, pair.Value)).GetString();
                    celval_i = (celval == "") ? 0 : int.Parse(celval);
                    accsum.Add(pair.Key, celval_i);
                }
                // Incomeを読み込む
                if (MODE_DEVELOP) DMLog.Write("-> Load Income");
                celval = sheet.Cell(INCOMECELL).GetString();
                int income = (celval == "") ? 0 : int.Parse(celval);

                // 月初めの残金を読み込む
                if (MODE_DEVELOP) DMLog.Write("-> Load Balance");
                Dictionary<string, int> balance = new Dictionary<string, int>();
                count = ITEMCOL;
                while (true)
                {
                    string key = sheet.Cell(MyUtils.MyMethods.GetCellName(CAPITEMRAW, count)).GetString();
                    if (key == "")
                    {
                        if (MODE_DEVELOP) DMLog.Write("--> " + (count - 1).ToString() + " items");
                        break;
                    }
                    celval = sheet.Cell(MyUtils.MyMethods.GetCellName(CAPITEMRAW + 1, count)).GetString();
                    celval_i = (celval == "") ? 0 : int.Parse(celval);
                    balance.Add(key, celval_i);

                    count++;
                }

                // MonthData型にして追加
                if (MODE_DEVELOP) DMLog.Write("-> Add MonthData");
                retData.Add(date_str, new MonthData(date_str, cashsum, creditsum, accsum, income, balance));

                if (date.Month == 12) openflag = true;
            }
            DMLog.WriteWithMethod(" Completed ");
            return retData;
        }

        // 管理月のデータをExcelファイルに保存（月末Fix処理用, 読み込みおよび参照専用）
        private bool FixSaveXLSX()
        {
            DMLog.WriteWithMethod("");

            // 引数をDateTimeクラスに変換
            DateTime fix_date = DateTime.Parse(ManageDate);
            string fix_year = fix_date.ToString("yyyy");
            string fix_month = (fix_date.Month).ToString() + "月";
            string fname = PATH_SAVE + fix_year + REPORTXLSX;

            DMLog.Write("-> Save Data << " + ManageDate);
            using (var book = new XLWorkbook(fname, XLEventTracking.Disabled))
            {
                try
                {
                    // Excelファイルを開く
                    DMLog.Write("-> Open Excel File << " + fname);
                    var sheet = book.Worksheet(fix_month);

                    // 費目を書き込む
                    Dictionary<string, int> itemindex = new Dictionary<string, int>();
                    int count = 0;
                    foreach (string eitem in ExpItemList)
                    {
                        itemindex.Add(eitem, count + ITEMCOL);
                        sheet.Cell(MyUtils.MyMethods.GetCellName(ITEMRAW, count + ITEMCOL)).Value = eitem;
                        count++;
                    }

                    // CashDataとCashDataDetailを書き込む
                    DMLog.Write("-> WriteCash ");
                    DateTime date;
                    int day;
                    foreach (string date_str in CashData.Keys)
                    {
                        date = DateTime.Parse(date_str);
                        if (date.ToString("yyyy/MM") == ManageDate)
                        {
                            day = date.Day;
                            // 費目ごとにセルへ書き込み
                            foreach (KeyValuePair<string, int> pair in CashData[date_str])
                                sheet.Cell(MyUtils.MyMethods.GetCellName(day + ITEMRAW, itemindex[pair.Key])).Value = pair.Value;
                            // 詳細の書き込み
                            sheet.Cell(MyUtils.MyMethods.GetCellName((day + ITEMRAW), DETAILCOL)).Value = CashDataDetail[date_str];
                        }
                    }

                    // クレジット利用履歴を書き込む
                    DMLog.Write("-> WriteCredit");
                    foreach (KeyValuePair<string, int> pair in CreditSumItem)
                        sheet.Cell(MyUtils.MyMethods.GetCellName(CREDITRAW, itemindex[pair.Key])).Value = pair.Value;

                    // 口座振替履歴の書き込み
                    DMLog.Write("-> WriteAccount");
                    foreach (KeyValuePair<string, int> pair in AccountSumItem)
                        sheet.Cell(MyUtils.MyMethods.GetCellName(ACCOUNTRAW, itemindex[pair.Key])).Value = pair.Value;

                    // 入金額の書き込み
                    DMLog.Write("-> WriteIncome");
                    sheet.Cell(INCOMECELL).Value = AccountSumIncome;

                    // 初期残金を書き込む
                    DMLog.Write("-> WriteBalance");
                    count = 0;
                    foreach (string citem in CapItemList)
                    {
                        sheet.Cell(MyUtils.MyMethods.GetCellName(CAPITEMRAW, count + ITEMCOL)).Value = citem;
                        sheet.Cell(MyUtils.MyMethods.GetCellName(CAPITEMRAW+1, count + ITEMCOL)).Value = InitBalance[citem];
                        count++;
                    }

                    book.Save();
                    //book.SaveAs(PATH_SAVE + REPORTXLSX + fix_year + "_test.xlsx");
                    DMLog.Write("-> WorkBook is Saved");
        
                    return true;

                }
                catch
                {
                    DMLog.WriteWithMethod("-----Exception!!!----");
                    return false;
                }
            }
        }

        // 新規年度のXLSXファイル作成
        private bool CreateNewXLSX(int year)
        {
            string fname = TEMPLATEXLSX;
            string fname_new = PATH_SAVE + year.ToString() + REPORTXLSX;
            DMLog.WriteWithMethod(" << " + fname_new);
            using (var book = new XLWorkbook(fname, XLEventTracking.Disabled))
            {
                try
                {
                    book.SaveAs(fname_new);
                    DMLog.Write("-> WorkBook is Saved");
                    return true;
                }
                catch
                {
                    DMLog.WriteWithMethod("-----Exception!!!----");
                    return false;
                }
            }
        }

        // Backupの保存
        public void CreateBackup()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string folname = PATH_BACKUP + timestamp;
            MyUtils.MyMethods.CopyDirectory(PATH_SAVE, folname, true);
            DMLog.WriteWithMethod(" >> " + folname);
            return;
        }

        // 月末処理
        public int FixEndofMonth()
        {
            DMLog.WriteWithMethod("");

            // Excelファイルに保存
            if (!FixSaveXLSX())
                return 1;

            // クレジット支払いの確定
            FixCreditData();
            // 初期値の更新
            foreach (string cap_key in CapItemList)
            {
                InitBalance[cap_key] = GetNowBalance(cap_key);
            }
            // 現金利用履歴の削除
            DeleteCashData();
            // 口座利用履歴の削除
            RemovePaidAccountData();
            // 管理月をひと月進める
            DateTime mdate = DateTime.Parse(ManageDate);
            if (mdate.Month == 12) // 年末なら新規のXLSXファイル作成
                CreateNewXLSX(mdate.Year + 1);
            ManageDate = mdate.AddMonths(1).ToString("yyyy/MM");
            SetSelectiveDate();

            // 初期設定項目の更新
            SaveInitFile();
            DMLog.Refresh();

            return 0;
        }


        // 初期化処理
        public void Init(string root_dir_path)
        {

            PATH_USERS_ROOT = @root_dir_path;

            // 出力先パスの設定
            PATH_SAVE = PATH_USERS_ROOT + PATH_SAVE;
            PATH_BACKUP = PATH_USERS_ROOT + PATH_BACKUP;
            
            // ログファイル設定
            DMLog = new MyUtils.Logger(SYSLOGFILE);
            DMLog.WriteWithMethod("");

            // 初期設定ファイルの読み込み
            ReadInitFile();

            // 選択可能日付リストの取得
            SetSelectiveDate();

            // 各種管理データの読み込み
            ReadCashLog(); // 現金利用履歴
            ReadCreditLog(); // クレジット利用履歴
            ReadAccountLog(); // 口座利用履歴

            return;
        }

        // コンストラクタ
        public DataManagement()
        {
        }

        /// <summary>
        /// デバッグ用コンソール出力
        /// </summary>
        // 初期設定情報の表示（デバッグ用）
        public void ShowInitInfo()
        {
            Console.WriteLine("費目リスト：");
            foreach (string key in ExpItemList) Console.Write("{0}, ", key);
            Console.Write("\n");
            Console.WriteLine("元手リスト：");
            foreach (string key in CapItemList) Console.Write("{0}, ", key);
            Console.Write("\n");
            Console.WriteLine("管理中の月：{0}", ManageDate);
            Console.WriteLine("初期残高：");
            foreach (string key in InitBalance.Keys) Console.Write("{0}={1}, ", key, InitBalance[key]);
            Console.WriteLine("\n");
        }
        public void ShowCashData()
        {
            Console.WriteLine("<< Show CashData !!>>");
            ExtCashDate(ExpItemList);
            Console.WriteLine("\n");
        }
        public void ShowCreditData()
        {
            Console.WriteLine("<< Show CreditData !!>>");
            int N = CreditData.Count;
            Console.WriteLine("ALL {0} Numbers of data", N);
            for (int n = 0; n < N; n++)
            {
                Console.Write("No.{0} -> ", (n+1).ToString("D2"));
                Console.WriteLine(CreditData[n].StringConv());
            }
            Console.WriteLine("\n");
        }
        public void ShowAccountData()
        {
            Console.WriteLine("<< Show AccountData !!>>");
            int N = AccountData.Count;
            Console.WriteLine("ALL {0} Numbers of data", N);
            for (int n = 0; n < N; n++)
            {
                Console.Write("No.{0} -> ", (n + 1).ToString("D2"));
                Console.WriteLine(AccountData[n].StringConv());
            }
            Console.WriteLine("\n");
        }
        public void ShowAllNowBalance()
        {
            Console.WriteLine("<< Show All NowBalance !!>>");
            foreach (string cap_item in CapItemList)
            {
                Console.Write("{0}={1}, ", cap_item, GetNowBalance(cap_item));
            }
            Console.WriteLine("\n");
        }

        // 保存中のCashDataから任意の項目の履歴と合計を表示
        public Dictionary<string, int> ExtCashDate(List<string> keys)
        {
            Dictionary<string, int> sums = new Dictionary<string, int>();
            int ramount;

            Console.Write("Search Key: ");
            foreach (string key in keys)
            {
                Console.Write("{0}, ", key);
                sums.Add(key, 0);
            }
            Console.WriteLine("");

            foreach (string date_str in CashData.Keys)
            {
                Console.Write("{0}: ", date_str);
                foreach (string key in keys)
                {
                    ramount = (CashData[date_str].ContainsKey(key)) ? CashData[date_str][key] : 0;
                    Console.Write("{0}, ", ramount);
                    sums[key] += ramount;
                }
                Console.WriteLine("");
            }

            Console.Write("Sums: ");
            foreach (string key in keys)
            {
                Console.Write("{0}, ", sums[key]);
            }
            Console.WriteLine("");

            return sums;
        }

    }

}
