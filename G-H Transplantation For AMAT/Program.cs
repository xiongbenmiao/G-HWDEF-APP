using System;
using System.Windows.Forms;

namespace G_H_Transplantation_For_AMAT
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
           Select_Operation f1 = new Select_Operation();
            f1.Show();
           Application.Run();
        }
    }
}
