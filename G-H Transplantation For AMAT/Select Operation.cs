using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Sankyo.Robot.HostProtocol;
using System.IO;
using System.Diagnostics;

namespace G_H_Transplantation_For_AMAT
{
    public partial class Select_Operation : Form
    {

        private ShpHostPrtcl shpcontrol = new ShpHostPrtcl();
        private ShpHostPrtclNode m_ShpHostPrtclNode = new ShpHostPrtclNode();
        private ShpSc3kCtrl sc5kcontrol = new ShpSc3kCtrl();
        bool allowclose = true;//当formのclosingイベントをトリガーするかどうか判断用
        int prestatus = 0;//controller接続状態を記録するGlobal variable

        public Select_Operation()
        {
            InitializeComponent();
            SHPCmdResult rc;
            string all;
            string[] row;
            string path = Directory.GetCurrentDirectory() + "\\IP config.txt";//ipのpathを取得
            Resources.message.log(4, "G-H Transplantation starts running.");
            if (!File.Exists(path))//IPアドレスファイルの存在を確認、なければエラーを出す。
            {
                Resources.message.log(1, "Could not find IP configration file.");//logを残す
                MessageBox.Show("Could not find IP configration file.", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                return;
            }
            else
            {
                using (StreamReader sr = new StreamReader(path, Encoding.Default))//ipファイルを展開し、必要な情報を検索。
                {
                    all = sr.ReadToEnd();
                    row = all.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    denglu_conip.Text = row[1];
                }
            }
            if ((rc = shpcontrol.Init("SOC:" + denglu_conip.Text, 0)) == SHPCmdResult.SHPCmdSuccess)
            {
                if ((rc = m_ShpHostPrtclNode.Init(ref shpcontrol, 0)) == SHPCmdResult.SHPCmdSuccess)
                {
                    rc = sc5kcontrol.Init(ref m_ShpHostPrtclNode, 8, 0);
                    sc5kcontrol.SHPEnableDispatchWinMsg(0);
                }
            }
        }

        /// <summary>
        /// 提示メッセージ用関数
        /// </summary>
        /// <param name="judgecode"></param>
        public void prompt(int judgecode)
        {
            switch (judgecode)
            {
                case 0:
                    MessageBox.Show("All parts (Include manipulator, controller and aligner) already correspond to Generic HWDEF function. You need AMAT firmware transfer tool function [NEW] to finish it.", "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);//message for step 4
                    allowclose = true;//このmessageが出た時、New Combination機能に入らない。なので、New Combinationのbuttonで変更されたallowcloseをresetする。
                    break;
                case 1:
                    MessageBox.Show("All parts (Include manipulator, controller and aligner) already correspond to Generic HWDEF function. You need AMAT firmware transfer tool function [BAKCUP], [NEW] and [RESTORE]  to finish it.", "INFORMATION", MessageBoxButtons.OK, MessageBoxIcon.Information);//message for step 4
                    allowclose = true;//このmessageが出た時、Exchange機能に入らない。なので、Exchange機能に入らないのbuttonで変更されたallowcloseをresetする。
                    break;
            }
        }

        /// <summary>
        /// All New機能に入る
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void alne_Click(object sender, EventArgs e)
        {
            allowclose = false;//各clickイベントを忘れないで！
            timer4.Enabled = false;
            this.Close();
            Form1 f = new Form1();
            f.Show();
        }

        /// <summary>
        /// New Combination機能に入る
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void neco_Click(object sender, EventArgs e)
        {
            allowclose = false;//各clickイベントを忘れないで！
            timer4.Enabled = false;
            SHPCmdResult rc;
            short gcheck = 0;
            rc = sc5kcontrol.SHPSysFuncGHWCHKCmd(out gcheck);//gcheckの数値によりどの操作に入るかを判断
            this.Close();
            New_Combination_new f = new New_Combination_new();
            f.Show();
            //prompt(0);
        }

        /// <summary>
        /// Exchange機能に入る
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ex_Click(object sender, EventArgs e)
        {
            allowclose = false;//各clickイベントを忘れないで！
            timer4.Enabled = false;
            SHPCmdResult rc;
           // prompt(1);

        }

        /// <summary>
        /// 現在の窓を閉じる時に、確認窓を出す。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectOperation_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (allowclose == true)//選択buttonを押す場合、allowcloseはfalseになっているので、確認窓をpopupしない。
            {
                DialogResult result = MessageBox.Show("Do you want to quit?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (result == DialogResult.OK)
                {
                    e.Cancel = false;  //okを押すと、SelectOperationをclose
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
        /// <summary>
        /// SelectOperationをclose後、全appをshutdown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectOperation_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (allowclose == true)//選択buttonを押す場合、allowcloseはfalseになっているので、appをshutdownしない。
            {
                Resources.message.log(4, "G-H Transplantation has ended.");
                Process.GetCurrentProcess().Kill();//appすべてのprocessを殺す。
                Application.Exit();//processを殺したら、全appをshutdown.
            }
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            SHPCmdResult rc;
            ushort io = 2000;
            long data = 0;
            int curstatus = 0;
            if ((rc = sc5kcontrol.SHPSysFuncReadIOCmd(io, out data)) == SHPCmdResult.SHPCmdSuccess)
            {
                curstatus = 1;//現在接続状態を記録
                denglu_ipstatus.BackColor = Color.Green;
                denglu_ipstatus.ForeColor = Color.White;
                denglu_ipstatus.Text = " ONLINE";
            }
            else
            {
                curstatus = 2;//現在接続状態を記録
                denglu_ipstatus.BackColor = Color.Yellow;
                denglu_ipstatus.ForeColor = Color.Red;
                denglu_ipstatus.Text = "OFFLINE";
            }
            if (curstatus != prestatus)
            {
                if (curstatus == 1)
                {
                    Resources.message.log(3, "Controller is ONLINE.");
                }
                else if (curstatus == 2)
                {
                    Resources.message.log(3, "Controller is OFFLINE.");
                }
            }
            prestatus = curstatus;//現在接続状態を記録変数に渡す。
        }
    }
}
