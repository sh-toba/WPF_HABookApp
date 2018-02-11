using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Collections.ObjectModel;
using System.Data;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace HABookApp
{
    public class HABAppViewModel
    {
        // 動作記録用
        public MyUtils.Logger VMLog; // ログ用クラス

        private static string SYSLOGFILE = "system.log";
        private const int MAX_CHACHE = 20;

        // Binding用データ
        public ReactiveProperty<string> NowDate { get; set; } = new ReactiveProperty<string>(); // アプリ起動日
        public ReactiveProperty<List<string>> ExpItemList { get; set; } = new ReactiveProperty<List<string>>(); // 費目リスト
        public ReactiveProperty<List<string>> CapItemList { get; set; } = new ReactiveProperty<List<string>>(); // 元手リスト
        public ReactiveProperty<List<string>> AccountList { get; set; } = new ReactiveProperty<List<string>>(); // 口座リスト
        public ReactiveProperty<List<string>> CardList { get; set; } = new ReactiveProperty<List<string>>(); // クレジットカードリスト
        public ReactiveProperty<List<string>> SelectiveDate { get; set; } = new ReactiveProperty<List<string>>(); // 管理月の日付リスト
        public ReactiveProperty<List<string>> AccInputItem { get; set; } = new ReactiveProperty<List<string>>(); // 口座取引種リスト
        public ReactiveProperty<Dictionary<string, Dictionary<string, int>>> CashData { get; set; } = new ReactiveProperty<Dictionary<string, Dictionary<string, int>>>(); // 現金利用履歴
        public ReactiveProperty<DataTable> CashDataView { get; set; } = new ReactiveProperty<DataTable>(); // 現金利用履歴（表用）
        public ReactiveProperty<Dictionary<string, string>> CashDataDetail { get; set; } = new ReactiveProperty<Dictionary<string, string>>(); // 現金利用詳細
        public ReactiveProperty<List<InCreditDataView>> CreditData { get; set; } = new ReactiveProperty<List<InCreditDataView>>(); // クレジット利用
        public ReactiveProperty<List<InAccountDataView>> AccountData { get; set; } = new ReactiveProperty<List<InAccountDataView>>(); // 口座利用

        // Undo用のキャッシュ
        public ReactiveProperty<bool> CashDataCacheState { get; set; } = new ReactiveProperty<bool>() { Value = false };
        public ReactiveProperty<bool> CreditDataCacheState { get; set; } = new ReactiveProperty<bool>() { Value = false };
        public ReactiveProperty<bool> AccountDataCacheState { get; set; } = new ReactiveProperty<bool>() { Value = false };

        public ReactiveProperty<List<CapitalDataView>> CapitalDataList { get; set; } = new ReactiveProperty<List<CapitalDataView>>();
        public ReactiveProperty<int> TotalExpense { get; set; } = new ReactiveProperty<int>();
        public ReactiveProperty<int> TotalIncome { get; set; } = new ReactiveProperty<int>();
        public ReactiveProperty<int> Profit { get; set; } = new ReactiveProperty<int>();

        // 予算設定画面
        public ReactiveProperty<List<BinderableNameValue>> ExpensesBudgetList { get; set; } = new ReactiveProperty<List<BinderableNameValue>>();
        public ReactiveProperty<List<BinderableNameValue>> IncomesBudgetList { get; set; } = new ReactiveProperty<List<BinderableNameValue>>();
        public ReactiveProperty<int> TotalExpenseBudget { get; set; } = new ReactiveProperty<int>();
        public ReactiveProperty<int> TotalIncomeBudget { get; set; } = new ReactiveProperty<int>();
        public ReactiveProperty<int> ProfitBudget { get; set; } = new ReactiveProperty<int>();

        // 合計値情報のアップデート
        public void UpdateBudgetSum()
        {
            TotalExpenseBudget.Value = GetExpeseBudgetSum();
            TotalIncomeBudget.Value = GetIncomeBudgetSum();
            ProfitBudget.Value = TotalIncomeBudget.Value - TotalExpenseBudget.Value;
        }
        // 出費の合計値を返す
        private int GetExpeseBudgetSum()
        {
            int val, sum=0;
            foreach (BinderableNameValue bnv in ExpensesBudgetList.Value)
            {
                if (!int.TryParse(bnv.Value.Value, out val)) return -1;
                sum += val;
            }
            return sum;
        }
        // 収入の合計値を返す
        private int GetIncomeBudgetSum()
        {
            int val, sum = 0;
            foreach (BinderableNameValue bnv in IncomesBudgetList.Value)
            {
                if (!int.TryParse(bnv.Value.Value, out val)) return -1;
                sum += val;
            }
            return sum;
        }

        // 入力内容のチェック
        public bool CheckSetBudget()
        {
            bool retb = true;
            int val;
            foreach (BinderableNameValue bnv in ExpensesBudgetList.Value)
                if (!int.TryParse(bnv.Value.Value, out val)) retb = false;
            foreach (BinderableNameValue bnv in IncomesBudgetList.Value)
                if (!int.TryParse(bnv.Value.Value, out val)) retb = false;

            return retb;
        }


        // Dictionaryをバインド用のオブジェクトに変換
        public List<BinderableNameValue> BNVConvert(Dictionary<string, int> dict)
        {
            List<BinderableNameValue> retlist = new List<BinderableNameValue>();
            foreach (KeyValuePair<string, int> pair in dict)
                retlist.Add(new BinderableNameValue(pair.Key, (pair.Value).ToString()));

            return retlist;
        }
        // 逆変換
        public Dictionary<string, int> ConvertEBLList()
        {
            Dictionary<string, int> retdict = new Dictionary<string, int>();
            foreach (BinderableNameValue bnv in ExpensesBudgetList.Value)
            {
                int val;
                if (!int.TryParse(bnv.Value.Value, out val)) return retdict;
                retdict.Add(bnv.Name.Value, val);
            }

            return retdict;
        }
        public Dictionary<string, int> ConvertIBLList()
        {
            Dictionary<string, int> retdict = new Dictionary<string, int>();
            foreach (BinderableNameValue bnv in IncomesBudgetList.Value)
            {
                int val;
                if (!int.TryParse(bnv.Value.Value, out val)) return retdict;
                retdict.Add(bnv.Name.Value, val);
            }

            return retdict;
        }

        // キャッシュデータ(Cash)
        private List<Dictionary<string, Dictionary<string, int>>> CacheCashData = new List<Dictionary<string, Dictionary<string, int>>>();
        private List<Dictionary<string, string>> CacheCashDataDetail = new List<Dictionary<string, string>>();
        public void AddCacheCashData()
        {
            if (CacheCashData.Count == MAX_CHACHE)
            {
                CacheCashData.RemoveAt(0);
                CacheCashDataDetail.RemoveAt(0);
            }
            CacheCashData.Add(new Dictionary<string, Dictionary<string, int>>(CashData.Value));
            CacheCashDataDetail.Add(new Dictionary<string, string>(CashDataDetail.Value));
            CashDataCacheState.Value = true;
        }
        public void LoadLatestCacheCashData()
        {
            CashData.Value = new Dictionary<string, Dictionary<string, int>>(CacheCashData.Last());
            CacheCashData.RemoveAt(CacheCashData.Count - 1);
            CashDataDetail.Value = new Dictionary<string, string>(CacheCashDataDetail.Last());
            CacheCashDataDetail.RemoveAt(CacheCashDataDetail.Count - 1);
            if (CacheCashDataDetail.Count == 0)
                CashDataCacheState.Value = false;
        }
        // キャッシュデータ(Credit)
        private List<List<InCreditDataView>> CacheCreditData = new List<List<InCreditDataView>>();
        public void AddCacheCreditData() {
            if (CacheCreditData.Count == MAX_CHACHE)
                CacheCreditData.RemoveAt(0);
            CacheCreditData.Add(new List<InCreditDataView>(CreditData.Value));
            CreditDataCacheState.Value = true;
        }
        public void LoadLatestCacheCreditData()
        {
            CreditData.Value = new List<InCreditDataView>(CacheCreditData.Last());
            CacheCreditData.RemoveAt(CacheCreditData.Count - 1);
            if (CacheCreditData.Count == 0)
                CreditDataCacheState.Value = false;
        }
        // キャッシュデータ(Account)
        private List<List<InAccountDataView>> CacheAccountData = new List<List<InAccountDataView>>();
        public void AddCacheAccountData()
        {
            if (CacheAccountData.Count == MAX_CHACHE)
                CacheAccountData.RemoveAt(0);
            CacheAccountData.Add(new List<InAccountDataView>(AccountData.Value));
            AccountDataCacheState.Value = true;
        }
        public void LoadLatestCacheAccountData()
        {
            AccountData.Value = new List<InAccountDataView>(CacheAccountData.Last());
            CacheAccountData.RemoveAt(CacheAccountData.Count - 1);
            if (CacheAccountData.Count == 0)
                AccountDataCacheState.Value = false;
        }


        // 口座リストを取得する
        public void GetAccountList()
        {
            List<string> list = new List<string>(CapItemList.Value);
            list.RemoveAt(0);
            AccountList.Value = new List<string>(list);
        }

        // DataManegementからCreditDataを受け取ってView用のCreditDataに変換
        public void ConvertCreditDataFromDM(List<InCreditData> CreditDataDM)
        {
            int no = 0;
            CreditData.Value = new List<InCreditDataView>();
            string mmonth = (DateTime.Parse(NowDate.Value)).ToString("yyyy/MM");
            foreach (InCreditData data in CreditDataDM)
            {
                no++;
                CreditData.Value.Add(new InCreditDataView(no, data, mmonth));
            }
            return;
        }
        // View用のCreditDataをDataManegement型に変換
        public List<InCreditData> ConvertCreditDataToDM(string mmonth)
        {
            List<InCreditData> CreditDataDM = new List<InCreditData>();
            foreach (InCreditDataView data in CreditData.Value)
            {
                if (data.IsChecked.Value)
                    CreditDataDM.Add(new InCreditData(data.Date.Value, data.ExpItem.Value, data.Amount.Value, data.Detail.Value, data.CardName.Value, mmonth));
                else
                    CreditDataDM.Add(new InCreditData(data.Date.Value, data.ExpItem.Value, data.Amount.Value, data.Detail.Value, data.CardName.Value));
            }
            return CreditDataDM;
        }


        // DataManegementからAccountDataを受け取ってView用のCreditDataに変換
        public void ConvertAccountDataFromDM(List<InAccountData> AccountDataDM)
        {
            int no = 0;
            AccountData.Value = new List<InAccountDataView>();
            foreach (InAccountData data in AccountDataDM)
            {
                no++;
                AccountData.Value.Add(new InAccountDataView(no, data));
            }
            return;
        }
        // View用のAccountDataをDataManegement型に変換
        public List<InAccountData> ConvertAccountDataToDM()
        {
            List<InAccountData> AccountDataDM = new List<InAccountData>();
            foreach (InAccountDataView data in AccountData.Value)
                AccountDataDM.Add(new InAccountData(data.AccInMode.Value, data.Date.Value, data.Account.Value, data.Amount.Value, data.ExpItem.Value, data.Remarks.Value, data.FixFlag.Value));
            return AccountDataDM;
        }


        // 初期化
        public void Init()
        {
            // 初期設定ファイルの読み込み
            VMLog = new MyUtils.Logger(SYSLOGFILE);
            VMLog.WriteWithMethod("");

            return;
        }

        // コンストラクタ
        public HABAppViewModel() { }

        // デバッグ用
        public void ShowCreditData()
        {
            Console.WriteLine("-----CreditDataView-----");
            foreach (InCreditDataView data in CreditData.Value)
            {
                Console.WriteLine("入力{0}::金額={1}::支払い確定={2}", data.Number.Value, data.Amount.Value, data.IsChecked.Value);
            }
            Console.WriteLine("-----------------------");
            return;
        }


        public ReactiveProperty<List<BarGraphItem>> BarGraphItems { get; set; } = new ReactiveProperty<List<BarGraphItem>>();

        public void ConvertBarGraphItems(Dictionary<string, int> data)
        {
            List<BarGraphItem> items = new List<BarGraphItem>();
            foreach (KeyValuePair<string, int> pair in data)
                items.Add(new BarGraphItem(pair.Key, pair.Value));
            BarGraphItems.Value = new List<BarGraphItem>(items);
            return;
        }

        public void ConvertCashDataForView()
        {
            // 保持データ読み込み
            Dictionary<string, Dictionary<string, int>> cdata = CashData.Value;
            Dictionary<string, string> cdata_detail = CashDataDetail.Value;

            // 数値入力のある費目の抜き出し
            HashSet<string> ext_date = new HashSet<string>();
            Dictionary<string, bool> item_hist = new Dictionary<string, bool>(); // 入力有無判定用
            foreach (string item_key in ExpItemList.Value) item_hist.Add(item_key, false);
            foreach (string date_key in SelectiveDate.Value)
            {
                if (cdata.ContainsKey(date_key))
                {
                    ext_date.Add(date_key);
                    foreach (string item_key in ExpItemList.Value)
                    {
                        if (cdata[date_key].ContainsKey(item_key))
                            item_hist[item_key] = true;
                    }
                }
            }

            // データテーブルに書き込み(ヘッダ)
            DataTable data_tab = new DataTable();
            data_tab.Columns.Add("RowHeader");
            HashSet<string> ext_item = new HashSet<string>();
            foreach (KeyValuePair<string, bool> pair in item_hist)
            {
                if (pair.Value)
                {
                    ext_item.Add(pair.Key);
                    data_tab.Columns.Add(pair.Key);
                }
            }
            data_tab.Columns.Add("詳細");

            // データテーブルに書き込み(データ本体)
            int row_count;
            foreach (string date in ext_date)
            {
                var row = data_tab.NewRow();
                row[0] = (DateTime.Parse(date)).ToString("MM/dd");
                row_count = 1;
                foreach (string item in ext_item)
                {
                    row[row_count] = (cdata[date].ContainsKey(item)) ? cdata[date][item].ToString() : "";
                    row_count++;
                }
                row[row_count] = cdata_detail[date];
                data_tab.Rows.Add(row);
            }

            // データ反映
            CashDataView.Value = new DataTable();
            CashDataView.Value = data_tab;

            return;
        }

        /// <summary>
        /// Mainタブのグラフ描画
        /// </summary>
        //private static double REGVAL = 1000;
        private static int STROKE_THICKNESS = 1, FONT_SIZE = 14;
        private static List<OxyColor> COLORS { get; } = new List<OxyColor> { OxyColors.Green, OxyColors.Red, OxyColors.Blue, OxyColors.LightCyan, OxyColors.Magenta, OxyColors.Brown, OxyColors.DarkOrange, OxyColors.DarkOrchid, OxyColors.SteelBlue, OxyColors.Tan, OxyColors.LightSalmon, OxyColors.Salmon, OxyColors.Goldenrod, OxyColors.MediumSpringGreen, OxyColors.Maroon, OxyColors.PaleVioletRed };
        public List<string> MainTabGraphMenu { get; } = new List<string> {"総計", "現金利用", "クレジット利用", "口座振替" }; 
        public ReactiveProperty<PlotModel> MyPlotModel { get; set; } = new ReactiveProperty<PlotModel>();

        // 棒グラフ用アイテム
        private class MainTabBarGraphItem
        {
            public string Label;
            public int Expense;
            public int Budget;

            public MainTabBarGraphItem(string s, int exp, int bud)
            {
                Label = s;
                Expense = exp;
                Budget = bud;
            }
        }
        private List<string> BGLabels = new List<string>();
        private List<MainTabBarGraphItem> BGItems = new List<MainTabBarGraphItem>();

        public int DrawGraph(string opt, Dictionary<string, Dictionary<string, int>> data)
        {

            try
            {
                // 全体のモデル定義
                var model = new PlotModel()
                {
                    LegendPlacement = LegendPlacement.Outside,
                    LegendPosition = LegendPosition.RightMiddle,
                    LegendOrientation = LegendOrientation.Vertical,
                    LegendBorderThickness = 0,
                    LegendFontSize = FONT_SIZE + 2
                };

                // データをチェックして、LedgendとLabelのリストを取得する。
                List<string> grouplist = new List<string>();
                List<string> labellist = new List<string>();
                Dictionary<string, bool> showdata = new Dictionary<string, bool>();
                foreach (string item in ExpItemList.Value)
                    showdata.Add(item, false);
                foreach (KeyValuePair<string, Dictionary<string, int>> pair in data)
                {
                    grouplist.Add(pair.Key);
                    foreach (KeyValuePair<string, int> pair2 in pair.Value)
                        if (pair2.Value != 0) showdata[pair2.Key] = true;
                }
                foreach (string item in ExpItemList.Value)
                    if (showdata[item]) labellist.Add(item);

                // X軸の設定
                var categoryAxis = new CategoryAxis
                {
                    Position = AxisPosition.Bottom,
                    FontSize = FONT_SIZE
                };
                foreach (string item in labellist)
                    categoryAxis.Labels.Add(item);
                model.Axes.Add(categoryAxis);

                // Y軸の設定
                var valueAxis = new LinearAxis
                {
                    Position = AxisPosition.Left,
                    FontSize = FONT_SIZE,
                    Title = "金額",
                    Unit = "円",
                    TickStyle = TickStyle.Inside,
                    StringFormat = "0",
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dot,
                    Minimum = 0,
                    Maximum = 80000
                };
                model.Axes.Add(valueAxis);

                // データの追加
                foreach (KeyValuePair<string, Dictionary<string, int>> pair in data)
                {
                    var bs = new ColumnSeries { Title = pair.Key, StrokeColor = OxyColors.Black, StrokeThickness = STROKE_THICKNESS, BaseValue = 1};
                    foreach (string item in labellist)
                    {
                        int val = (pair.Value.ContainsKey(item)) ? pair.Value[item] : 0;
                        bs.Items.Add(new ColumnItem { Value = val });
                    }
                    model.Series.Add(bs);
                }

                MyPlotModel.Value = model;

                return 0;
            }
            catch
            {
                return 2;
            }
        }
    }


    public class InCreditDataView
    {
        public ReactiveProperty<int> Number { get; set; } = new ReactiveProperty<int>();
        public ReactiveProperty<string> Date { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> ExpItem { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<int> Amount { get; set; } = new ReactiveProperty<int>();
        public ReactiveProperty<string> Detail { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> CardName { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<bool> IsChecked { get; set; } = new ReactiveProperty<bool>();

        public InCreditDataView(int no, InCreditData data, string mmonth)
        {
            Number.Value = no;
            Date.Value = data.Date;
            ExpItem.Value = data.ExpItem;
            Amount.Value = data.Amount;
            Detail.Value = data.Detail;
            CardName.Value = data.CardName;
            if (data.PayDate == mmonth)
                IsChecked.Value = true;
            else
                IsChecked.Value = false;
        }

        public string ConvStr()
        {
            string confirm;
            if (IsChecked.Value)
                confirm = "支払い確定済み";
            else
                confirm = "支払い未確定";

            return "管理No." + Number.Value.ToString() + ":" + Amount.Value.ToString() + ":" + Detail.Value + ":" + CardName.Value + ":" + confirm;
        }

    }

    public class InAccountDataView
    {
        public ReactiveProperty<int> Number { get; set; } = new ReactiveProperty<int>();
        public ReactiveProperty<string> AccInMode { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> Date { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> Account { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<int> Amount { get; set; } = new ReactiveProperty<int>();
        public ReactiveProperty<string> ExpItem { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<bool> FixFlag { get; set; } = new ReactiveProperty<bool>();
        public ReactiveProperty<string> Remarks { get; set; } = new ReactiveProperty<string>();

        public ReactiveProperty<SolidColorBrush> FontColor { get; set; } = new ReactiveProperty<SolidColorBrush>();

        public InAccountDataView(int no, InAccountData data)
        {
            Number.Value = no;
            AccInMode.Value = data.AccInMode;
            Date.Value = data.Date;
            Account.Value = data.Account;

            //switch (data.AccInMode) {
            //    case "振替":
            //    Amount.Value = -data.Amount;
            //    FontColor.Value = new SolidColorBrush(Colors.Red);
            //        break;
            //    case "入金":
            //    Amount.Value = data.Amount;
            //    FontColor.Value = new SolidColorBrush(Colors.Blue);
            //        break;
            //    case "預入":
            //        Amount.Value = data.Amount;
            //        FontColor.Value = new SolidColorBrush(Colors.Black);
            //        break;
            //    case "引落":
            //        Amount.Value = -data.Amount;
            //        FontColor.Value = new SolidColorBrush(Colors.Black);
            //        break;
            //}
            switch (data.AccInMode)
            {
                case "振替":
                case "引落":
                    Amount.Value = data.Amount;
                    FontColor.Value = new SolidColorBrush(Colors.Red);
                    break;
                case "入金":
                case "預入":
                    Amount.Value = data.Amount;
                    FontColor.Value = new SolidColorBrush(Colors.Blue);
                    break;
            }
            ExpItem.Value = data.ExpItem;
            FixFlag.Value = data.Fixflag;
            Remarks.Value = data.Remarks;
        }

    }

    public class CapitalDataView {

        public ReactiveProperty<string> CapName { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<int> InitBalance { get; set; } = new ReactiveProperty<int>();
        public ReactiveProperty<int> NowBalance { get; set; } = new ReactiveProperty<int>();
        public ReactiveProperty<int> PredictBalance { get; set; } = new ReactiveProperty<int>();

        public ReactiveProperty<Dictionary<string, int>> ItemExpence { get; set; } = new ReactiveProperty<Dictionary<string, int>>();

        public CapitalDataView(string cname, int ib, int nb, int pb, Dictionary<string, int> itexp)
        {
            CapName.Value = cname;
            InitBalance.Value = ib;
            NowBalance.Value = nb;
            PredictBalance.Value = pb;
            ItemExpence.Value = new Dictionary<string, int>(itexp);

        }
    }



    public class BarGraphItem
    {
        public ReactiveProperty<string> Label { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<int> Expense { get; set; } = new ReactiveProperty<int>();
        public BarGraphItem(string lab, int val)
        {
            this.Label.Value = lab;
            this.Expense.Value = val;
        }
    }

    // GraphView用のViewModelクラス
    public class GraphViewModel
    {

        private class DrawData
        {
            public List<DataPoint> PointList;
            public bool AccumMode; // 累積モード

            public DrawData(bool mode)
            {
                AccumMode = mode;
                PointList = new List<DataPoint>();
            }

            public DrawData(DrawData data)
            {
                AccumMode = data.AccumMode;
                PointList = new List<DataPoint>(data.PointList);
            }

            public void AddPoint(DateTime date, double value)
            {
                // 累積モードなら前の値に足して格納
                if (PointList.Count >= 1 && AccumMode)
                    PointList.Add(new DataPoint(DateTimeAxis.ToDouble(date), PointList.Last().Y + value));
                else
                    PointList.Add(new DataPoint(DateTimeAxis.ToDouble(date), value));
            }

            public void Stack(DrawData stdata)
            {
                List<DataPoint> tmp = new List<DataPoint>();
                if (this.PointList.Count == stdata.PointList.Count)
                {
                    for (int i = 0; i < this.PointList.Count; i++)
                        tmp.Add(new DataPoint(this.PointList[i].X, this.PointList[i].Y + stdata.PointList[i].Y));
                    this.PointList = new List<DataPoint>(tmp);
                }
            }
        }

        // 描画用データクラス
        Dictionary<string, DrawData> DrawDataList;

        // グラフ描画設定値
        private static double REGVAL = 1000;
        private static int STROKE_THICKNESS = 2, FONT_SIZE = 14;
        //private static string MARKER_TYPE = "Circle";
        private static List<OxyColor> COLORS { get; } = new List<OxyColor> { OxyColors.Green, OxyColors.Red, OxyColors.Blue, OxyColors.LightCyan, OxyColors.Magenta, OxyColors.Brown, OxyColors.DarkOrange, OxyColors.DarkOrchid, OxyColors.SteelBlue, OxyColors.Tan, OxyColors.LightSalmon, OxyColors.Salmon, OxyColors.Goldenrod, OxyColors.MediumSpringGreen, OxyColors.Maroon, OxyColors.PaleVioletRed };

        // 表示項目設定
        private static List<string> CONTENTS = new List<string> { "収支", "収益", "費目", "所持金", "所持金（総額）" };
        private static List<string> VIEWLEVEL = new List<string> { "月毎", "年毎", "全て" };

        public Dictionary<string, Dictionary<string, MonthData>> LoadedData { get; set; } = new Dictionary<string, Dictionary<string, MonthData>>();
        public HashSet<string> ExpItemList = new HashSet<string>();
        public HashSet<string> CapitalList = new HashSet<string>();
        public HashSet<string> LoadedDateListY = new HashSet<string>();
        public Dictionary<string, HashSet<string>> LoadedDateListM = new Dictionary<string, HashSet<string>>();

        public ReactiveProperty<List<string>> ContentList { get; set; } = new ReactiveProperty<List<string>>();
        public ReactiveProperty<List<string>> ViewLevelList { get; set; } = new ReactiveProperty<List<string>>();
        public ReactiveProperty<List<string>> YearList { get; set; } = new ReactiveProperty<List<string>>();
        public ReactiveProperty<bool> AccumMode { get; set; } = new ReactiveProperty<bool>();
        public ReactiveProperty<bool> StackMode { get; set; } = new ReactiveProperty<bool>();

        public ReactiveProperty<PlotModel> MyPlotModel { get; set; } = new ReactiveProperty<PlotModel>();

        // 初期化
        public bool Init(Dictionary<string, MonthData> ldata)
        {
            ContentList.Value = new List<string>(CONTENTS);
            ViewLevelList.Value = new List<string>(VIEWLEVEL);
            AccumMode.Value = false;
            StackMode.Value = false;

            try
            {
                foreach (KeyValuePair<string, MonthData> pair in ldata)
                {
                    DateTime date = DateTime.Parse(pair.Key);
                    string ystr = date.ToString("yyyy年");
                    string mstr = date.ToString("MM月");
                    if (!LoadedDateListM.ContainsKey(ystr))
                    {
                        LoadedDateListM.Add(ystr, new HashSet<string>());
                        LoadedData.Add(ystr, new Dictionary<string, MonthData>());
                    }
                    LoadedDateListY.Add(ystr);
                    LoadedDateListM[ystr].Add(mstr);
                    LoadedData[ystr].Add(mstr, new MonthData(pair.Value));

                    foreach (string item in pair.Value.ExpItemList)
                        ExpItemList.Add(item);

                    foreach (string item in pair.Value.CapitalList)
                        CapitalList.Add(item);
                }
                YearList.Value = new List<string>(LoadedDateListY);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // コンボボックスの内容設定
        public void SetList_Yaer(string vtype)
        {
            if (vtype == "月毎")
                YearList.Value = new List<string>(LoadedDateListY);
            else
                YearList.Value = new List<string>();
        }

        // 表示用データ計算
        private bool CreateDataList(string ctype, string vtype, string year)
        {
            try
            {
                Dictionary<DateTime, MonthData> refdata = new Dictionary<DateTime, MonthData>(); //参照データ
                switch (vtype)
                {
                    case "月毎": // 月毎の場合、指定年度のデータを受け取る
                        refdata = new Dictionary<DateTime, MonthData>();
                        foreach (KeyValuePair<string, MonthData> pair in LoadedData[year])
                            refdata.Add(DateTime.Parse(year + pair.Key), new MonthData(pair.Value));
                        break;
                    case "年毎": // 年毎の場合、年毎にデータを結合する
                        foreach (KeyValuePair<string, Dictionary<string, MonthData>> rpair in LoadedData)
                        {
                            MonthData tmpdata = new MonthData("dummy", new Dictionary<string, int>(), new Dictionary<string, int>(), new Dictionary<string, int>(), 0, new Dictionary<string, int>());
                            foreach (KeyValuePair<string, MonthData> pair in rpair.Value)
                                tmpdata.Plus(pair.Value);
                            refdata.Add(DateTime.Parse(rpair.Key), new MonthData(tmpdata));
                        }
                        break;
                    case "全て":
                        foreach (KeyValuePair<string, Dictionary<string, MonthData>> rpair in LoadedData)
                            foreach (KeyValuePair<string, MonthData> pair in rpair.Value)
                                refdata.Add(DateTime.Parse(rpair.Key + pair.Key), new MonthData(pair.Value));
                        break;
                    default:
                        return false;
                }

                // 表示用データとして格納
                DrawDataList = new Dictionary<string, DrawData>();
                switch (ctype)
                {
                    case "収支":
                        DrawDataList.Add("収入", new DrawData(AccumMode.Value));
                        DrawDataList.Add("支出", new DrawData(AccumMode.Value));
                        foreach (KeyValuePair<DateTime, MonthData> pair in refdata)
                        {
                            DrawDataList["収入"].AddPoint(pair.Key, pair.Value.Income / REGVAL);
                            DrawDataList["支出"].AddPoint(pair.Key, pair.Value.TotalExpense / REGVAL);
                        }
                        break;
                    case "収益":
                        DrawDataList.Add("収益", new DrawData(AccumMode.Value));
                        foreach (KeyValuePair<DateTime, MonthData> pair in refdata)
                            DrawDataList["収益"].AddPoint(pair.Key, pair.Value.Profit / REGVAL);
                        break;
                    case "費目":
                        foreach (KeyValuePair<DateTime, MonthData> pair in refdata)
                        {
                            foreach (string item in ExpItemList)
                            {
                                if (!DrawDataList.ContainsKey(item))
                                    DrawDataList.Add(item, new DrawData(AccumMode.Value));

                                if (!pair.Value.ExpenseSum.ContainsKey(item))
                                    DrawDataList[item].AddPoint(pair.Key, 0);
                                else
                                    DrawDataList[item].AddPoint(pair.Key, pair.Value.ExpenseSum[item] / REGVAL);
                            }
                        }
                        break;
                    case "所持金":
                        foreach (KeyValuePair<DateTime, MonthData> pair in refdata)
                        {
                            foreach (string item in CapitalList)
                            {
                                if (!DrawDataList.ContainsKey(item))
                                    DrawDataList.Add(item, new DrawData(AccumMode.Value));

                                if (!pair.Value.Balance.ContainsKey(item))
                                    DrawDataList[item].AddPoint(pair.Key, 0);
                                else
                                    DrawDataList[item].AddPoint(pair.Key, pair.Value.Balance[item] / REGVAL);
                            }
                        }
                        break;
                    case "所持金（総額）":
                        DrawDataList.Add("", new DrawData(AccumMode.Value));
                        foreach (KeyValuePair<DateTime, MonthData> pair in refdata)
                            DrawDataList[""].AddPoint(pair.Key, pair.Value.TotalBalance / REGVAL);
                        break;
                }
                if (StackMode.Value)
                {
                    DrawData tmp = new DrawData(AccumMode.Value);
                    foreach (KeyValuePair<string, DrawData> pair in DrawDataList)
                    {
                        pair.Value.Stack(tmp);
                        tmp = new DrawData(pair.Value);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public int DrawGraph(string ctype, string vtype, string year)
        {
            if (!CreateDataList(ctype, vtype, year))
                return 1;

            try
            {

                MyPlotModel.Value = new PlotModel();
                MyPlotModel.Value.ResetAllAxes();

                var AxisY = new LinearAxis()
                {
                    Title = "金額",
                    Unit = "千円",
                    FontSize = FONT_SIZE,
                    Position = AxisPosition.Left,
                    TickStyle = TickStyle.Inside,
                    StringFormat = "0",
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.None,
                };
                MyPlotModel.Value.Axes.Add(AxisY);

                var AxisX = new DateTimeAxis();
                AxisX.Position = AxisPosition.Bottom;
                AxisX.FontSize = FONT_SIZE;
                AxisX.MajorGridlineStyle = LineStyle.Solid;
                AxisX.MinorGridlineStyle = LineStyle.None;
                switch (vtype)
                {
                    case "月毎":
                        AxisX.StringFormat = "MM月";
                        AxisX.MinorIntervalType = DateTimeIntervalType.Months;
                        AxisX.IntervalType = DateTimeIntervalType.Months;
                        break;
                    case "年毎":
                        AxisX.StringFormat = "yyyy年";
                        AxisX.MinorIntervalType = DateTimeIntervalType.Years;
                        AxisX.IntervalType = DateTimeIntervalType.Years;
                        break;
                    case "全て":
                        AxisX.StringFormat = "yyyy/MM";
                        AxisX.MinorIntervalType = DateTimeIntervalType.Months;
                        AxisX.IntervalType = DateTimeIntervalType.Months;
                        break;
                }
                MyPlotModel.Value.Axes.Add(AxisX);

                int count = 0;
                if (!StackMode.Value)
                {
                    foreach (KeyValuePair<string, DrawData> pair in DrawDataList)
                    {
                        var lineSerie = new LineSeries
                        {
                            Title = pair.Key,
                            FontSize = FONT_SIZE,
                            ItemsSource = pair.Value.PointList,
                            StrokeThickness = STROKE_THICKNESS,
                            DataFieldX = "Date",
                            DataFieldY = "Value",
                            CanTrackerInterpolatePoints = false,
                        };
                        MyPlotModel.Value.Series.Add(lineSerie);
                        count++;
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, DrawData> pair in DrawDataList)
                    {
                        var areaSerie = new AreaSeries
                        {
                            Title = pair.Key,
                            FontSize = FONT_SIZE,
                            ItemsSource = pair.Value.PointList,
                            StrokeThickness = STROKE_THICKNESS,
                            DataFieldX = "Date",
                            DataFieldY = "Value",
                            CanTrackerInterpolatePoints = false,
                        };
                        MyPlotModel.Value.Series.Add(areaSerie);
                        count++;
                    }
                }
                return 0;
            }
            catch
            {
                return 2;
            }
        }


        // コンストラクタ
        public GraphViewModel() { }
    }


    /// <summary>
    /// ユーザー情報登録画面のビューモデル
    /// </summary>
    public class UserSettingViewModel
    {

        DataManagement DM = new DataManagement();
        
        public ReactiveProperty<string> ItemList { get; set; } = new ReactiveProperty<string>(); // 費目の設定 ※','区切りの一行文字列
        public ReactiveProperty<bool> IsEditable { get; set; } = new ReactiveProperty<bool>();
        public ReactiveProperty<List<BinderableNameValue>> AccountInfoList { get; set; } = new ReactiveProperty<List<BinderableNameValue>>(); // 
        public ReactiveProperty<List<CreditInfoObject>> CreditInfoList { get; set; } = new ReactiveProperty<List<CreditInfoObject>>();

        // 初期化
        public void Init(string path)
        {
            // DMを設定ファイルだけ読み出して初期化（ファイルが存在すればtrueで返ってくる）
            BindingDataManagement(DM.Init(path, true));
        }

        // コンストラクタ
        public UserSettingViewModel() {
        }

        public void BindingDataManagement(bool ro)
        {
            ItemList.Value = MyUtils.MyMethods.ListToStrLine(DM.ExpItemList, ',');
            IsEditable.Value = !ro;

            List<BinderableNameValue> tmp_acclist = new List<BinderableNameValue>();
            foreach (KeyValuePair<string, int> pair in DM.InitBalance)
                tmp_acclist.Add(new BinderableNameValue(pair.Key, pair.Value.ToString(), !ro));
            AccountInfoList.Value = new List<BinderableNameValue>(tmp_acclist);

            List<CreditInfoObject> tmp_credlist = new List<CreditInfoObject>();
            foreach (KeyValuePair<string, List<string>> pair in DM.CreditInformation)
                tmp_credlist.Add(new CreditInfoObject(pair.Key, pair.Value[0], pair.Value[1], !ro));
            CreditInfoList.Value = new List<CreditInfoObject>(tmp_credlist);

        }

        public int DataManagementReflection()
        {
            int val;

            List<string> tmp_items = new List<string>();
            string[] items = ItemList.Value.Split(',');
            foreach (string s in items)
                tmp_items.Add(s);

            Dictionary<string, int> tmp_ib = new Dictionary<string, int>();
            foreach (BinderableNameValue aio in AccountInfoList.Value)
            {
                if (!int.TryParse(aio.Value.Value, out val))
                    return 1;

                tmp_ib.Add(aio.Name.Value, val);
            }

            Dictionary<string, List<string>> tmp_cinfo = new Dictionary<string, List<string>>();
            foreach(CreditInfoObject cio in CreditInfoList.Value)
            {
                if (!int.TryParse(cio.PayDay.Value, out val)) // 支払予定日が不正。
                    return 2;
                if (val < 1 && val > 31)
                    return 2;

                if (!tmp_ib.ContainsKey(cio.Account.Value)) // 引き落とし口座が登録されていない
                    return 3;

                tmp_cinfo.Add(cio.CardName.Value, new List<string>() { cio.Account.Value, cio.PayDay.Value});
            }

            DM.UpdateSetting(tmp_items, tmp_ib, tmp_cinfo);

            return 0;
        }

    }

    // バインド用のNameとValueのみのクラス
    public class BinderableNameValue
    {

        public ReactiveProperty<string> Name { get; set; } = new ReactiveProperty<string>(); // 名前
        public ReactiveProperty<string> Value { get; set; } = new ReactiveProperty<string>(); // 現残金
        public ReactiveProperty<bool> IsEditable { get; set; } = new ReactiveProperty<bool>(); // ビュー上での編集可否

        // コンストラクタ
        public BinderableNameValue(string n, string v, bool ie = true)
        {
            Name.Value = n;
            Value.Value = v;
            IsEditable.Value = ie;
        }

    }

    // クレジットカード情報を扱うクラス
    public class CreditInfoObject
    {
        public ReactiveProperty<string> CardName { get; set; } = new ReactiveProperty<string>(); // カード名
        public ReactiveProperty<string> Account { get; set; } = new ReactiveProperty<string>(); // 引き落とし口座
        public ReactiveProperty<string> PayDay { get; set; } = new ReactiveProperty<string>();// 支払予定日
        public ReactiveProperty<bool> IsEditable { get; set; } = new ReactiveProperty<bool>(); // ビュー上での編集可否

        // コンストラクタ
        public CreditInfoObject(string cn, string ac, string pd, bool ie = true)
        {
            CardName.Value = cn;
            Account.Value = ac;
            PayDay.Value = pd;
            IsEditable.Value = ie;
        }

    }

}
