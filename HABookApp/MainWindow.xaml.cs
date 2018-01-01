using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace HABookApp
{ 
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// MainWindow定義値
        /// </summary>
        private const string APPNAME = "Household Account Book";
        private const string VERSION = "__ver3.0β__";
        private const bool MODE_DEVELOP = false;
        private const int INCASH_MAX_INPUT_NUM = 5; // MAX入力数 ※XAMLに合わせる

        /// <summary>
        /// メッセージWindow表示内容 (他Windowと共用するためpublicにしている)
        /// </summary>
        public string TITLE_WARNING_DIALOG { get; } = "Notification";
        public string TITLE_CONFIRM_DIALOG { get; } = "Confirmation";
        public string TITLE_INFO_DIALOG { get; } = "Information";
        public string MSG_INVALID_INPUT { get; } = "無効な入力。";

        public string BOOTDATE { get; set; }
        public string NOWDATE { get; set; }

        public DataManagement DM { get; set; } = new DataManagement();
        public HABAppViewModel HABAVM { get; set; } = new HABAppViewModel();
        public MyUtils.LoginManage LM { get; set; } = new MyUtils.LoginManage();
        
        // スレッド管理
        //Task updatetask = new Task(()=> { }); // アップデート処理のタスク

        public MainWindow()
        {

            // ユーザーリスト読み込み
            if (!LM.LoadUsers())
            {
                //MessageBox.Show("ユーザーリストの読み取りに失敗しましたm(_ _)m\nアプリを終了します。", TITLE_WARNING_DIALOG, MessageBoxButton.OK, MessageBoxImage.Error);
                //Environment.Exit(0); // 強制終了
            }

            // ログイン画面
            var LoginWindow = new LoginWindow(this);
            LoginWindow.ShowDialog();
            // 操作なしで閉じられた場合
            if (LM.LoginState != MyUtils.LoginManage.STATE.SUCCESS)
            {
                //MessageBox.Show("ログインに失敗しましたm(_ _)m\nアプリを終了します。", TITLE_WARNING_DIALOG, MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0); // 強制終了
            }                    

            InitializeComponent();
            this.Title = APPNAME + " " + VERSION;

            IDText.Text = "ID : " + LM.GetID();
            DM.Init(LM.GetDIR());
            HABAVM.Init();

            // アプリ初期設定
            Initialize_All();

        }

        /// <summary>
        /// 初期化処理の設定
        /// </summary>
        /// 
        private void Initialize_All()
        {
            HABAVM.VMLog.WriteWithMethod("");

            // 起動日とアプリ内選択日の保持
            BOOTDATE = DM.bootDate.ToString("yyyy/MM/dd");
            NOWDATE = (DM.SelectiveDate.Contains(BOOTDATE)) ? BOOTDATE : DM.SelectiveDate.Last();

            // バインディングデータの設定
            HABAVM.VMLog.Write("-> Set Binding Data");
            this.DataContext = HABAVM;
            HABAVM.ExpItemList.Value = new List<string>(DM.ExpItemList);
            HABAVM.CapItemList.Value = new List<string>(DM.CapItemList);
            HABAVM.GetAccountList();
            HABAVM.CardList.Value = new List<string>(DM.GetCardList());
            HABAVM.AccInputItem.Value = new List<string>(DM.AccInputItem);
            InCreditSelectedAcc.SelectedValue = HABAVM.AccountList.Value[0]; //Credotタブの初期選択口座
            InAccountSelectedAcc.SelectedValue = HABAVM.CapItemList.Value[2]; //Accountタブの初期選択口座
            HABAVM.SelectiveDate.Value = new List<string>(DM.SelectiveDate);
            HABAVM.NowDate.Value = NOWDATE;
            HABAVM.MainGraphMenu.Value = new List<string>() { "総計", "現金利用", "クレジット利用", "口座振替"};
            MainSelectGraph.SelectedValue = HABAVM.MainGraphMenu.Value[0];

            this.textBlock_ManageDate.Text = (DateTime.Parse(DM.ManageDate)).ToString("yyyy年MM月") + " 家計簿";

            if (DM.SelectiveDate.Contains(BOOTDATE))
                this.EndofMonthFix.IsEnabled = false;
            else
                this.EndofMonthFix.IsEnabled = true;

            // 各タブの初期設定
            HABAVM.VMLog.Write("-> Initialize each tab");
            UpdateCapitalInformation();
            MainDrawGraph();
            UpdateCashData("Get");
            UpdateCreditData("Get");
            UpdateAccountData("Get");

            return;
        }


        /// <summary>
        /// Mainタブ関連
        /// </summary>
        /// 
        private void UpdateCapitalInformation()
        {
            HABAVM.VMLog.WriteWithMethod("");
            // 総出費額の取得
            int cashsum = DM.CashSum;
            int creditsum = DM.CalcCreditSum(DM.ManageDate, "", "", "");
            int accsum = DM.CalcAccountSum("振替", DM.ManageDate, "", "", "");
            int totalsum = cashsum + creditsum + accsum;
            HABAVM.TotalExpense.Value = totalsum;
            // 入金総額値の取得
            int income = DM.CalcAccountSum("入金", DM.ManageDate, "", "", "");
            HABAVM.TotalIncome.Value = income;
            // 利益の計算
            HABAVM.Profit.Value = income - totalsum;
            if (HABAVM.Profit.Value >= 0)
                MainProfitPanel.Background = new SolidColorBrush(Colors.AliceBlue);
            else
                MainProfitPanel.Background = new SolidColorBrush(Colors.MistyRose);
            //HABAVM.VMLog.Write("-> TotalExpense=" + totalsum.ToString() + " :: Income=" + income.ToString());

            Dictionary<string, int> dummy = new Dictionary<string, int>();
            HABAVM.CapitalDataList.Value = new List<CapitalDataView>(); 
            int ib, nb, pb, initsum = 0, nowsum = 0, predictsum = 0;
            foreach(string citem in HABAVM.CapItemList.Value)
            {
                ib = DM.InitBalance[citem];
                nb = DM.GetNowBalance(citem);
                pb = 0;
                HABAVM.CapitalDataList.Value.Add(new CapitalDataView(citem, ib, nb, 0, dummy));
                initsum += ib;
                nowsum += nb;
                predictsum += pb;
            }
            HABAVM.CapitalDataList.Value.Add(new CapitalDataView("合計", initsum, nowsum, predictsum, dummy));

            // グラフ再描画
            MainDrawGraph();
            return;
        }

        private void MainDrawGraph()
        {
            string option = MainSelectGraph.SelectedValue as string;
            // DataManagement用のoptionに変換
            switch (option)
            {
                case "現金利用":
                    option = "cash";
                    break;
                case "クレジット利用":
                    option = "credit";
                    break;
                case "口座振替":
                    option = "account";
                    break;
                case "総計":
                    option = "total";
                    break;
            }
            // 合計値を取得しバインディング
            HABAVM.ConvertBarGraphItems(DM.GetItemsSum(option));
            return;
        }

        private void MainSelectGraph_DropDownClosed(object sender, EventArgs e)
        {
            MainDrawGraph();
        }

        /// <summary>
        /// Cashタブ関連
        /// </summary>

        // 入力内容の初期化
        private void ResetInCashContents()
        {
            string rlabel = "InCash", label;
            List<string> label_list = new List<string> { "Date", "Amount", "Item", "Detail" };

            TextBox tb;
            ComboBox cb;

            for (int i = 1; i <= INCASH_MAX_INPUT_NUM; i++)
            {
                foreach (string label0 in label_list)
                {
                    label = rlabel + label0 + i.ToString();
                    switch (label0)
                    {
                        case "Date":
                            cb = this.FindName(label) as ComboBox;
                            cb.SelectedValue = NOWDATE;
                            break;
                        case "Amount":
                        case "Detail":
                            tb = this.FindName(label) as TextBox;
                            tb.Text = null;
                            break;
                        case "Item":
                            cb = this.FindName(label) as ComboBox;
                            cb.SelectedValue = null;
                            break;
                    }
                }
            }
            return;
        }

        private void UpdateCashInformation()
        {
            HABAVM.VMLog.WriteWithMethod("");

            // 現在の残金の更新
            int nbalance = DM.GetNowBalance("現金");
            this.InCashNowBalance.Text = "\\" + nbalance.ToString();

            // Amount欄の合計値の取得
            int sum = 0;
            TextBox tb;
            string rlabel = "InCashAmount", label;
            for (int i = 1; i <= INCASH_MAX_INPUT_NUM; i++)
            {
                label = rlabel + i.ToString();
                tb = this.FindName(label) as TextBox;
                sum += MyUtils.MyMethods.CheckTextBoxAmount(tb.Text);
            }

            // Add後の額の更新
            int addbalance = nbalance - sum;
            this.InCashNowBalance_Add.Text = "Add後 : \\" + addbalance.ToString();

            // 手入力値との差分の取得
            int manualbalance = MyUtils.MyMethods.CheckTextBoxAmount(InCashManuaBalanceCheck.Text);
            this.InCashCheckDifference.Text = "差分 : \\" + (addbalance - manualbalance).ToString();

            return;
        }
        // CashDataのGUI反映処理
        private void UpdateCashData(string way)
        {
            HABAVM.VMLog.WriteWithMethod(" [" + way + "]");
            switch (way)
            {
                case "Get": // DataManagement ⇒ View
                    HABAVM.CashData.Value = new Dictionary<string, Dictionary<string, int>>(DM.CashData);
                    HABAVM.CashDataDetail.Value = new Dictionary<string, string>(DM.CashDataDetail);
                    break;
                case "Set": // View ⇒ DataManagement
                    DM.SetCashData(HABAVM.CashData.Value, HABAVM.CashDataDetail.Value);
                    break;
            }
            HABAVM.ConvertCashDataForView(); // 表用データの更新
            // 情報の更新
            UpdateCashInformation();
        }

        // Addボタン
        private void InCashAdd_Click(object sender, RoutedEventArgs e)
        {
            HABAVM.VMLog.WriteWithMethod("");
            // キャッシュデータの保存
            HABAVM.AddCacheCashData();

            string rlabel = "InCash", label;
            TextBox tb;
            ComboBox cb;

            // 数値入力のあるnoを取得
            int amount;
            Dictionary<int, int> inputno = new Dictionary<int, int>();
            label = rlabel + "Amount";
            for (int i = 1; i <= INCASH_MAX_INPUT_NUM; i++)
            {
                tb = this.FindName(label + i.ToString()) as TextBox;
                amount = MyUtils.MyMethods.CheckTextBoxAmount(tb.Text);
                if (amount != 0)
                    inputno.Add(i, amount);
            }
            if (inputno.Count == 0) return;

            // 入力内容をInCashData型にしてリスト化
            string date, item, detail;
            List<InCashData> adddata_list = new List<InCashData>();
            foreach (int no in inputno.Keys)
            {
                // 日付取得
                cb = this.FindName(rlabel + "Date" + no.ToString()) as ComboBox;
                date = (string)cb.SelectedValue;
                // 費目取得
                cb = this.FindName(rlabel + "Item" + no.ToString()) as ComboBox;
                if (cb.SelectedValue == null)
                {
                    MessageBox.Show("入力"　+ no.ToString()　+ " : 費目が未選択です。", TITLE_WARNING_DIALOG, MessageBoxButton.OK);
                    return;
                }
                item = (string)cb.SelectedValue;
                // 詳細取得
                tb = this.FindName(rlabel + "Detail" + no.ToString()) as TextBox;
                detail = tb.Text;
                if(detail == "")
                {
                    MessageBox.Show("入力" + no.ToString() + " : 詳細が未入力です。", TITLE_WARNING_DIALOG, MessageBoxButton.OK);
                    return;
                }
                adddata_list.Add(new InCashData(date, item, inputno[no], detail));
            }

            // DataManagementに渡す
            DM.AddCashData(adddata_list);
            ResetInCashContents();
            UpdateCashData("Get");
            
        }
        // Undoボタン
        private void InCashUndo_Click(object sender, RoutedEventArgs e)
        {
            HABAVM.VMLog.WriteWithMethod("");
            HABAVM.LoadLatestCacheCashData();
            UpdateCashData("Set");
        }
        // 数値入力用テキストボックス
        private void InCashAmountInput(object sender, RoutedEventArgs e)
        {
            // テキストボックス入力内容のチェック
            TextBox tb = sender as TextBox;
            if(MyUtils.MyMethods.CheckTextBoxAmount(tb.Text) == -1)
            {
                MessageBox.Show(MSG_INVALID_INPUT, TITLE_WARNING_DIALOG, MessageBoxButton.OK);
                tb.Text = null;
                return;
            }
            // 情報の更新
            UpdateCashInformation();
        }


        /// <summary>
        /// Creditタブ関連
        /// </summary>

        private void UpdateCreditInformation()
        {
            HABAVM.VMLog.WriteWithMethod("");
            // 口座取得
            string acc = InCreditSelectedAcc.SelectedValue as string; // 選択されている口座の取得

            // 統計値計算
            int nbalance = DM.GetNowBalance(acc);
            int totalsum = DM.CalcCreditSum("", acc, "", "");
            int confirmedsum = DM.CalcCreditSum("", acc, "", DM.ManageDate);
            int difference = nbalance - confirmedsum;
            int monthsum = DM.CalcCreditSum(DM.ManageDate, acc, "", "");

            // GUIに反映
            InCreditNowbalance.Text = "\\" + nbalance.ToString();
            if (nbalance < totalsum)
                InCreditNowbalance.Foreground = new SolidColorBrush(Colors.Red);
            else
                InCreditNowbalance.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
            InCreditTotalSum.Text = "\\" + totalsum.ToString();
            InCreditConfirmedSum.Text = "\\" + confirmedsum.ToString();
            InCreditDifference.Text = "\\" + difference.ToString();
            if (difference <= 0)
                InCreditDifference.Foreground = new SolidColorBrush(Colors.Red);
            else
                InCreditDifference.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
            InCreditMonthSum.Text = "\\" + monthsum.ToString();

            return;
        }
        // CreditDataのGUI反映処理
        private void UpdateCreditData(string way)
        {
            HABAVM.VMLog.WriteWithMethod(" [" + way + "]");
            switch (way)
            {
                case "Get": // DataManagement ⇒ View
                    HABAVM.ConvertCreditDataFromDM(DM.CreditData);
                    break;
                case "Set": // View ⇒ DataManagement
                    DM.SetCreditData(HABAVM.ConvertCreditDataToDM(DM.ManageDate));
                    break;
            }
            // 情報の更新
            UpdateCreditInformation();
            return;
        }

        private void InCreditSelectedAcc_DropDownClosed(object sender, EventArgs e)
        {
            UpdateCreditInformation();
        }
        private void InCreditCheckBox_Click(object sender, RoutedEventArgs e)
        {
            UpdateCreditData("Set");
            //HABAVM.ShowCreditData();
            //Console.WriteLine("{0}", sender);
        }
        private void InCreditUndo_Click(object sender, RoutedEventArgs e)
        {
            HABAVM.LoadLatestCacheCreditData();　// 最新のキャッシュの読み込み
            UpdateCreditData("Set");
        }
        private void InCreditDelete_Click(object sender, RoutedEventArgs e)
        {
            // 選択されているインデックスの取得
            List<int> SelectedIndex = new List<int>();
            for (int no = 0; no < InCreditListView.SelectedItems.Count; no++)
            {
                SelectedIndex.Add((InCreditListView.Items.IndexOf(InCreditListView.SelectedItems[no])));
            }
            if (SelectedIndex.Count == 0)
                return;

            // 選択項目の情報取得
            string selectediteminfo = "管理No.";
            foreach (int index in SelectedIndex)
            {
                selectediteminfo += ((HABAVM.CreditData.Value[index].Number.Value).ToString() + ", ");
            }

            // 確認ダイアログ
            var ret = MessageBox.Show("下記項目を削除しますか？\n" + selectediteminfo, TITLE_CONFIRM_DIALOG, MessageBoxButton.OKCancel, MessageBoxImage.Question);

            // OKなら項目削除
            if (ret == MessageBoxResult.OK)
            {
                HABAVM.AddCacheCreditData(); // キャッシュに保存
                // 削除
                DM.RemoveCreditData(SelectedIndex);
                // 更新
                UpdateCreditData("Get");
            }
            else
                return;
        }
        private void InCreditAdd_Click(object sender, RoutedEventArgs e)
        {
            //キャッシュ保存
            HABAVM.AddCacheCreditData();
            // Add用のWindowでDataManagementにデータ追加
            var AddWindow = new AddWindowforCredit(this);
            AddWindow.ShowDialog();
            // 更新
            UpdateCreditData("Get");
            return;
        }
        private void CreditDataSort(object sender, RoutedEventArgs e)
        {
            // ソート
            DM.SortCreditData();
            // 更新
            UpdateCreditData("Get");
        }

        /// <summary>
        /// Accountタブ関連
        /// </summary>

        private void UpdateAccountInformation()
        {
            HABAVM.VMLog.WriteWithMethod("");
            // 口座取得
            string acc = InAccountSelectedAcc.SelectedValue as string; // 選択されている口座の取得

            // 統計値計算
            int nbalance = DM.GetNowBalance(acc);
            int totalsum = DM.CalcAccountSum("振替", DM.ManageDate, acc, "", "");
            int nopaidsum = DM.CalcAccountSum("振替", DM.ManageDate, acc, "", "nopaid");
            int difference = nbalance - nopaidsum;
            int monthinputsum = DM.CalcAccountSum("入金", DM.ManageDate, acc, "", "");

            // GUIに反映
            InAccountNowbalance.Text = "\\" + nbalance.ToString();
            InAccountTotalSum.Text = "\\" + totalsum.ToString();
            InAccountNoPaidSum.Text = "\\" + nopaidsum.ToString();
            InAccountDifference.Text = "\\" + difference.ToString();
            if (difference <= 0)
                InAccountDifference.Foreground = new SolidColorBrush(Colors.Red);
            else
                InAccountDifference.Foreground = new SolidColorBrush(Colors.CornflowerBlue);
            InAccountInputSum.Text = "\\" + monthinputsum.ToString();

            return;
        }
        // AccountDataのGUI反映処理
        private void UpdateAccountData(string way)
        {
            HABAVM.VMLog.WriteWithMethod(" [" + way + "]");
            switch (way)
            {
                case "Get": // DataManagement ⇒ View
                    HABAVM.ConvertAccountDataFromDM(DM.AccountData);
                    break;
                case "Set": // View ⇒ DataManagement
                    DM.SetAccountData(HABAVM.ConvertAccountDataToDM());
                    break;
            }
            // 情報の更新
            UpdateAccountInformation();
        }

        private void InAccountSelectedAcc_DropDownClosed(object sender, EventArgs e)
        {
            UpdateAccountInformation();
        }
        private void InAccountCheckBox_Click(object sender, RoutedEventArgs e)
        {
            UpdateAccountData("Set");
            //HABAVM.ShowCreditData();
            //Console.WriteLine("{0}", sender);
        }
        private void InAccountUndo_Click(object sender, RoutedEventArgs e)
        {
            HABAVM.LoadLatestCacheAccountData();　// 最新のキャッシュの読み込み
            UpdateAccountData("Set");
        }
        private void InAccountDelete_Click(object sender, RoutedEventArgs e)
        {
            // 選択されているインデックスの取得
            List<int> SelectedIndex = new List<int>();
            for (int no = 0; no < InAccountListView.SelectedItems.Count; no++)
            {
                SelectedIndex.Add((InAccountListView.Items.IndexOf(InAccountListView.SelectedItems[no])));
            }
            if (SelectedIndex.Count == 0)
                return;

            // 選択項目の情報取得
            string selectediteminfo = "管理No.";
            foreach (int index in SelectedIndex)
            {
                selectediteminfo += ((HABAVM.AccountData.Value[index].Number.Value).ToString() + ", ");
            }

            // 確認ダイアログ
            var ret = MessageBox.Show("下記項目を削除しますか？\n" + selectediteminfo, TITLE_CONFIRM_DIALOG, MessageBoxButton.OKCancel, MessageBoxImage.Question);

            // OKなら項目削除
            if (ret == MessageBoxResult.OK)
            {
                HABAVM.AddCacheAccountData(); // キャッシュに保存
                // 削除
                DM.RemoveAccountData(SelectedIndex);
                // 更新
                UpdateAccountData("Get");
            }
            else
                return;
        }
        private void InAccountAdd_Click(object sender, RoutedEventArgs e)
        {
            // キャッシュ保存
            HABAVM.AddCacheAccountData();
            // Add用のWindowでDataManagementにデータ追加
            var AddWindow = new AddWindowforAccount(this);
            AddWindow.ShowDialog();
            // 更新
            UpdateAccountData("Get");
            return;
        }
        private void AccountDataSort(object sender, RoutedEventArgs e)
        {
            // ソート
            DM.SortAccountData();
            // 更新
            UpdateAccountData("Get");
            return;
        }

        /// <summary>
        /// タブ遷移時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = TabControl.SelectedIndex;
            switch (selectedIndex)
            {
                case 0:
                    UpdateCapitalInformation();
                    break;
                case 1:
                    UpdateCashInformation();
                    break;
                case 2:
                    UpdateCreditInformation();
                    break;
                case 3:
                    UpdateAccountInformation();
                    break;
            }
        }


        /// <summary>
        /// 月末Fix処理
        /// </summary>
        private void EndofMonthFix_Click(object sender, RoutedEventArgs e)
        {
            int ret = DM.FixEndofMonth();
            if (ret != 0)
                MessageBox.Show("月末Fix処理に失敗しました!!\n(return " + ret.ToString() + " )\n", TITLE_WARNING_DIALOG, MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                MessageBox.Show("月末Fix処理が完了しました!!\n", TITLE_CONFIRM_DIALOG, MessageBoxButton.OK, MessageBoxImage.Information);
                HABAVM.VMLog.Refresh();
                Initialize_All();
            }
        }


        private void myDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "RowHeader")
            {
                e.Cancel = true;
            }
        }

        private void GraphViewButton_Click(object sender, RoutedEventArgs e)
        {
            HABAVM.VMLog.WriteWithMethod("");
            var GraphView = new GraphView(this);
            GraphView.ShowDialog();
        }

        private void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            DM.CreateBackup();
            MessageBox.Show("バックアップを作成しました。", TITLE_INFO_DIALOG, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }


}
