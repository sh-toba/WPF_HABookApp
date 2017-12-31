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
    /// GraphView.xaml の相互作用ロジック
    /// </summary>
    public partial class GraphView : Window
    {

        MainWindow MWin;
        public GraphViewModel GVM { get; set; } = new GraphViewModel();

        public GraphView(MainWindow mwin)
        {
            InitializeComponent();

            this.MWin = mwin;

            MWin.HABAVM.VMLog.Write("-> GraphView Initialization");
            if (!GVM.Init(MWin.DM.LoadMonthData()))
            {
                MessageBox.Show("初期化に失敗しました。", MWin.TITLE_WARNING_DIALOG, MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
            this.DataContext = GVM;

            // 初期化
            SelectContentType.SelectedValue = GVM.ContentList.Value[0];
            SelectViewlevel.SelectedValue = GVM.ViewLevelList.Value[0];
            SelectYear.SelectedValue = GVM.YearList.Value.Last();
            MWin.HABAVM.VMLog.Write("-> GraphView Initialization is Completed");
        }

        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            MWin.HABAVM.VMLog.WriteWithMethod("");
            string ctype = SelectContentType.SelectedValue as string;
            string vtype = SelectViewlevel.SelectedValue as string;
            string year = SelectYear.SelectedValue as string;

            switch(GVM.DrawGraph(ctype, vtype, year))
            {
                case 1:
                    MessageBox.Show("データ計算に失敗しました。", MWin.TITLE_WARNING_DIALOG, MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case 2:
                    MessageBox.Show("描画処理に失敗しました。", MWin.TITLE_WARNING_DIALOG, MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
            return;
        }

        private void SelectViewlevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string vtype = SelectViewlevel.SelectedValue as string;
            GVM.SetList_Yaer(vtype);

            if (vtype == "月毎") SelectYear.SelectedValue = GVM.YearList.Value.Last();
        }

        private void DrawSettingButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
