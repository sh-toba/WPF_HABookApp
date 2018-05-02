using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HABookApp;

namespace UnitTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // LoginManageクラスのテスト
            MyUtils.LoginManage LM = new MyUtils.LoginManage();
            LM.TestEncryption();
        }
    }
}
