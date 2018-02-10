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
    /// BudgetSetting.xaml の相互作用ロジック
    /// </summary>
    public partial class BudgetSetting : Window
    {

        MainWindow MWin;

        public BudgetSetting(MainWindow mwin)
        {
            InitializeComponent();

            this.MWin = mwin;
            this.DataContext = MWin.HABAVM;
            UpdateBudgetSumInfo();

        }

        // 過去数か月以内の出費の平均を反映させる
        private void ExpensesBudgetCalcButton_Click(object sender, RoutedEventArgs e)
        {
            int val;
            TextBox tb = this.ExpensesBudgetMonthRange as TextBox;
            if (int.TryParse(tb.Text, out val))
            {
                if (val <= 0)
                {
                    MessageBox.Show(MWin.MSG_INVALID_INPUT, MWin.TITLE_WARNING_DIALOG, MessageBoxButton.OK);
                    tb.Text = "3";
                    return;
                }
                else
                {
                    MWin.HABAVM.ExpensesBudgetList.Value = new List<BinderableNameValue>(MWin.HABAVM.BNVConvert(MWin.DM.CalcExpenditureTrend(val)));
                    UpdateBudgetSumInfo();
                }
            }
            else
            {
                MessageBox.Show(MWin.MSG_INVALID_INPUT, MWin.TITLE_WARNING_DIALOG, MessageBoxButton.OK);
                tb.Text = "3";
                return;
            }
        }

        // 入力内容を適用
        private void BudgetApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (MWin.HABAVM.CheckSetBudget())
            {
                MWin.DM.UpdateBudget(MWin.HABAVM.ConvertEBLList(), MWin.HABAVM.ConvertIBLList());
                this.Close();
            }
            else
            {
                MessageBox.Show(MWin.MSG_INVALID_INPUT, MWin.TITLE_WARNING_DIALOG, MessageBoxButton.OK);
            }
        }

        // 合計値表示の更新
        private void UpdateBudgetSumInfo()
        {
            MWin.HABAVM.UpdateBudgetSum();
            if (MWin.HABAVM.ProfitBudget.Value >= 0)
                this.MainProfitPanel.Background = new SolidColorBrush(Colors.AliceBlue);
            else
                this.MainProfitPanel.Background = new SolidColorBrush(Colors.MistyRose);
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if(MWin.HABAVM.CheckSetBudget())
                UpdateBudgetSumInfo();
        }

    }
}
