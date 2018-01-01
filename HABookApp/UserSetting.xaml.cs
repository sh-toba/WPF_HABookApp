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
    /// UserSetting.xaml の相互作用ロジック
    /// </summary>
    public partial class UserSetting : Window
    {

        MainWindow MWin;
        UserSettingViewModel USVM = new UserSettingViewModel();
        public string MODE;

        public UserSetting(string mode, MainWindow mwin, string path)
        {
            InitializeComponent();
            SetDIR.Text = path;
            USVM.Init(path);

            this.MWin = mwin;
            this.MODE = mode;

            if (MODE == "change")
            {
                SetID.Text = MWin.LM.GetID();
                SetPASS.Password = MWin.LM.GetPASS();
                SetPASS2.Password = MWin.LM.GetPASS();
                SetButton.Content = " 変更 ";
            }

            this.DataContext = USVM;
        }

        private void SetButton_Click(object sender, RoutedEventArgs e)
        {
            // 空欄チェック
            if (CheckTextBlank(SetID.Text) || CheckTextBlank(SetPASS.Password) || CheckTextBlank(SetPASS2.Password) || CheckTextBlank(SetDIR.Text))
                return;

            string id = SetID.Text;
            string pass = SetPASS.Password;
            string pass2 = SetPASS2.Password;
            string dir = SetDIR.Text;

            if (pass != pass2)
                MessageBox.Show("パスワード（再入力）が一致しません。", MWin.TITLE_WARNING_DIALOG, MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                if (MODE == "add")
                {
                    if(MWin.LM.ExistUser(id))
                        MessageBox.Show("登録済みIDです。", MWin.TITLE_WARNING_DIALOG, MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                    {
                        if (!MWin.LM.AddUser(id, pass, dir))
                            MessageBox.Show("AddUser is Failed", MWin.TITLE_WARNING_DIALOG, MessageBoxButton.OK, MessageBoxImage.Error);
                        else
                        {
                            int ret_val = USVM.DataManagementReflection();
                            switch (ret_val) {
                                case 0:
                                    this.Close();
                                    break;
                            }
                        }
                    }
                }else if (MODE == "change"){
                    if (!MWin.LM.ChangeUsersInfo(MWin.LM.GetID(), id, pass, dir))
                        MessageBox.Show("ChangeUsersInfo is Failed", MWin.TITLE_WARNING_DIALOG, MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        this.Close();
                }
            }
        }

        private void FolderSelect_Click(object sender, RoutedEventArgs e)
        {
        }

        private bool CheckTextBlank(string text)
        {
            if (text == "")
            {
                MessageBox.Show("空欄があります。", MWin.TITLE_WARNING_DIALOG, MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }
            else
                return false;
        }
    }
}
