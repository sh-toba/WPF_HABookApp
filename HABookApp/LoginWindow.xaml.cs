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
using Microsoft.WindowsAPICodePack.Dialogs;

namespace HABookApp
{

    /// <summary>
    /// LoginWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class LoginWindow : Window
    {
        MainWindow MWin;

        public LoginWindow(MainWindow mwin)
        {
            InitializeComponent();

            this.MWin = mwin;
            LoginID.ItemsSource = MWin.LM.GetUsers();

            LoginID.Focus();
        }

        private void RegistButton_Click(object sender, RoutedEventArgs e)
        {

            var dlg = new CommonOpenFileDialog("フォルダを選択してください");
            // フォルダ選択モード。
            dlg.IsFolderPicker = true;
            var ret = dlg.ShowDialog();
            if (ret == CommonFileDialogResult.Ok)
            {
                var SetWindow = new UserSetting("add", MWin, dlg.FileName + "\\");
                SetWindow.ShowDialog();

                LoginID.ItemsSource = MWin.LM.GetUsers();
            }

            return;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoginEvent())
                this.Close();
            return;
        }

        private void LoginPASS_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (LoginEvent())
                     this.Close();
            }
            return;
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if(LoginEvent())
            {
                var SetWindow = new UserSetting("change", MWin, MWin.LM.GetDIR());
                SetWindow.ShowDialog();
            }
            return;
        }

        private bool LoginEvent()
        {
            string id = LoginID.Text;
            string pass = LoginPASS.Password;

            MWin.LM.Login(id, pass); // ログイン処理
            switch (MWin.LM.LoginState)
            {
                case MyUtils.LoginManage.STATE.SUCCESS:
                    return true;
                case MyUtils.LoginManage.STATE.NOTMATCH:
                    MessageBox.Show("パスワードが一致しません。", MWin.TITLE_WARNING_DIALOG, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                case MyUtils.LoginManage.STATE.NOREISTERED:
                    MessageBox.Show("未登録のユーザーです。", MWin.TITLE_WARNING_DIALOG, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                case MyUtils.LoginManage.STATE.INVALID:
                    MessageBox.Show("他ユーザがアクセス中です。", MWin.TITLE_WARNING_DIALOG, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                default:
                    return false;
            }
        }

        
    }
}
