using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Sankyo.Robot.HostProtocol;
using System.Threading;
using System.IO;
using System.Collections;
using System.Diagnostics;
using RobotFWDownloadNET;

namespace G_H_Transplantation_For_AMAT
{
    public partial class Form1 : Form
    {
        public static Form1 form1;//別classに当formのコントローラを使えるため、form1変量を宣言
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;//多threadでコントロールを操作する許可
            form1 = this;//別classに当formのコントローラを使えるため、form1変量を宣言
            //最初applicationを起動するとき、protocolは一回のみを初期化する。後の再接続はtimer内のReadIO状態により判断する。
            SHPCmdResult rc;
            string all;
            string[] row;
            DateTime dt = DateTime.Now;
            string path = Directory.GetCurrentDirectory() + "\\IP config.txt";//ipのpathを取得
            Resources.message.log(4, "Scene [Append G-HWDEF To All Parts] starts running.");
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
                    sr.Close();
                    conip.Text = row[1];
                }
            }
            if ((rc = shpcontrol.Init("SOC:" + conip.Text, 0)) == SHPCmdResult.SHPCmdSuccess)
            {
                if ((rc = m_ShpHostPrtclNode.Init(ref shpcontrol, 0)) == SHPCmdResult.SHPCmdSuccess)
                {
                    rc = sc5kcontrol.Init(ref m_ShpHostPrtclNode, 8, 0);
                    //sc5kcontrol.SHPEnableDispatchWinMsg(0);
                }
            }
        }
        
        private ShpHostPrtcl shpcontrol = new ShpHostPrtcl();
        private ShpHostPrtclNode m_ShpHostPrtclNode = new ShpHostPrtclNode();
        private ShpSc3kCtrl sc5kcontrol = new ShpSc3kCtrl();
        int STEP10_FINISH = 0;//Timer1用的Global variable------0: 現在位置はstep 1の"Before Tran"に入る。　1: 現在位置は"After Tran"に入る
        int bintrans_status = 4;//Timer2用的Global variable-------　0: 転送中.   1: bin fileの転送はOK.    2: bin fileの転送はNG　3:ドライバー更新中。
        //int firmtrans_status = 0;//Timer3用的Global variable-------1: firmwareの転送はOK.    2: firmwareの転送はNG
        string SSLID = "";//Firmware転送用的Global variable------- G-HWDEFを転送する時、SSLIDはこの文字列に格納。(step 9の比較にも使う)
        int prestatus = 0;//controller接続状態を記録するGlobal variable
        int step1_ok = 0;//step 1で情報を取得したら、このflagを立つ。
        public int aligner_flag = 0;//step 1の旧HWDEF情報により、alignerあり・なしを記録。STEP 2に使われる。
        int file_num = 0;//step 8用。ファイルは何個目を転送したの記録
        int step4_ok = 0;//step 4の終了記録
        int step5_ok = 0;//step 5は成功したら、このflagを立つ。
        int step6_ok = 0;//step 6は成功したら、このflagを立つ。
        int step7_ok = 0;//step 7は成功したら、このflagを立つ。
        int step8_ok = 0;//step 8は成功したら、このflagを立つ。
        int driver_no = 0;//Driverのaxis数を記録
        public string dr_s = "";//driver versionはmessage.csに伝達用
        public string sc_s = "";//sc5000 versionはmessage.csに伝達用
        public string r_rbtp, r_linlinlin, r_linyiwu, r_yilinsan, r_yilinsi, r_yilinwu, a_linlinlin, a_linyiwu, a_yilinsan, a_yilinsi, a_yilinwu;//step 4の各種情報をmessage.csに伝達用

        /// <summary>
        ///Step 0:  firmware packageのpathを導入。step 0に使われるから...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sppath_Click(object sender, EventArgs e)
        {
            string[] fileinfo,arr_fname;
            string fname;           
            int length,length_fname;
            OpenFileDialog dilog = new OpenFileDialog();
            if (dilog.ShowDialog() == DialogResult.OK)
            {
                fileinfo = dilog.FileName.Split('\\');
                length = fileinfo.Length;
                fname = fileinfo[length - 1];
                arr_fname = fname.Split('.');//firmware packageの名前と拡張名を分離
                length_fname = arr_fname.Length;//分離された情報を保存する文字列組の長さを取得
                if (arr_fname[length_fname-1] != "prm")//指定されたファイルの拡張名は".prm"じゃなければ、エラーを出す。(最後の1位は必ず拡張名)
                {
                    Resources.message.judge_message(1);
                    return;
                }
                else
                {
                    firmpath.Text = dilog.FileName;//pathをtextboxに表示
                    firmname.Text = arr_fname[0]+"."+arr_fname[1];//firmware packageの名前をtextboxに表示
                    Resources.message.judge_message(4);
                }
            }
        }


        /// <summary>
        /// Step 0: Toolフォルダ内容確認＋Cont中身backup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fcheck_Click(object sender, EventArgs e)
        {
            int number = 0;
            DirectoryInfo motherfolder = new DirectoryInfo(Directory.GetCurrentDirectory());
            foreach (FileInfo NextFile in motherfolder.GetFiles())//必要なファイルの有無を全部check。
            {
                if (NextFile.Name == "sc5000.bin" || NextFile.Name == "5KSTDRV.bin" || NextFile.Name == "7-zip32.dll" || NextFile.Name == "hwdacc.dll" || NextFile.Name == "ICSharpCode.SharpZipLib.dll" || NextFile.Name == "RobotFWDownload.Sankyo.dll" || NextFile.Name == "Sc5kTeach.dll")
                {
                    number = number + 1;
                }
            }
            //必要なファイルをcheck
            if (number < 7) {//いずれのファイルは存在しない時、エラーを出す。
                Resources.message.judge_message(2);
                return;
            }
            //firmwareのpathを指定するかどうかをcheck
            if(firmpath.Text == "")
            {
                Resources.message.judge_message(3);
                return;
            }
            timer1.Enabled = false;//バックアップ終了後、startRobotFirmwareUpload関数によりコントとの接続は切断されるので、timer1のIO問い合わせを一時的に無効化。
            fcheck.Enabled = false;
            int backup = 1;//startRobotFirmwareUpload関数の第3パラメータを指定し、コントローラ中身をバックアップ
            string szPath, szPort, path;
            szPort = conip.Text;
            path = Directory.GetCurrentDirectory();//exeのpathを取得
            szPath = Path.Combine(path, "BACKUP");//上記pathに"BACKUP"pathを追加
            if (!Directory.Exists(szPath))
            {//なければ、"BACKUP"フォルダを生成
                Directory.CreateDirectory(szPath);
            }
            RobotFWDownload.startRobotFirmwareUpload(szPath, szPort, backup);
            timer4.Enabled = true;//backup進行状況を監視するtimerはここから有効
        }

        /// <summary>
        /// step 1: 旧HWDEFを吸い上げ、必要な情報を取得
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gethwinfo_Click(object sender, EventArgs e)
        {
            gethwinfo.Enabled = false;
            SHPCmdResult rc;
            string all;
            string[] row;
            string path = Directory.GetCurrentDirectory();//exeのpathを取得
            string path1 = Path.Combine(path, "OLD HWDEF");//上記pathに"Old HWDEF"pathを追加
            string filename = "HWDEF5.CFG";
            int count = 0;//同じ情報を何回がでる判断flag
            if (!Directory.Exists(path1))
            {//なければ、"Old HWDEF"フォルダを生成
                Directory.CreateDirectory(path1);
            }
            string path2 = path1 + "\\HWDEF5.CFG";
            if ((rc = sc5kcontrol.SHPUploadFileCmd(path2, filename, SHPFileType.SHPHwdefFile, SHPDriveType.SHPDriveFLASH)) == SHPCmdResult.SHPCmdSuccess)
            {
                using (StreamReader sr = new StreamReader(path2, Encoding.Default))//保存したばかりのHWDEFを開き、必要な情報を抽出。
                {
                    all = sr.ReadToEnd();
                    row = all.Split(new string[] { "\r\n" }, StringSplitOptions.None);//HWDEFすべての情報を一行一行でrow[]に渡す
                    sr.Close();
                    //逐行遍历整个字符组
                    foreach (string i in row)
                    {
                        if (i.Length >= 10)
                        {//文字列の長さは8より長い時。
                            if (i.Substring(0, 7) == "#RBTYPE" && i.Substring(0, 8) != "#RBTYPE2" && i.Substring(0, 11) != "#RBTYPE_SUB")
                            {
                                h_rbtype.Text = i;
                                r_rbtp = i;//旧HWDEF情報を伝達文字列に渡す。
                            }
                            if (i.Substring(0, 10) == "%robot_cfg")//以下の情報はrobotとalignerに両方存在するので、%robot_cfgの出没回数で区別する。
                            {
                                count = count + 1;
                            }
                            if (count == 1)
                            {
                                if (i.Substring(0, 4) == "#000")//system configに#000もある。結局robot configの#000に上書きされる。
                                {
                                    h_000.Text = i;
                                    r_linlinlin = i;//旧HWDEF情報を伝達文字列に渡す。
                                }
                                if (i.Substring(0, 4) == "#015")
                                {
                                    h_shiwu.Text = i;
                                    r_linyiwu = i;//旧HWDEF情報を伝達文字列に渡す。
                                }
                                if (i.Substring(0, 4) == "#103")
                                {
                                    h_yaolingsan.Text = i;
                                    r_yilinsan = i;//旧HWDEF情報を伝達文字列に渡す。
                                }
                                if (i.Substring(0, 4) == "#104")
                                {
                                    h_yaolingsi.Text = i;
                                    r_yilinsi = i;//旧HWDEF情報を伝達文字列に渡す。
                                }
                                if (i.Substring(0, 4) == "#105")
                                {
                                    h_yaolingwu.Text = i;
                                    r_yilinwu = i;//旧HWDEF情報を伝達文字列に渡す。
                                    //count = 1;
                                }
                            }
                            else if (count == 2)
                            {
                                if (i.Substring(0, 4) == "#000")
                                {
                                    ah_000.Text = i;
                                    a_linlinlin = i;//旧HWDEF情報を伝達文字列に渡す。
                                }
                                if (i.Substring(0, 4) == "#015")
                                {
                                    ah_shiwu.Text = i;
                                    a_linyiwu = i;//旧HWDEF情報を伝達文字列に渡す。
                                }
                                if (i.Substring(0, 4) == "#103")
                                {
                                    ah_yaolingsan.Text = i;
                                    a_yilinsan = i;//旧HWDEF情報を伝達文字列に渡す。
                                }
                                if (i.Substring(0, 4) == "#104")
                                {
                                    ah_yaolingsi.Text = i;
                                    a_yilinsi = i;//旧HWDEF情報を伝達文字列に渡す。
                                }
                                if (i.Substring(0, 4) == "#105")
                                {
                                    ah_yaolingwu.Text = i;
                                    a_yilinwu = i;//旧HWDEF情報を伝達文字列に渡す。
                                    //count = 2;
                                }
                                aligner_flag = 1;//alignerありのflagを立つ。
                            }
                            else if (count == 3)//aligner以外のタスクのため、予備
                            {

                            }
                        }
                    }
                }
                step1_ok = 1;
                Resources.message.judge_message(19);
                gethwinfo.Enabled = true;//処理成功後にbuttonのfreezeを解除
                tabControl1.SelectTab(2);
            }
            else
            {
                gethwinfo.Enabled = true;//buttonのfreezeを解除
                Resources.message.judge_message(10);//controllerからhwdefを取得できなかったら、エラーを出す。
                return;
            }
        }

        /// <summary>
        /// step 2:ロボットの現在位置を取得。step 9は成功後、もう一回取得する必要がある
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbpos_Click(object sender, EventArgs e)
        {
            SHPCmdResult rc,rc2,rc3,rc4;
            SHPCartePos pos_rb,pos_al;
            byte index_rb = 1;
            byte index_al = 2;
            double x, y, z, s1, s2,aligner ;
            string path = Directory.GetCurrentDirectory()+"\\SC5K_SN.CFG";//SC5K_SNのpathを指定
            if ((rc = sc5kcontrol.SHPGetCartePosCmd(index_rb, out pos_rb)) == SHPCmdResult.SHPCmdSuccess)
            {
                x = Math.Round(Convert.ToDouble(pos_rb.Positions[0]), 2);
                y = Math.Round(Convert.ToDouble(pos_rb.Positions[1]), 2);
                z = Math.Round(Convert.ToDouble(pos_rb.Positions[2]), 2);
                s1 = Math.Round(Convert.ToDouble(pos_rb.Positions[3]), 2);
                s2 = Math.Round(Convert.ToDouble(pos_rb.Positions[4]), 2);
                if (aligner_flag == 1)//flagによりaligner座標を取得。ない場合は取得すると、コントローラは通信不能な状態になる。
                {
                    if ((rc2 = sc5kcontrol.SHPGetCartePosCmd(index_al, out pos_al)) == SHPCmdResult.SHPCmdSuccess)//alignerはrobot task 2になる。
                    {
                        aligner = Math.Round(Convert.ToDouble(pos_al.Positions[0]), 2);
                    }
                    else
                    {
                        SHPResultCode resultcode;
                        long code1, code2;
                        sc5kcontrol.SHPGetLastError(out resultcode, out code1, out code2);//debug用
                        aligner = 0;
                    }
                }else
                {
                    aligner = 0;//alignerなし場合、座標は""
                }
                
                if (STEP10_FINISH == 0)
                {
                    xb.Text = x.ToString();
                    yb.Text = y.ToString();
                    zb.Text = z.ToString();
                    s1b.Text = s1.ToString();
                    s2b.Text = s2.ToString();
                    if (aligner_flag == 1)
                    {
                        alb.Text = aligner.ToString();
                    }else
                    {
                        alb.Text = "Null";
                    }
                    //直接メモリーからcont typeとsnを呼び出す。
                    byte baseregste = 1;
                    uint baseadd_con = 2684878800;//A007FFD0番地
                    uint baseadd_sn = 2684878816;//A007FFED番地
                    ushort offadd = 0;
                    byte addlen = 16;
                    int count;
                    byte[] data = { 0 };
                    string conttp_array = "";
                    string sn_array = "";
                    rc3 = sc5kcontrol.SHPSetBaseAddressCmd(baseregste, baseadd_con);//cont typeのメモリーアドレスを指定
                    if ((rc3 = sc5kcontrol.SHPMemoryReadCmd(baseregste, offadd, addlen, out data)) == SHPCmdResult.SHPCmdSuccess)
                    {
                        count = 0;
                        while (count < 15) {
                            conttp_array = conttp_array + Resources.transform.NunToChar(data[count]);
                            count = count + 1;
                         }
                        cur_ct.Text = conttp_array;//順次読み取ったcont typeをtextboxで表示
                    }else
                    {
                        cur_ct.Text = "Null";//ない場合,"Null"で表示
                    }
                    rc4 = sc5kcontrol.SHPSetBaseAddressCmd(baseregste, baseadd_sn);//snのメモリーアドレスを指定
                    if ((rc4 = sc5kcontrol.SHPMemoryReadCmd(baseregste, offadd, addlen, out data)) == SHPCmdResult.SHPCmdSuccess)
                    {
                        count = 0;
                        while (count < 10)
                        {
                            sn_array = sn_array + Resources.transform.NunToChar(data[count]);
                            count = count + 1;
                            cur_sn.Text = sn_array;//順次読み取ったsnをtextboxで表示
                        }
                    }else
                    {
                        cur_sn.Text = "Null";//ない場合,"Null"で表示
                    }
                    Resources.message.judge_message(29);
                    tabControl1.SelectTab(3);
                }
                else
                {
                    xa.Text = x.ToString();
                    ya.Text = y.ToString();
                    za.Text = z.ToString();
                    s1a.Text = s1.ToString();
                    s2a.Text = s2.ToString();
                    if (aligner_flag == 1)
                    {
                        ala.Text = aligner.ToString();
                    }
                    else {
                        ala.Text = "Null";
                    }
                    Resources.message.judge_message(20);
                }
            }
            else
            {
                Resources.message.judge_message(21);//現在位置の取得は失敗したら、エラーを出す。
                return;
            }
        }
        /// <summary>
        /// step 3:S/Nファイルをコントローラに転送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sndl_Click(object sender, EventArgs e)
        {
            SHPCmdResult rc;
            string path,sc5k, rbconst_path,all,contype;
            string sc5kcontent;
            string[] row;
            string zip_path = "";
            string unzip_name = "";
            string filename = "SC5K_SN.CFG";
            sc5k = "";
            contype = "";
            sc5k = Directory.GetCurrentDirectory() + "\\SC5K_SN.CFG";
            sc5kcontent = Resources.transform.ToDBC(controllertype.Text) + "/" + Resources.transform.ToDBC(snno.Text);//ToDBC関数により、全角で入力してしまう時、半角で変換する。
            sndl.Enabled = false;//転送開始後、buttonをfreezeする。
            //RBCONST.DEF中にuserは入力したcont typeを探す。なければエラーを出す。
            //上記操作を完成するため、まずfirmwareを解凍する。
            //Step 0でfirmwareを指定するかどうかをcheck
            if (firmpath.Text == "")
            {
                sndl.Enabled = true;
                Resources.message.judge_message(23);
                return;
            }
            else
            {
                zip_path = firmpath.Text;
                unzip_name = firmname.Text;//名前をtextboxから取得
                string dirpath = Directory.GetCurrentDirectory() + "\\" + unzip_name;
                Resources.Zip.unZipFile(zip_path, dirpath);//元ファイル存在する時、すべて上書き。
            }
            //解凍されたら、RBCONST.DEFのpathを取得。
              rbconst_path = Directory.GetCurrentDirectory() + "\\" + unzip_name + "\\HWDEF5\\RBCONST.DEF";
            using (StreamReader sr = new StreamReader(rbconst_path, Encoding.Default))//DEFファイルを展開し、必要な情報を検索。
            {
                all = sr.ReadToEnd();
                row = all.Split(new string[] { "\r\n" }, StringSplitOptions.None);//DEFすべての情報を一行一行でrow[]に渡す
                sr.Close();
                foreach (string i in row)
                {
                    if (i.Length > 2)
                    {
                        if (i.Substring(12, 15) == controllertype.Text.ToUpper())//RBCONST.DEFの２列目でuserで入力したコントtypeを探す。
                        {
                            contype = i;
                        }
                    }
                }
            }
            //userで入力したcont typeはrbconst.defに存在しなければ、エラーを出す。
            if (contype == "")
            {
                sndl.Enabled = true;//buttonのfreezeを解除
                Resources.message.judge_message(32);
                return;
            }
            //桁数をcheck(26桁はOK)
            else if (sc5kcontent.Length != 26)
            {
                sndl.Enabled = true;
                Resources.message.judge_message(31);
                return;
            }else
            {
                File.AppendAllText(sc5k, sc5kcontent);//SC5K_SNを生成。(writelinesは駄目。改行は入ってしまうから)             
            }
            path = Directory.GetCurrentDirectory() + "\\SC5K_SN.CFG";//指定路径还得包括完整文件名，坑爹啊！
            //二つ場面
            //                                                                             |------user input is different from current => based on user's selection, continue transfer or interrupt
            //１．コントローラは既にTypeとSNを持っている----|
            //                                                                             |------user input is same with current => transfer
            //
            //2. コントローラはTYPEとSNをもっていない => transfer
            if (cur_ct.Text != "Null")//TypeとSNを既に持っている
            {
                if (cur_ct.Text != controllertype.Text || cur_sn.Text != snno.Text)//User input is different from current
                {
                    Resources.message.judge_message(34);//Pop up selection window. 
                    if(Resources.message.sn_trans == 1)//Continue transfer
                    {
                        //2回を試す。だめならエラーを出す。
                        if ((rc = sc5kcontrol.SHPDownloadFileCmd(path, filename, SHPFileType.SHPObjectFile, SHPDriveType.SHPDriveFLASH)) != SHPCmdResult.SHPCmdSuccess)
                        {
                            if (sc5kcontrol.SHPDownloadFileCmd(path, filename, SHPFileType.SHPObjectFile, SHPDriveType.SHPDriveFLASH) != SHPCmdResult.SHPCmdSuccess)
                            {
                                sndl.Enabled = true;//転送失敗の時、buttonのfreezeを解除
                                File.Delete(sc5k);//転送失敗でもファイルを削除
                                Resources.message.judge_message(30);
                                return;
                            }
                        }
                        else
                        {
                            File.Delete(sc5k);
                            sndl.Enabled = true;//転送成功後、buttonのfreezeを解除
                            Resources.message.judge_message(39);
                            tabControl1.SelectTab(4);
                        }
                    }
                    else//Interrupt transfer
                    {
                        File.Delete(sc5k);//転送を諦めてもファイルを削除
                        sndl.Enabled = true;//転送を諦める後buttonのfreezeを解除
                        return;
                    }
                }else//User input is same with current
                {
                    //2回を試す。だめならエラーを出す。
                    if ((rc = sc5kcontrol.SHPDownloadFileCmd(path, filename, SHPFileType.SHPObjectFile, SHPDriveType.SHPDriveFLASH)) != SHPCmdResult.SHPCmdSuccess)
                    {
                        if (sc5kcontrol.SHPDownloadFileCmd(path, filename, SHPFileType.SHPObjectFile, SHPDriveType.SHPDriveFLASH) != SHPCmdResult.SHPCmdSuccess)
                        {
                            sndl.Enabled = true;//転送失敗の時、buttonのfreezeを解除
                            File.Delete(sc5k);//転送失敗でもファイルを削除
                            Resources.message.judge_message(30);
                            return;
                        }
                    }
                    else
                    {
                        File.Delete(sc5k);
                        sndl.Enabled = true;//転送成功後、buttonのfreezeを解除
                        Resources.message.judge_message(39);
                        tabControl1.SelectTab(4);
                    }
                }
            }
            else//TypeとSNをもっていない
            {
                //2回を試す。だめならエラーを出す。
                if ((rc = sc5kcontrol.SHPDownloadFileCmd(path, filename, SHPFileType.SHPObjectFile, SHPDriveType.SHPDriveFLASH)) != SHPCmdResult.SHPCmdSuccess)
                {
                    if (sc5kcontrol.SHPDownloadFileCmd(path, filename, SHPFileType.SHPObjectFile, SHPDriveType.SHPDriveFLASH) != SHPCmdResult.SHPCmdSuccess)
                    {
                        sndl.Enabled = true;//転送失敗の時、buttonのfreezeを解除
                        Resources.message.judge_message(30);
                        return;
                    }
                }
                else
                {
                    File.Delete(sc5k);//転送後ファイルを削除
                    sndl.Enabled = true;//転送成功後、buttonのfreezeを解除
                    Resources.message.judge_message(39);
                    tabControl1.SelectTab(4);
                }
            }
        }

        /// <summary>
        /// step 4:BINファイルをコントローラに転送また更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bindl_Click(object sender, EventArgs e)
        {
            bindl.Enabled = false;
            timer2.Enabled = true;//転送状況監視を開始。
            byte device_id = 0;//system firmware更新用(0: 2軸ドライバー)
            byte subb_id = 0;//system firmware更新用(軸番号を指定する変数、0は1軸！)
            int subi_id = 0;//system firmware更新用(軸番号をカウンタする変数)
            int count = 0;//SR7173: count = 6      SR8241: count = 8
            bintrans_status = 0;
            transferstatus.BackColor = Color.DarkBlue;
            transferstatus.ForeColor = Color.White;
            transferstatus.Text = "Process Status:        Transferring";
            SHPCmdResult rc1;
            SHPCmdResult rc2;
            string filename1 = "sc5000.bin";
            string filename2 = "5KSTDRV.bin";
            string path1 = Directory.GetCurrentDirectory() + "\\sc5000.bin";//指定路径还得包括完整文件名，坑爹啊！
            string path2 = Directory.GetCurrentDirectory() + "\\5KSTDRV.bin";//指定路径还得包括完整文件名，坑爹啊
            Application.DoEvents();
            //ドライバーを更新すべき軸数を取得。また、選択されたロボット機種とstep 2で入力したコントローラタイプを一致するかどうかをcheck
            if (rbsel.Text == "SR7173")
            {
                count = 6;//SR7173は6軸
                if (controllertype.Text == "SC5000-EX2-2001")
                {
                    bintrans_status = 2;
                    bindl.Enabled = true;
                    Resources.message.judge_message(41);
                    return;
                }
            }
            else
            {
                count = 8;//SR8241は8軸
                if (controllertype.Text != "SC5000-EX2-2001")
                {
                    bintrans_status = 2;
                    bindl.Enabled = true;
                    Resources.message.judge_message(42);
                    return;
                }
            }
            driver_no = count;
            //先にsc5000.binを転送(2回試す)
            if ((rc1 = sc5kcontrol.SHPDownloadFileCmd(path1, filename1, SHPFileType.SHPControllerBINFile, SHPDriveType.SHPDriveFLASH)) != SHPCmdResult.SHPCmdSuccess)
            {
                if (sc5kcontrol.SHPDownloadFileCmd(path1, filename1, SHPFileType.SHPControllerBINFile, SHPDriveType.SHPDriveFLASH) != SHPCmdResult.SHPCmdSuccess)
                {
                    bintrans_status = 2;
                    bindl.Enabled = true;//転送失敗の時、buttornのreleaseを解除
                    Resources.message.judge_message(40);
                    return;
                }
            }
            else
            {
                if ((rc2 = sc5kcontrol.SHPDownloadFileCmd(path2, filename2, SHPFileType.SHPControllerBINFile, SHPDriveType.SHPDriveFLASH)) != SHPCmdResult.SHPCmdSuccess)
                {
                    if (sc5kcontrol.SHPDownloadFileCmd(path2, filename2, SHPFileType.SHPControllerBINFile, SHPDriveType.SHPDriveFLASH) != SHPCmdResult.SHPCmdSuccess)
                    {
                        bintrans_status = 2;
                        bindl.Enabled = true;//転送失敗の時、buttornのreleaseを解除
                        Resources.message.judge_message(43);
                        return;
                    }
                }
                else
                {
                    bintrans_status = 10;
                    transferstatus.BackColor = Color.DarkBlue;//updating中,labelの背景は緑になる
                    transferstatus.ForeColor = Color.Yellow;
                    Thread.Sleep(500);
                    //system firmware更新に入る。
                    if ((rc1 = sc5kcontrol.SHPEnterROMCmd()) == SHPCmdResult.SHPCmdSuccess)
                    {
                        Thread.Sleep(500);//完全に書き込みモードに入ったら、ドライバーの更新作業に入る。
                        while (subi_id < count)
                        {
                            subb_id = (byte)subi_id;
                            bintrans_status = 10 + subi_id;
                            Application.DoEvents();
                            if ((rc2 = sc5kcontrol.SHPUpdateFirmwareCmd(device_id, subb_id)) != SHPCmdResult.SHPCmdSuccess)
                            {
                                SHPResultCode code;
                                long data1, data2;
                                sc5kcontrol.SHPGetLastError(out code, out data1, out data2);

                                bintrans_status = 2;
                                bindl.Enabled = true;//転送失敗の時、buttornのreleaseを解除
                                Resources.message.judge_message(44);
                                return;
                            }
                            //debug用
                            subi_id = subi_id + 1;
                        }
                    }
                    else
                    {
                        bintrans_status = 2;
                        bindl.Enabled = true;//転送失敗の時、buttornのreleaseを解除
                        Resources.message.judge_message(45);
                        return;
                    }
                    step4_ok = 1;
                    bintrans_status = 1;
                    bindl.Enabled = true;
                    reboot_m.Visible = true;//gif"Please reboot controller by manual."を表示する。
                    groupBox5.Enabled = false;//step 5を凍結する。
                }
            }
            //debug用
            //step4_ok = 1;
            //bintrans_status = 1;
            //bindl.Enabled = true;
            //Resources.message.judge_message(7);
            //tabControl1.SelectTab(4);
        }
  
        /// <summary>
        /// step 5: G-HWDEFを生成し、コントローラに転送また再起動
        /// </summary>
        /// <param name="sender"></param>
       /// <param name="e"></param>
        private void transfergh_Click(object sender, EventArgs e)
        {
            SHPCmdResult rc;
            transfergh.Enabled = false;//buttonをfreezeする
            string unzip_name = "";
            string thw_path,rbconst_path;
            int task_no = 0;//task総数
            long row1_no = 0;
            long row2_no = 0;
            ushort rebootio = 949;
            byte on = 1;
            string task1_path, task1_target;
            string all, rbtype, task1_rbtype2,task2_rbtype2, rbtype3,hwid1,hwid2,hwid3,sub,task1_all,task2_all, hwid1_name;
            task1_rbtype2 = "";//Robot和aligner的#RBTYPE2不是一个东西，坑爹啊！
            task2_rbtype2 = "";
            hwid2 = "";
            task1_target = "";
            string[] row,task1_row,task2_row,rbinfo;
            string namarbinfo = "";
            DirectoryInfo motherfolder = new DirectoryInfo(Directory.GetCurrentDirectory());
            //step 3で既に解凍したので、ここの解凍を諦める。
            unzip_name = firmname.Text;//名前をtextboxから取得
            thw_path = Directory.GetCurrentDirectory() + "\\" + unzip_name + "\\HWDEF5";
                rbconst_path = Directory.GetCurrentDirectory() + "\\" + unzip_name + "\\HWDEF5\\RBCONST.DEF";
                if (!File.Exists(rbconst_path))//DEFファイルの存在を確認、なければエラーを出す。
            {
                transfergh.Enabled = true;
                Resources.message.judge_message(50);
                return;
            }
            if (step1_ok == 0)//step 1で旧HWDEF情報を取得しなければ、エラーを出す。
            {
                transfergh.Enabled = true;
                Resources.message.judge_message(54);
                return;
            }
            else
            {
                using (StreamReader sr = new StreamReader(rbconst_path, Encoding.Default))//DEFファイルを展開し、必要な情報を検索。
                {
                    all = sr.ReadToEnd();
                    row = all.Split(new string[] { "\r\n" }, StringSplitOptions.None);//DEFすべての情報を一行一行でrow[]に渡す
                    sr.Close();
                    foreach (string i in row)
                    {
                        if (i.Length > 2)
                        {
                            if (i.Substring(0, 11) == h_rbtype.Text.Substring(9,11))
                            {
                                namarbinfo = i;
                            }
                        }
                    }
                    //遍历DEF完事假使没有对应机种的信息，报错
                    if (namarbinfo == "")
                    {
                        transfergh.Enabled = true;//buttonのfreezeを解除
                        Resources.message.judge_message(51);
                        return;
                    }
                    else
                    {
                        rbinfo = namarbinfo.Split(',');
                        rbtype = rbinfo[0];
                        hwid1 = rbinfo[2];
                        task1_rbtype2 = rbinfo[3];
                        sub = rbinfo[4];
                        if (rbinfo[5] != "-")//Task2 HWDEF IDがあるとき、IDとTask2の#RBTYPE2を抽出。
                        {
                            hwid2 = rbinfo[5];
                            task2_rbtype2 = rbinfo[6];
                            task_no = 2;
                        }
                        if (rbinfo[7] != "-")
                        {
                            hwid3 = rbinfo[7];
                            rbtype3 = rbinfo[8];
                            task_no = 3;
                        }
                        //  System.IO.File.WriteAllLine
                        hwid1_name = hwid1 + ".HW1";
                        task1_path = thw_path + "\\" + hwid1_name;//path== 解凍されたfirmware package folder + "HWDEF5" + HWID.HWX (X = task番号)
                        task1_target = Directory.GetCurrentDirectory() + "\\G-HWDEF\\HWDEF5.CFG";
                        if (!File.Exists(task1_path))//task1テンプレーHWDEFなければ...
                        {
                            transfergh.Enabled = true;//buttonのfreezeを解除
                            Resources.message.judge_message(52);
                            return;
                        }
                        else
                        {//とりあえずtask1のテンプレートG-HWDEFを展開
                            using (StreamReader sp = new StreamReader(task1_path, Encoding.Default))//ファイルを展開し、必要な情報を検索。
                            {
                                task1_all = sp.ReadToEnd();
                                task1_row = task1_all.Split(new string[] { "\r\n" }, StringSplitOptions.None);//テンプレートHW1すべての情報を一行一行でrow[]に渡す
                                row1_no = task1_row.GetLength(0);
                                int t = 0;
                                while (t < row1_no)
                                {
                                    if (task1_row[t].Length >= 8)
                                    {
                                        //テンプレートの#RBTYPEはDEFファイルの#RBTYPEで変える。
                                        if (task1_row[t].Substring(0, 7) == "#RBTYPE" && task1_row[t].Substring(0, 8) != "#RBTYPE2" && task1_row[t].Substring(0, 8) != "#RBTYPE_")
                                        {
                                            task1_row[t] = "#RBTYPE" + "  " + rbtype;
                                        }
                                        //テンプレートの#RBTYPE_SUBはDEFファイルの#RBTYPE_SUBで変える。
                                        if (task1_row[t].Substring(0, 8) == "#RBTYPE_")
                                        {
                                            task1_row[t] = "#RBTYPE_SUB" + "  " + sub;
                                        }
                                        //aligner存在するのみ、#RBTYPE2をDEFのやつで変える
                                        if (task1_row[t].Substring(0, 8) == "#RBTYPE2")
                                        {
                                            task1_row[t] = "#RBTYPE2" + "  " + task1_rbtype2;
                                        }
                                        //system configの#000ではなく、robot configの#000を書き換える。
                                        if (t > 7)
                                        {
                                            //step 4のrb series番号で書き換え
                                            if (task1_row[t].Substring(0, 4) == "#000")
                                            {
                                                task1_row[t] = h_000.Text;
                                            }
                                        }

                                        //step 4のrb #015番号で書き換え
                                        if (task1_row[t].Substring(0, 4) == "#015")
                                        {
                                            task1_row[t] = h_shiwu.Text;
                                        }

                                        //step 4のrb #103番号で書き換え
                                        if (task1_row[t].Substring(0, 4) == "#103")
                                        {
                                            task1_row[t] = h_yaolingsan.Text;
                                        }

                                        //step 4のrb #104番号で書き換え
                                        if (task1_row[t].Substring(0, 4) == "#104")
                                        {
                                            task1_row[t] = h_yaolingsi.Text;
                                        }

                                        //step 4のrb #105番号で書き換え
                                        if (task1_row[t].Substring(0, 4) == "#105")
                                        {
                                            task1_row[t] = h_yaolingwu.Text;
                                        }

                                        //step 8用のSSLIDを取得
                                        if (task1_row[t].Substring(0, 6) == "#SSLID")
                                        {
                                            SSLID = task1_row[t].Substring(8, 1);
                                        }
                                    }
                                    t = t + 1;
                                }
                            }
                            string ghw_path = Path.Combine(Directory.GetCurrentDirectory(), "G-HWDEF");//"G-HWDEF"pathを追加
                            if (!Directory.Exists(ghw_path)) {//なければ、"G-HWDEF"フォルダを生成
                                 Directory.CreateDirectory(ghw_path);
                             }
                            File.WriteAllLines(task1_target, task1_row,Encoding.Default);//task1のG-HWDEFを"G-HWDEF"フォルダ内に生成
                        }
                        if (task_no == 2)//task2あり(大体alignerがある時..)
                       {
                            string hwid2_name = hwid2 + ".HW2";
                            string task2_path = thw_path + "\\" + hwid2_name;
                            if (!File.Exists(task2_path))//task2テンプレーHWDEFなければ...
                            {
                                transfergh.Enabled = true;//buttonのfreezeを解除
                                Resources.message.judge_message(52);
                                return;
                            }
                            else
                            {
                                using (StreamReader sk = new StreamReader(task2_path, Encoding.Default))//ファイルを展開し、必要な情報を検索。
                                {
                                    task2_all = sk.ReadToEnd();
                                    task2_row = task2_all.Split(new string[] { "\r\n" }, StringSplitOptions.None);//テンプレートHW2すべての情報を一行一行でrow[]に渡す
                                    row2_no = task2_row.GetLength(0);
                                    int t = 0;
                                    while (t < row2_no)
                                    {
                                        if (task2_row[t].Length >= 8)
                                        {
                                            if (task2_row[t].Substring(0, 8) == "#RBTYPE2")
                                            {
                                                task2_row[t] = "#RBTYPE2" + "  " + task2_rbtype2;
                                            }

                                            //step 4のaligner series番号で書き換え
                                            if (task2_row[t].Substring(0, 4) == "#000")
                                            {
                                                task2_row[t] = ah_000.Text;
                                            }

                                            //step 4のaligner #015番号で書き換え
                                            if (task2_row[t].Substring(0, 4) == "#015")
                                            {
                                                task2_row[t] = ah_shiwu.Text;
                                            }

                                            //step 4のaligner #103番号で書き換え
                                            if (task2_row[t].Substring(0, 4) == "#103")
                                            {
                                                task2_row[t] = ah_yaolingsan.Text;
                                            }

                                            //step 4のaligner #104番号で書き換え
                                            if (task2_row[t].Substring(0, 4) == "#104")
                                            {
                                                task2_row[t] = ah_yaolingsi.Text;
                                            }

                                            //step 4のaligner #105番号で書き換え
                                            if (task2_row[t].Substring(0, 4) == "#105")
                                            {
                                                task2_row[t] = ah_yaolingwu.Text;
                                            }
                                        }
                                            t = t + 1;
                                    }
                                }
                                File.AppendAllLines(task1_target,task2_row,Encoding.Default);//task1のG-HWDEFにtask2の内容を追加。
                            }
                        }
                        if (task_no == 3)//予備
                        {
                        }
                    }
                }
            }
            if ((rc = sc5kcontrol.SHPDownloadFileCmd(task1_target, "HWDEF5.CFG", SHPFileType.SHPHwdefFile, SHPDriveType.SHPDriveFLASH)) != SHPCmdResult.SHPCmdSuccess)
            {
                Thread.Sleep(5000);//手太快点了按钮怎么办？大兄弟儿慢慢等着吧。。。
                if (sc5kcontrol.SHPDownloadFileCmd(task1_target, "HWDEF5.CFG", SHPFileType.SHPHwdefFile, SHPDriveType.SHPDriveFLASH) != SHPCmdResult.SHPCmdSuccess)
                {
                    transfergh.Enabled = true;//転送失敗の時、buttonのfreezeを解除
                    Resources.message.judge_message(53);
                    return;
                }
            }
            if(rc == SHPCmdResult.SHPCmdSuccess)//前記のすべて操作は問題なければ、コントローラを再起動
            {
                //再起動IO949を操作できるため、まずoperationモードに切り替える。
                if ((rc = sc5kcontrol.SHPModeSetCmd(SHPModeType.SHPModeOperation)) == SHPCmdResult.SHPCmdSuccess)
                {
                    Thread.Sleep(1000);
                    if ((rc = sc5kcontrol.SHPSysFuncWriteIOCmd(rebootio, on)) != SHPCmdResult.SHPCmdSuccess)
                    {
                        transfergh.Enabled = true;//再起動失敗の時、buttonのfreezeを解除
                        Resources.message.judge_message(55);
                        return;
                    }
                       // rc = sc5kcontrol.SHPSysFuncWriteIOCmd(rebootio, on); //コントローラを再起動。
                }
            }
            transfergh.Enabled = true;//buttonのfreezeを解除
            reboot_st5.Visible = true;
            step5_ok = 1;
            groupBox6.Enabled = false;//step 6を凍結する。
            Resources.message.log(2, "Controller is automatically rebooting during step 5.");//logに自動再起動を記録
                                                                                             // Resources.message.judge_message(59);
                                                                                             //tabControl1.SelectTab(6);
        }

        /// <summary>
        /// step 6: G-HWDEFの情報は各モータに書き込み
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wrtomt_Click(object sender, EventArgs e)
        {
            SHPCmdResult rc;
            ushort task = 15;//16進の0x0f
            ushort axis = 255;//16進の0xff
            ushort param = 4095;//16進の0xfff
            short psts = 0;
            st6_bacha.Visible = false;//"×"を初期化
            st6_gou.Visible = false;//"√"を初期化
            wrtomt.Enabled = false;
            if((rc = sc5kcontrol.SHPSysFuncGHWCPYCmd(task, axis, param,out psts)) != SHPCmdResult.SHPCmdSuccess)
            {
                SHPResultCode code;
                long data1, data2;
                sc5kcontrol.SHPGetLastError(out code, out data1, out data2);
                wrtomt.Enabled = true;
                Resources.message.judge_message(60);
                return;
            }
            if(psts != 0)//転送判断変量は0じゃないと、エラー
            {
                wrtomt.Enabled = true;
                st6_bacha.Visible = true;//"×"を表示する
                Resources.message.judge_message(60);
                return;
            }
            wrtomt.Enabled = true;
            step6_ok = 1;
            st6_gou.Visible = true;//"√"を表示する
            transdef.Enabled = true;//step 7のbuttonを解放する。
            Resources.message.judge_message(69);
            //tabControl1.SelectTab(7);
        }
        /// <summary>
        /// step 7: RBCONST.DEFとテンプレートHWDEFをコントローラに転送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void transdef_Click(object sender, EventArgs e)
        {
            SHPCmdResult rc;
            ArrayList themhw_name = new ArrayList();
            ArrayList temphw_path = new ArrayList();
            int hwd_qua = 0;
            int t = 0;
            string def_path, def_name;
            string unzip_name = "";
            st7_bacha.Visible = false;//"×"を初期化
            st7_gou.Visible = false;//"√"を初期化
            //解凍されたfirmware folderの名前を取得(step 2で既に解凍されたので、folderの存在をcheckしない)
            unzip_name = firmname.Text;//名前をtextboxから取得

            def_name = "RBCONST.DEF";
           def_path = Directory.GetCurrentDirectory() + "\\" + unzip_name + "\\HWDEF5\\RBCONST.DEF";//転送予定のRBCONST.DEFのpathを指定
            transdef.Enabled = false;//転送開始後、buttonをfreezeする。
            DirectoryInfo TheFolder = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\" + unzip_name + "\\HWDEF5");//転送予定のtemplate hwdefのfolder pathを指定
            if (!File.Exists(def_path) || TheFolder.Exists == false)
            {
                transdef.Enabled = true;
                st7_bacha.Visible = true;//"×"を表示する
                Resources.message.judge_message(72);
                return;
            }
            //2回を試す。だめならエラーを出す。
            if ((rc = sc5kcontrol.SHPDownloadFileCmd(def_path, def_name, SHPFileType.SHPObjectFile, SHPDriveType.SHPDriveFLASH)) != SHPCmdResult.SHPCmdSuccess)
            {
                if (sc5kcontrol.SHPDownloadFileCmd(def_path, def_name, SHPFileType.SHPObjectFile, SHPDriveType.SHPDriveFLASH) != SHPCmdResult.SHPCmdSuccess)
                {
                    transdef.Enabled = true;
                    st7_bacha.Visible = true;//"×"を表示する
                    Resources.message.judge_message(70);
                    return;
                }
            }
            else
            {
                //DEFの転送は成功後、テンプレートHWDEFの転送作業に入る。
                //コントローラに転送するため、解凍されたフォルダ中の"HWDEF5"フォルダの各テンプレートHWDEFの名前とpathを取得する。
                foreach (FileInfo NextFile in TheFolder.GetFiles())
                {
                    themhw_name.Add(NextFile.Name);//テンプレートHWDEFの名前を収録
                    temphw_path.Add(NextFile.FullName);//テンプレートHWDEFのpathを収録
                }
                hwd_qua = themhw_name.Count;//テンプレートhwdefの数を取得
                while(t< hwd_qua)//すべてテンプレートHWDEFをコントローラに転送(2回を試すことじゃない)
                {
                    string hw_name = themhw_name[t].ToString();
                    string hw_path = temphw_path[t].ToString();
                    if ((rc = sc5kcontrol.SHPDownloadFileCmd(hw_path, hw_name, SHPFileType.SHPObjectFile, SHPDriveType.SHPDriveFLASH)) == SHPCmdResult.SHPCmdSuccess)//どんどん転送！
                    {
                    }else
                    {
                        transdef.Enabled = true;
                        st7_bacha.Visible = true;//"×"を表示する
                        Resources.message.judge_message(71);
                        return;
                    }
                    t = t + 1;
                }
            }
            if(rc == SHPCmdResult.SHPCmdSuccess)
            {
                Resources.message.judge_message(79);
            }
            transdef.Enabled = true;
            step7_ok = 1;
            st7_gou.Visible = true;//"×"を表示する
            transfirm.Enabled = true;//step 8のbuttonを解放する。
            //tabControl1.SelectTab(8);
        }
        /// <summary>
        /// step 8: 新SSL firmwareをコントローラに転送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void transfirm_Click(object sender, EventArgs e)
        {
            SHPCmdResult rc;
            //timer3.Enabled = true;//転送監視timer3を開始する
            //firmtransstatus.Text = "Transfer Status: Transferring";
            transfirm.Enabled = false;            
            string unzip_name = "";
            string sslfolder_path;
            ArrayList task_name = new ArrayList();
            ArrayList task_path = new ArrayList();
            ushort rebootio = 949;
            byte on = 1;
            int t = 0;
            int task_qua = 0;
            //Application.DoEvents();
            unzip_name = firmname.Text;//名前をtextboxから取得
            //SSLID = "3";//debug用
            if (unzip_name != "")
            {
                sslfolder_path = Directory.GetCurrentDirectory() + "\\" + unzip_name;
            }else
            {
                transfirm.Enabled = true;
                //firmtrans_status = 2;
                Resources.message.judge_message(82);
                return;
            }
            if(SSLID == "")
            {
                transfirm.Enabled = true;
               // firmtrans_status = 2;
                Resources.message.judge_message(83);
                return;
            }
            DirectoryInfo sslfolder = new DirectoryInfo(sslfolder_path+"\\"+SSLID);
            foreach (FileInfo sslfile in sslfolder.GetFiles())
            {
                if (sslfile.Extension == ".TSK" || sslfile.Name == "PSSVER.CFG")//すべての拡張名は".tsk"のファイルのpathと名前を記録(+PSSVER.CFG)
                 {
                      task_name.Add(sslfile.Name);//task名前を収録
                      task_path.Add(sslfile.FullName);//task pathを収録
                  }
             }
            timer1.Enabled = false;//SHPReadIOを使うtimer1とSHPDownloadFileを並行処理するのは駄目らしい。ここでtimer1を無効する。
            sc5kcontrol.SHPEnableDispatchWinMsg(1);//マルチスライドを有効。
            timer3.Enabled = true;//転送監視timer3を開始する
            task_qua = task_name.Count;//task fileの数を数える。
            skinProgressBar2.Maximum = task_qua*100;//barの長さを設定
            while (t < task_qua)
             {
                 string t_name = task_name[t].ToString();
                 string t_path = task_path[t].ToString();
                 if ((rc = sc5kcontrol.SHPDownloadFileCmd(t_path, t_name, SHPFileType.SHPObjectFile, SHPDriveType.SHPDriveFLASH)) != SHPCmdResult.SHPCmdSuccess)//どんどん転送！
                 {
                     transfirm.Enabled = true;
                     //firmtrans_status = 2;
                     Resources.message.judge_message(80);
                     return;
                 }
                 t = t + 1;
                file_num = file_num + 1;
             }
            sc5kcontrol.SHPEnableDispatchWinMsg(0);//転送後マルチスライドを無効。timer1のON/OFF LINE監視は必ずmain threadを使うので、無効しないと、監視はおかしくなる。
            file_num = 8;
            //firmtrans_status = 1;//transfer成功を記録
            Thread.Sleep(500);//0.5s休憩

            //直接CPUのメモリーを操作してABS忘れを解除
            byte baseregste = 1;
            uint baseadd = 2818572288;
            ushort offadd = 0;
            byte addlen = 4;
            byte[] data = { 0};
            rc = sc5kcontrol.SHPSetBaseAddressCmd(baseregste, baseadd);// A8000000);
            if ((rc = sc5kcontrol.SHPMemoryWriteCmd(baseregste, offadd, addlen, data)) != SHPCmdResult.SHPCmdSuccess)
            {
                transfirm.Enabled = true;
                Resources.message.judge_message(81);
                return;
            } ;//(1, 0, 4, 0);
            //if ((rc = sc5kcontrol.SHPSysFuncABSRstCmd()) != SHPCmdResult.SHPCmdSuccess)
            //    {
            //    SHPResultCode code;
            //    long data1, data2;
            //    sc5kcontrol.SHPGetLastError(out code, out data1, out data2);
            //    transfirm.Enabled = true;
            //         judge_message(21);
            //       return;
            //   }
            Thread.Sleep(500);//0.5s休憩
            //再起動IO949を操作できるため、まずoperationモードに切り替える。
            if ((rc = sc5kcontrol.SHPModeSetCmd(SHPModeType.SHPModeOperation)) == SHPCmdResult.SHPCmdSuccess)
            {
                sc5kcontrol.SHPSysFuncWriteIOCmd(rebootio, on); //コントローラを再起動。
            }

            transfirm.Enabled = true;
            reboot_st8.Visible = true;
            step8_ok = 1;
            groupBox7.Enabled = false;//step 9を凍結する。
            Resources.message.log(2, "Controller is automatically rebooting during step 8.");//logに自動再起動を記録
            //Resources.message.judge_message(89);
            //tabControl1.SelectTab(9);
        }
        /// <summary>
        /// step 9: G-HWDEF作業成功の確認
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void conf_Click(object sender, EventArgs e)
        {
            SHPCmdResult rc;
            SHPResultCode rc1;
            long data1, data2;
            short gcheck = 0;
            int ret = 0;
            string all;
            string[] row, rbinfo;
            string unzip_name = firmname.Text;
            string path = Directory.GetCurrentDirectory() + "\\" + unzip_name + "\\HWDEF5\\RBCONST.DEF";//DEFのpathを取得
            string namarbinfo = "";
            StringBuilder sb_rbtype = new StringBuilder(40);
            StringBuilder sb_ssl_id = new StringBuilder(40);
            StringBuilder sb_hwid1 = new StringBuilder(40);
            StringBuilder sb_task1_rbtype2 = new StringBuilder(40);
            StringBuilder sb_task1_rbtypesub = new StringBuilder(40);
            StringBuilder sb_hwid2 = new StringBuilder(40);
            StringBuilder sb_task2_rbtype2 = new StringBuilder(40);
            byte rbtype_b = 1;
            byte ssl_id_b = 4;
            byte hwid1_b = 5;
            byte task1_rbtype2_b = 6;
            byte task1_rbtypesub_b = 8;
            byte hwid2_b = 10;
            byte task2_rbtype2_b = 11;
            if (!File.Exists(path))
            {
                Resources.message.judge_message(92);
                return;
            }
            sc5kcontrol.SHPGetLastError(out rc1,out data1, out data2);//現在controllerにエラーは発生しているかどうかをcheck
            if(data1 != 0 && data2 != 0)
            {
                Resources.message.judge_message(91);
                return;
            }
            rc = sc5kcontrol.SHPSysFuncGHWCHKCmd(out gcheck);//起動時のG-HWDEFとeepromのは一致するかをcheck
            if(rc == SHPCmdResult.SHPCmdSuccess && gcheck == 0)//問題なければ、G-HWDEF情報抽出し、以前stepで保存する情報と比較
            {
                sc5kcontrol.SHPGhwinfoCmd(rbtype_b, sb_rbtype);
                sc5kcontrol.SHPGhwinfoCmd(ssl_id_b, sb_ssl_id);
                sc5kcontrol.SHPGhwinfoCmd(hwid1_b, sb_hwid1);
                sc5kcontrol.SHPGhwinfoCmd(task1_rbtype2_b, sb_task1_rbtype2);
                sc5kcontrol.SHPGhwinfoCmd(task1_rbtypesub_b, sb_task1_rbtypesub);
                sc5kcontrol.SHPGhwinfoCmd(hwid2_b, sb_hwid2);
                sc5kcontrol.SHPGhwinfoCmd(task2_rbtype2_b, sb_task2_rbtype2);
                //後の比較と関係なく、とりあえず抽出直後にlogに記録
                Resources.message.log(0, "[STEP 9] ↓↓↓↓  Robot Information   ↓↓↓↓");
                Resources.message.log(0, " RBTYPE: "+sb_rbtype.ToString());//全装置のtype
                Resources.message.log(0, " SSLID: "+sb_ssl_id.ToString());//sslid
                Resources.message.log(0, " TASK1 RBTYPE: "+sb_task1_rbtype2.ToString());//robotのtype
                Resources.message.log(0, " TASK1 RBTYPE_SUB: "+sb_task1_rbtypesub.ToString());//robotのsub type
                Resources.message.log(0, " TASK1 HWID: "+sb_hwid1.ToString());//robotのhwid
                Resources.message.log(0, " TASK2 RBTYPE: "+sb_task2_rbtype2.ToString());//alignerのtype
                Resources.message.log(0, " TASK2 HWID: "+sb_hwid2.ToString());//alignerのhwid
                //直接RBCONST.DEFを展開し、情報を比較する。
                using (StreamReader sr = new StreamReader(path, Encoding.Default))//DEFファイルを展開し、必要な情報を検索。
                {
                    all = sr.ReadToEnd();
                    row = all.Split(new string[] { "\r\n" }, StringSplitOptions.None);//DEFすべての情報を一行一行でrow[]に渡す
                    sr.Close();
                }
                foreach (string i in row)//モータから抽出したロボット名前によりRBCONST.DEF中の対応機種情報を探す。
                {
                    if (i.Length > 2)
                    {
                        if (i.Substring(0, 11) == sb_rbtype.ToString())
                        {
                            namarbinfo = i;
                        }
                    }
                }
                if (namarbinfo == "")
                {
                    ret = 1;
                }else
                {
                    rbinfo = namarbinfo.Split(',');
                    if(rbinfo[0] != sb_rbtype.ToString() || rbinfo[2] != sb_hwid1.ToString() || rbinfo[3] != sb_task1_rbtype2.ToString() || rbinfo[4] != sb_task1_rbtypesub.ToString())
                    {
                        string rbty = sb_rbtype.ToString();
                        string hd1 = sb_hwid1.ToString();
                        string ts1_rbty2 = sb_task1_rbtype2.ToString();
                        string ts1_rbsub = sb_task1_rbtypesub.ToString();
                        ret = 1;
                    }
                    if(rbinfo[5] != "-")//defファイルでtask2存在するかどうかを判断
                    {
                        if(rbinfo[5] != sb_hwid2.ToString() || rbinfo[6] != sb_task2_rbtype2.ToString())
                        {
                            ret = 1;
                        }
                    }
                }
                if (ret != 0)//情報は一致ではなければ、エラー
                {
                    Resources.message.judge_message(90);
                    return;
                }
             }
            else//GHWDEFのcheck関数結果はNG時、エラー
            {
                Resources.message.judge_message(90);
                return;
            }

            //RBCONST.DEFにより正しい情報を確認済み後、解凍されたフォルダを削除
            // 削除ディレクトリ情報を取得
            DirectoryInfo delDir = new DirectoryInfo(Directory.GetCurrentDirectory() + "\\" + unzip_name);
            // サブディレクトリ内も含めすべてのファイルを取得する
            FileSystemInfo[] fileInfos = delDir.GetFileSystemInfos("*", SearchOption.AllDirectories);
            // ファイル属性から読み取り専用属性を外す
            foreach (FileSystemInfo fileInfo in fileInfos)
            {
                // ディレクトリまたはファイルであるかを判断する
                if ((fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    // ディレクトリの場合
                    fileInfo.Attributes = FileAttributes.Directory;
                }
                else
                {
                    // ファイルの場合
                    fileInfo.Attributes = FileAttributes.Normal;
                }
            }
            // ディレクトリを削除（サブディレクトリを含む）
            delDir.Delete(true);

            STEP10_FINISH = 1;
            Resources.message.judge_message(99);
            tabControl1.SelectTab(2);//step 2に戻り、座標を比較
        }

        /// <summary>
        /// コントローラをの接続状態を監視する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            SHPCmdResult rc;
            ushort io = 2000;
            long data = 0;
            int curstatus = 0;
            ushort rebootio = 949;
            byte on = 1;
            if ((rc = sc5kcontrol.SHPSysFuncReadIOCmd(io, out data)) == SHPCmdResult.SHPCmdSuccess)
            {
                curstatus = 1;//現在接続状態を記録
                ipstatus.BackColor = Color.Green;
                ipstatus.ForeColor = Color.White;
                ipstatus.Text = " ONLINE";
                if (step4_ok == 2)
                {
                    step4_ok = 4;
                }
            }
            else
            {
                curstatus = 2;//現在接続状態を記録
                ipstatus.BackColor = Color.Yellow;
                ipstatus.ForeColor = Color.Red;
                ipstatus.Text = "OFFLINE";
            }
            //Timerは常時に回しても、以下のcodeにより、該当操作は１回のみ
            if(curstatus != prestatus)
            {
                if(curstatus == 1)
                {
                    //ONLINEの時、すべてのbuttonを有効する。誤操作を防止
                    sppath.Enabled = true;
                    rbpos.Enabled = true;
                    sndl.Enabled = true;
                    bindl.Enabled = true;
                    gethwinfo.Enabled = true;
                    transfergh.Enabled = true;
                    wrtomt.Enabled = true;
                    if (step6_ok == 1)
                    {
                        transdef.Enabled = true;
                    }
                    if (step7_ok == 1)
                    {
                        transfirm.Enabled = true;
                    }
                    conf.Enabled = true;
                    fcheck.Enabled = true;
                    Resources.message.log(3, "Controller is ONLINE.");
                    //------------------------------------------------
                    //--------------step 4の再起動操作---------------
                    //------------------------------------------------
                    if (step4_ok == 1)//step4はOKかつOFFLINE-->ONLINEの変化ある時(=step 4で手動再起動済み)、コントを自動再起動
                    {
                        step4_ok = 2;//step 4のok flagを2にする。
                        reboot_m.Visible = false;//gif "Please reboot controller by manual."を隠す。
                        reboot_a.Visible = true;//lable "Controller is automaticlly rebooting. Please wait..."を表示する。
                        Resources.message.log(2, "Controller is automatically rebooting during step 4.");//logに自動再起動を記録
                        if ((rc = sc5kcontrol.SHPModeSetCmd(SHPModeType.SHPModeOperation)) == SHPCmdResult.SHPCmdSuccess)
                        {
                            sc5kcontrol.SHPSysFuncWriteIOCmd(rebootio, on); //コントローラを再起動。
                        }
                    }
                    if (step4_ok == 4)//自動再起動はOKならば....
                    {
                        groupBox5.Enabled = true;//step 5を解放する。
                        reboot_a.Text = "Controller has successfully completed two reboots.";//表示文字を変える。
                        step4_ok = 0;// step 4のok flagをreset
                        Resources.message.log(2, "Controller has successfully completed two reboots.");//logに2回再起動成功を記録。
                        //driveとsc5000のversion情報をlogに記録
                        SHPVersionType dr_version = SHPVersionType.SHPDriver1Version;
                        SHPVersionType sc_version = SHPVersionType.SHPMajorVersion;
                        ushort sc, dr;
                        string temp_sc, temp_dr;
                        //string sc_s = "";
                        //string dr_s = "";
                        //driver versionを取得
                        if((rc = sc5kcontrol.SHPGetVersionCmd(dr_version,out dr)) == SHPCmdResult.SHPCmdSuccess)
                        {
                            temp_dr = dr.ToString();
                            dr_s = temp_dr.Substring(0, 1) + "." + temp_dr.Substring(1, 2);//生version番号は"."が付いていないので、ここで"."を追加する。
                        }
                        //sc5000 versionを取得
                        if ((rc = sc5kcontrol.SHPGetVersionCmd(sc_version, out sc)) == SHPCmdResult.SHPCmdSuccess)
                        {
                            temp_sc = sc.ToString();
                            sc_s = temp_sc.Substring(0, 1) + "." + temp_sc.Substring(1, 2);//生version番号は"."が付いていないので、ここで"."を追加する。
                        }
                        Resources.message.judge_message(49);
                    }
                    //------------------------------------------------
                    //------------step 5の再起動後の操作------------
                    //------------------------------------------------
                    if (step5_ok == 1)
                    {
                        groupBox6.Enabled = true;//step 6を解放する。
                        reboot_st5.Text = "Controller was successfully rebooted.";
                        step5_ok = 0;//step 5のok flagをreset.
                        Resources.message.log(2, "Controller was successfully rebooted.");//logにstep 5の再起動成功を記録。                       
                        Resources.message.judge_message(59);
                    }
                    //------------------------------------------------
                    //------------step 8の再起動後の操作------------
                    //------------------------------------------------
                    if (step8_ok == 1)
                    {
                        groupBox7.Enabled = true;//step 6を解放する。
                        reboot_st8.Text = "Controller was successfully rebooted.";
                        st8_gou.Visible = true;//"√"を表示する。
                        step8_ok = 0;//step 8のok flagをreset.
                        Resources.message.log(2, "Controller was successfully rebooted.");//logにstep 8の再起動成功を記録。                       
                        Resources.message.judge_message(89);
                    }
                }
                else if (curstatus == 2)
                {
                    //OFFLINEの時、すべてのbuttonを無効化する。誤操作を防止
                    sppath.Enabled = false;
                    rbpos.Enabled = false;
                    sndl.Enabled = false;
                    bindl.Enabled = false;
                    gethwinfo.Enabled = false;
                    transfergh.Enabled = false;
                    wrtomt.Enabled = false;
                    if (step6_ok == 1)
                    {
                        transdef.Enabled = false;
                    }
                    if (step7_ok == 1)
                    {
                        transfirm.Enabled = false;
                    }
                    conf.Enabled = false;
                    fcheck.Enabled = false;
                    Resources.message.log(3, "Controller is OFFLINE.");
                }
            }
            prestatus = curstatus;//現在接続状態を記録変数に渡す。
        }
        /// <summary>
        /// Binファイルの転送とupdate状況を監視する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Tick(object sender, EventArgs e)
        {
            switch (bintrans_status)
            {
                case 0:
                    transferstatus.BackColor = Color.DarkBlue;
                    transferstatus.ForeColor = Color.White;
                    transferstatus.Text = "Process Status:        Transferring";
                    break;
                case 1:
                    transferstatus.BackColor = Color.Green;
                    transferstatus.ForeColor = Color.White;
                    transferstatus.Text = "Process Status:        Success";
                    timer2.Enabled = false;//転送成功後timer2を閉める。無駄な処理を防ぐ。
                    break;
                case 2:
                    transferstatus.BackColor = Color.Red;
                    transferstatus.ForeColor = Color.Yellow;
                    transferstatus.Text = "Process Status:        Failure";
                    timer2.Enabled = false;//転送失敗後timer2を閉める。無駄な処理を防ぐ。
                    break;
                case 10:
                    transferstatus.BackColor = Color.DarkBlue;
                    transferstatus.ForeColor = Color.Yellow;
                    if (driver_no == 6)
                    {
                        transferstatus.Text = "Process Status: Driver 1/6 updating";
                    }else
                    {
                        transferstatus.Text = "Process Status: Driver 1/8 updating";
                    }
                    break;
                case 11:
                    if (driver_no == 6)
                    {
                        transferstatus.Text = "Process Status: Driver 2/6 updating";
                    }
                    else
                    {
                        transferstatus.Text = "Process Status: Driver 2/8 updating";
                    }
                    break;
                case 12:
                    if (driver_no == 6)
                    {
                        transferstatus.Text = "Process Status: Driver 3/6 updating";
                    }
                    else
                    {
                        transferstatus.Text = "Process Status: Driver 3/8 updating";
                    }
                    break;
                case 13:
                    if (driver_no == 6)
                    {
                        transferstatus.Text = "Process Status: Driver 4/6 updating";
                    }
                    else
                    {
                        transferstatus.Text = "Process Status: Driver 4/8 updating";
                    }
                    break;
                case 14:
                    if (driver_no == 6)
                    {
                        transferstatus.Text = "Process Status: Driver 5/6 updating";
                    }
                    else
                    {
                        transferstatus.Text = "Process Status: Driver 5/8 updating";
                    }
                    break;
                case 15:
                    if (driver_no == 6)
                    {
                        transferstatus.Text = "Process Status: Driver 6/6 updating";
                    }
                    else
                    {
                        transferstatus.Text = "Process Status: Driver 6/8 updating";
                    }
                    break;
                case 16:
                    transferstatus.Text = "Process Status: Driver 7/8 updating";
                    break;
                case 17:
                    transferstatus.Text = "Process Status: Driver 8/8 updating";
                    break;
            }
        }
        /// <summary>
        /// Firmwareの転送状態を監視する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer3_Tick(object sender, EventArgs e)
        {
            ushort progress;
            ushort basenum;
            sc5kcontrol.GetFileTransferProgress(out progress);
            switch (file_num)
            {
                case 0:
                    basenum = 0;
                    break;
                case 1:
                    basenum = 100;
                    break;
                case 2:
                    basenum = 200;
                    break;
                case 3:
                    basenum = 300;
                    break;
                case 4:
                    basenum = 400;
                    break;
                case 5:
                    basenum = 500;
                    break;
                default:
                    basenum = 0;
                    break;
            }
            skinProgressBar2.Value = basenum + progress;
            if(file_num == 8)
            {
                skinProgressBar2.Value = 0;
                timer3.Enabled = false;
                timer1.Enabled = true;
            }
            //switch (firmtrans_status)
            //{
            //    case 1:
            //        firmtransstatus.ForeColor = Color.White;
            //        firmtransstatus.Text = "Transfer Status:    Success";
            //        timer3.Enabled = false;//転送成功後timer3を閉める。無駄な処理を防ぐ。
            //        break;
            //    case 2:
            //        firmtransstatus.ForeColor = Color.Red;
            //        firmtransstatus.Text = "Transfer Status:    Failure";
            //        timer3.Enabled = false;//転送失敗後timer3を閉める。無駄な処理を防ぐ。
            //        break;
            //}
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
        /// 現在の窓を閉じる時に、確認窓を出す。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to quit?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.OK)
            {
                e.Cancel = false;  //okを押すと、Form1をclose
            }
            else
            {
                e.Cancel = true;
            }
        }
     /// <summary>
     /// Form1をclose後、全appをshutdown
     /// </summary>
     /// <param name="sender"></param>
     /// <param name="e"></param>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Resources.message.log(4, "Scene [Append G-HWDEF To All Parts] has ended.");
            Resources.message.log(4, "G-H Transplantation has ended.");
            Process.GetCurrentProcess().Kill();//appすべてprocessを殺す。しないと、「appは停止しました」というエラーがでる。
            Application.Exit();//processを殺したら、全appをshutdown.
        }


        /// <summary>
        /// step 0のbackup進行状況を監視する。
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
            SetProgressBarValue(skinProgressBar1, 100 - percentLeft);
            if (rc == (int)RobotFWDownload.TransferStatus.TransferStatus_Success/*成功*/)
            {
                timer4.Enabled = false;
                string lastUploadPath;
                RobotFWDownload.SRFirmUpDownGetLastUploadFilePath(out lastUploadPath);
                SetProgressBarValue(skinProgressBar1, 0);
                fcheck.Enabled = true;
                //成功失敗に関係なく、とりあえずコントローラと再接続。
                if ((rt = shpcontrol.Init("SOC:" + conip.Text, 0)) == SHPCmdResult.SHPCmdSuccess)
                {
                    if ((rt = m_ShpHostPrtclNode.Init(ref shpcontrol, 0)) == SHPCmdResult.SHPCmdSuccess)
                    {
                        rt = sc5kcontrol.Init(ref m_ShpHostPrtclNode, 8, 0);
                        sc5kcontrol.SHPEnableDispatchWinMsg(0);
                    }
                }
                timer1.Enabled = true;//timer1の監視を有効する。
                Resources.message.judge_message(9);
                tabControl1.SelectTab(1);
            }
            else if (rc == (int)RobotFWDownload.TransferStatus.TransferStatus_Failed/*失敗*/)
            {
                timer4.Enabled = false;
                StringBuilder sb = new StringBuilder();
                SetProgressBarValue(skinProgressBar1, 0);
                //成功失敗に関係なく、とりあえずコントローラと再接続。
                if ((rt = shpcontrol.Init("SOC:" + conip.Text, 0)) == SHPCmdResult.SHPCmdSuccess)
                {
                    if ((rt = m_ShpHostPrtclNode.Init(ref shpcontrol, 0)) == SHPCmdResult.SHPCmdSuccess)
                    {
                        rt = sc5kcontrol.Init(ref m_ShpHostPrtclNode, 8, 0);
                        sc5kcontrol.SHPEnableDispatchWinMsg(0);
                    }
                }
                fcheck.Enabled = true;
                Resources.message.judge_message(0);
                timer1.Enabled = true;//timer1の監視を有効する。
                return;
            }
        }
    }
}
