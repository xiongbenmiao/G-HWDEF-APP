using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using Sankyo.Robot.HostProtocol;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using RobotFWDownloadNET;

namespace G_H_Transplantation_For_AMAT
{
    public partial class New_Combination_new : Form
    {
        private ShpHostPrtcl shpcontrol = new ShpHostPrtcl();
        private ShpHostPrtclNode m_ShpHostPrtclNode = new ShpHostPrtclNode();
        private ShpSc3kCtrl sc5kcontrol = new ShpSc3kCtrl();
        bool allowclose = true;//当formのclosingイベントをトリガーするかどうか判断用
        int prestatus = 0;//controller接続状態を記録するGlobal variable
        public New_Combination_new()
        {
            InitializeComponent();
            SHPCmdResult rc;
            string all;
            string[] row;
            string path = Directory.GetCurrentDirectory() + "\\IP config.txt";//ipのpathを取得
            Resources.message.log(4, "Scene [New Combination (Only NEW process)] starts running.");
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
                    ncn_conip.Text = row[1];
                }
            }
            if ((rc = shpcontrol.Init("SOC:" + ncn_conip.Text, 0)) == SHPCmdResult.SHPCmdSuccess)
            {
                if ((rc = m_ShpHostPrtclNode.Init(ref shpcontrol, 0)) == SHPCmdResult.SHPCmdSuccess)
                {
                    rc = sc5kcontrol.Init(ref m_ShpHostPrtclNode, 8, 0);
                    sc5kcontrol.SHPEnableDispatchWinMsg(0);
                }
            }
        }

        /// <summary>
        /// Step 0: Firmware packageのpathを選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void imppath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dilog = new FolderBrowserDialog();
            dilog.SelectedPath = Application.StartupPath;
            dilog.Description = "Select  \"NEW\" folder which in current tool path.";
            if (dilog.ShowDialog() == DialogResult.OK)
            {
                firm_path.Text = dilog.SelectedPath;
            }
        }

        /// <summary>
        /// Step 0: Firmware packageのNew操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ncn_new_Click(object sender, EventArgs e)
        {
           ncn_new.Enabled = false;
            int New = 1;//startRobotFirmwareDownload関数の第3パラメータを指定し、コントローラ中身をNewする
            string szPath, szPort;
            szPort = ncn_conip.Text;
            szPath = firm_path.Text;
            if(firm_path.Text == "")
            {
                Resources.message.judge_message(200);
            }

            RobotFWDownload.startRobotFirmwareDownload(szPath, szPort, New);
            timer4.Enabled = true;//New進行状況を監視するtimerはここから有効
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
                DialogResult result = MessageBox.Show("Do you want to quit?", "INFORMATION", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
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
                Resources.message.log(4, "Scene [New Conbination (Only NEW process)] has ended.");
                Resources.message.log(4, "G-H Transplantation has ended.");
                Process.GetCurrentProcess().Kill();//appすべてのprocessを殺す。
                Application.Exit();//processを殺したら、全appをshutdown.
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            SHPCmdResult rc;
            ushort io = 2000;
            long data = 0;
            int curstatus = 0;
            if ((rc = sc5kcontrol.SHPSysFuncReadIOCmd(io, out data)) == SHPCmdResult.SHPCmdSuccess)
            {
                curstatus = 1;//現在接続状態を記録
                ncn_ipstatus.BackColor = Color.Green;
                ncn_ipstatus.ForeColor = Color.White;
                ncn_ipstatus.Text = " ONLINE";
            }
            else
            {
                curstatus = 2;//現在接続状態を記録
                ncn_ipstatus.BackColor = Color.Yellow;
                ncn_ipstatus.ForeColor = Color.Red;
                ncn_ipstatus.Text = "OFFLINE";
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
        /// <summary>
        /// progress barを操作する関数。古川さんのソースから移植
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="val"></param>
        public static void SetProgressBarValue(ProgressBar pb, int val)
        {
            if (pb.Value < val)
            {
                //値を増やす時
                if (val < pb.Maximum)
                {
                    //目的の値より一つ大きくしてから、目的の値にする
                    pb.Value = val + 1;
                    pb.Value = val;
                }
                else
                {
                    //最大値にする時
                    //最大値を1つ増やしてから、元に戻す
                    pb.Maximum++;
                    pb.Value = val + 1;
                    pb.Value = val;
                    pb.Maximum--;
                }
            }
            else
            {
                //値を減らす時は、そのまま
                pb.Value = val;
            }
        }
        /// <summary>
        /// step 0のnew進行状況を監視する。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer4_Tick(object sender, EventArgs e)
        {
            SHPCmdResult rt;
            int rc;
            int percentLeft;
            string errorReazon;
            rc = RobotFWDownload.getRobotFirmwareTransferStatus(out percentLeft, out errorReazon);
            SetProgressBarValue(progressBar_UpDownProgress, 100 - percentLeft);
            if (rc == (int)RobotFWDownload.TransferStatus.TransferStatus_Success/*成功*/)
            {
                timer4.Enabled = false;
                string lastUploadPath;
                RobotFWDownload.SRFirmUpDownGetLastUploadFilePath(out lastUploadPath);
                SetProgressBarValue(progressBar_UpDownProgress, 0);
                ncn_new.Enabled = true;
                //成功失敗に関係なく、とりあえずコントローラと再接続。
                if ((rt = shpcontrol.Init("SOC:" + ncn_conip.Text, 0)) == SHPCmdResult.SHPCmdSuccess)
                {
                    if ((rt = m_ShpHostPrtclNode.Init(ref shpcontrol, 0)) == SHPCmdResult.SHPCmdSuccess)
                    {
                        rt = sc5kcontrol.Init(ref m_ShpHostPrtclNode, 8, 0);
                        sc5kcontrol.SHPEnableDispatchWinMsg(0);
                    }
                }
                timer1.Enabled = true;//timer1の監視を有効する。
                Resources.message.judge_message(202);
            }
            else if (rc == (int)RobotFWDownload.TransferStatus.TransferStatus_Failed/*失敗*/)
            {
                timer4.Enabled = false;
                StringBuilder sb = new StringBuilder();
                SetProgressBarValue(progressBar_UpDownProgress, 0);
                //成功失敗に関係なく、とりあえずコントローラと再接続。
                if ((rt = shpcontrol.Init("SOC:" + ncn_conip.Text, 0)) == SHPCmdResult.SHPCmdSuccess)
                {
                    if ((rt = m_ShpHostPrtclNode.Init(ref shpcontrol, 0)) == SHPCmdResult.SHPCmdSuccess)
                    {
                        rt = sc5kcontrol.Init(ref m_ShpHostPrtclNode, 8, 0);
                        sc5kcontrol.SHPEnableDispatchWinMsg(0);
                    }
                }
                ncn_new.Enabled = true;
                Resources.message.judge_message(201);
                timer1.Enabled = true;//timer1の監視を有効する。
                return;
            }
        }
    }
}
