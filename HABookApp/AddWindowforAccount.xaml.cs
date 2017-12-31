using System;
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
using System.Windows.Shapes;

namespace HABookApp
{
    /// <summary>
    /// AddWindowforCredit.xaml の相互作用ロジック
    /// </summary>
    public partial class AddWindowforAccount : Window
    {

        private const int INACCOUNT_MAX_INPUT_NUM = 5;
        MainWindow MWin;

        public AddWindowforAccount(MainWindow mwin)
        {
            InitializeComponent();

            this.MWin = mwin;
            this.DataContext = MWin.HABAVM;
            AddContentsReset();

        }

        private void AddContentsReset()
        {
            string rlabel = "Add", label;
            List<string> label_list = new List<string> { "Mode", "Date", "Account", "Amount", "Remarks", "Item"};

            DatePicker dp;
            TextBox tb;
            ComboBox cb;

            for (int i = 1; i <= INACCOUNT_MAX_INPUT_NUM; i++)
            {
                foreach (string label0 in label_list)
                {
                    label = rlabel + label0 + i.ToString();
                    switch (label0)
                    {
                        case "Date":
                            dp = this.FindName(label) as DatePicker;
                            dp.Text = MWin.NOWDATE;
                            break;
                        case "Amount":
                        case "Detail":
                            tb = this.FindName(label) as TextBox;
                            tb.Text = null;
                            break;
                        case "Mode":
                        case "Account":
                        case "Item":
                            cb = this.FindName(label) as ComboBox;
                            cb.SelectedValue = null;
                            break;
                    }
                }
            }
            return;
        }

        private void AddAmountCheck(object sender, RoutedEventArgs e)
        {
            // テキストボックス入力内容のチェック
            TextBox tb = sender as TextBox;
            if (MyUtils.MyMethods.CheckTextBoxAmount(tb.Text) == -1)
            {
                MessageBox.Show(MWin.MSG_INVALID_INPUT, MWin.TITLE_WARNING_DIALOG, MessageBoxButton.OK);
                tb.Text = null;
                return;
            }
        }

        //private void AddDateCheck(object sender, RoutedEventArgs e)
        //{
        //    // DatePicker入力内容のチェック
        //    DatePicker dp = sender as DatePicker;
        //    DateTime date;
        //    if (!(DateTime.TryParse(dp.Text, out date)))
        //    {
        //        MessageBox.Show(MWin.MSG_INVALID_INPUT, MWin.TITLE_WARNING_DIALOG, MessageBoxButton.OK);
        //        dp.Text = null;
        //        return;
        //    }
        //}

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {

            string rlabel = "Add", label;
            TextBox tb;
            ComboBox cb;
            DatePicker dp;

            // 数値入力のあるnoを取得
            int amount;
            Dictionary<int, int> inputno = new Dictionary<int, int>();
            label = rlabel + "Amount";
            for (int i = 1; i <= INACCOUNT_MAX_INPUT_NUM; i++)
            {
                tb = this.FindName(label + i.ToString()) as TextBox;
                amount = MyUtils.MyMethods.CheckTextBoxAmount(tb.Text);
                if (amount != 0)
                    inputno.Add(i, amount);
            }
            if (inputno.Count == 0) return;

            // 入力内容をInCreditData型にしてリスト化
            string mode, date, eitem, remark, citem;
            string addinfo = "", no_str;
            InAccountData adddata;
            List<InAccountData> adddata_list = new List<InAccountData>();
            foreach (int no in inputno.Keys)
            {
                no_str = "入力" + no.ToString();
                // モード取得
                cb = this.FindName(rlabel + "Mode" + no.ToString()) as ComboBox;
                if (cb.SelectedValue == null)
                {
                    MessageBox.Show(no_str + "->取引 : 未選択です。", MWin.TITLE_WARNING_DIALOG, MessageBoxButton.OK);
                    return;
                }
                mode = (string)cb.SelectedValue;
                // 日付取得
                dp = this.FindName(rlabel + "Date" + no.ToString()) as DatePicker;
                date = dp.Text;
                if (date == "")
                {
                    MessageBox.Show(no_str + "->日付 ： 未入力です。", MWin.TITLE_WARNING_DIALOG, MessageBoxButton.OK);
                    return;
                }
                // 口座取得
                cb = this.FindName(rlabel + "Account" + no.ToString()) as ComboBox;
                if (cb.SelectedValue == null)
                {
                    MessageBox.Show(no_str + "->口座 : 未選択です。", MWin.TITLE_WARNING_DIALOG, MessageBoxButton.OK);
                    return;
                }
                citem = (string)cb.SelectedValue;
                // 詳細取得(未入力OK)
                tb = this.FindName(rlabel + "Remarks" + no.ToString()) as TextBox;
                remark = tb.Text;
                // 費目取得(振替のみ未入力NG)
                cb = this.FindName(rlabel + "Item" + no.ToString()) as ComboBox;
                if (cb.SelectedValue == null)
                {
                    if (mode == "振替")
                    {
                        MessageBox.Show(no_str + "->費目 : 未選択です。", MWin.TITLE_WARNING_DIALOG, MessageBoxButton.OK);
                        return;
                    }
                    else
                    {
                        eitem = "";
                    }
                }else
                    eitem = (string)cb.SelectedValue;

                adddata = new InAccountData(mode, date, citem, inputno[no], eitem, remark, false);
                addinfo += ("\n" + adddata.StringConv2());
                adddata_list.Add(adddata);
            }

            // 確認画面
            var ret = MessageBox.Show("下記の利用履歴を登録します。" + addinfo, MWin.TITLE_CONFIRM_DIALOG, MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (ret == MessageBoxResult.OK)
            {
                // DataManagementに渡す
                MWin.DM.AddAccountData(adddata_list);
                Close();
            }
            else
                return;

        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            AddContentsReset();
        }
    }
}
