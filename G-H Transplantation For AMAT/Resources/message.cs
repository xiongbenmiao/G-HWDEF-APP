using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace G_H_Transplantation_For_AMAT.Resources
{
    /// <summary>
    /// このmethod: 各種messageを表示する。またlogを残す
    /// </summary>
    class message
    {
        public static int sn_trans;//step 3のSNファイルを転送するかどうかのflag
        public static void log(int judge, string message)
        {
            string ghlog;
            string[] ghlogcontent = { "" };
            DateTime dt = DateTime.Now;
            ghlog = Directory.GetCurrentDirectory() + "\\G-H Transplantation_Log.txt";
            switch (judge)
            {
                case 0:
                    ghlogcontent[0] = dt.ToString() + " ------ Information Record>>" + message;
                    break;
                case 1://step or connection失敗
                    ghlogcontent[0] = dt.ToString() + " ------ Failure: " + message;
                    break;
                case 2://step成功
                    ghlogcontent[0] = dt.ToString() + " ------ Success: " + message;
                    break;
                case 3://コントローラ一時的に切断 or 再接続OK
                    ghlogcontent[0] = dt.ToString() + " ------ " + message;
                    break;
                case 4:
                    ghlogcontent[0] = "*************** " + message + " ***************";
                    break;
            }
            if (!File.Exists(ghlog))//logファイルは存在じゃなければ、新規作成
            {
                File.WriteAllLines(ghlog, ghlogcontent, Encoding.Default);
            }
            else
            {
                File.AppendAllLines(ghlog, ghlogcontent, Encoding.Default);
            }
        }
        /// <summary>
        /// パラメータに転送されたjudge codeによりerror or OK messageを表示する
        /// </summary> 
        public static void judge_message(int judgecode)
        {
            switch (judgecode)
            {
                //Append G-HWDEF To All Partsのmessage
                //step 0
                case 0:
                    log(1, "STEP 0 => Back up controller failed. Make sure the controller is ONLINE and try this step again.");
                    MessageBox.Show("Back up controller failed. Make sure the controller is ONLINE and try this step again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 0
                    break;
                case 1:
                    log(1, "STEP 0 => The specified firmware package is invalid.");
                    MessageBox.Show("The specified firmware package is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 0
                    break;
                case 2:
                    log(1, "STEP 0 => Missing necessary \".bin\" or \".dll\" file. Read tool manual and check tool folder.");
                    MessageBox.Show(" Missing necessary \".bin\" or \".dll\" file. Read tool manual and check tool folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 0
                    break;
                case 3:
                    log(1, "STEP 0 => Firmware package was not specified.");
                    MessageBox.Show("Firmware package was not specified.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 0
                    break;
                case 4:
                    log(0, "[STEP 0] Specified Firmware: "+ Form1.form1.firmname.Text);//firmware名前を記録
                    DialogResult jr = MessageBox.Show("Press \"OK\" to skip STEP 0 (Without files check and backup). \"Cancel\" to continue STEP 0.", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);//message for step 0
                    if (jr == DialogResult.OK)
                    {
                        log(2, "Skipped STEP 0.");
                        Form1.form1.tabControl1.SelectTab(1);//step 1に移動
                    }
                    else
                    {
                    }
                    break;
                case 9:
                    log(2, "STEP 0");
                    MessageBox.Show("All files all ready. Controller was successfully backed up. Start to work! Move to STEP 1.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);//message for step 0
                    break;
                //step 1
                case 10:
                    log(1, "STEP 1 => Get old HWDEF5.CFG information failed. Make sure the controller is ONLINE and try this step again.");
                    MessageBox.Show("Get old HWDEF5.CFG information failed. Make sure the controller is ONLINE and try this step again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 1
                    break;
                case 19:
                    //旧HWDEF情報はlogに記録
                    log(0, "[STEP 1] ↓↓↓↓  Manipulator Information   ↓↓↓↓");
                    log(0, " " + Form1.form1.r_rbtp);
                    log(0, " " + Form1.form1.r_linlinlin);
                    log(0, " " + Form1.form1.r_linyiwu);
                    log(0, " " + Form1.form1.r_yilinsan);
                    log(0, " " + Form1.form1.r_yilinsi);
                    log(0, " " + Form1.form1.r_yilinwu);
                    log(0, "[STEP 1] ↓↓↓↓   Aligner Information   ↓↓↓↓");
                    if (Form1.form1.aligner_flag == 1)
                    {
                        log(0, " " + Form1.form1.a_linlinlin);
                        log(0, " " + Form1.form1.a_linyiwu);
                        log(0, " " + Form1.form1.a_yilinsan);
                        log(0, " " + Form1.form1.a_yilinsi);
                        log(0, " " + Form1.form1.a_yilinwu);
                    }else
                    {
                        log(0, " This robot does not have aligner.");
                    }
                    log(2, "STEP 1");
                    MessageBox.Show("All informations were successfully got. Move to step 2.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);//message for step 1
                    break;
                //step 2
                case 20:
                    log(0, "[ALL STEP] Robot Current Position: X = " + Form1.form1.xa.Text + "  Y = " + Form1.form1.ya.Text + "  Z= " + Form1.form1.za.Text + "  S1= " + Form1.form1.s1a.Text + "  S2 =" + Form1.form1.s2a.Text + "  Aligner =" + Form1.form1.ala.Text);//移植後robot現在位置を記録
                    log(2, "ALL STEP.");
                    MessageBox.Show("All G-HWDEF transplantation steps are finished!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);//message for step 1
                    break;
                case 21:
                    log(1, "STEP 2 => Get current position failed. Make sure the controller is ONLINE and try this step again.");
                    MessageBox.Show("Get current position failed. Make sure the controller is ONLINE and try this step again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 2
                    break;
                case 29:
                    log(0, "[STEP 2] Robot Current Position: X = " + Form1.form1.xb.Text+"  Y = "+ Form1.form1.yb.Text + "  Z= " + Form1.form1.zb.Text + "  S1= " + Form1.form1.s1b.Text + "  S2 =" + Form1.form1.s2b.Text + "  Aligner =" + Form1.form1.alb.Text);//robot現在位置を記録
                    log(2, "STEP 2");
                    log(0, "[STEP 3] Current Controller Type: " + Form1.form1.cur_ct.Text); //step 2の現在のコントローラ タイプをlogに記録
                    log(0, "[STEP 3] Current Controller Serial Number: " + Form1.form1.cur_sn.Text);//step 2の現在のコントローラシリアル番号をlogに記録
                    MessageBox.Show("Current position was successfully got. Move to STEP 3.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);//message for step 2
                    break;
                //step 3
                case 30:
                    log(1, "STEP 3 => Transfer SC5K_SN.CFG failed. Make sure the controller is ONLINE and try this step again.");
                    MessageBox.Show("Transfer SC5K_SN.CFG failed. Make sure the controller is ONLINE and try this step again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 3
                    break;
                case 31:
                    log(1, "STEP 3 => Invalid input. Check the total number of input digits.");
                    MessageBox.Show("Invalid input. Check the total number of input digits.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 3
                    break;
                case 32:
                    log(1, "STEP 3 => Controller type does not exists.");
                    MessageBox.Show("Controller type does not exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 3
                    break;
                case 33:
                    log(1, "STEP 3 => Firmware package was not specified. Back to Step 0 to specify it.");
                    MessageBox.Show("Firmware package was not specified. Back to Step 0 to specify it.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 3
                    break;
                case 34:
                    sn_trans = 0;
                    DialogResult result = MessageBox.Show("Your input are different from Current Content. Press \"OK\" to continue transfer. Press \"Cancel\" to interrupt it.", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);//message for step 3
                    if (result == DialogResult.OK)//提示windowのOKを押すと、転送する。
                    {
                        sn_trans = 1;
                        log(0, "[STEP 3] Input is different from current, user selected transfer."); //
                    }
                    else
                    {
                        log(0, "[STEP 3] Input is different from current, user selected interrupt transfer."); //
                    }
                        break;
                case 39:
                    log(0, "[STEP 3] Input Controller Type: " + Form1.form1.controllertype.Text); //step 2のインプットのコントローラ タイプをlogに記録
                    log(0, "[STEP 3] Input Controller Serial Number: " + Form1.form1.snno.Text);//step 2のインプットのコントローラシリアル番号をlogに記録
                    log(2, "STEP 3");
                    MessageBox.Show("The S/N file was successfully transfered. Move to step 4.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);//message for step 3
                    break;
                    //step 4
                case 40:
                    log(1, "STEP 4 => Transfer sc5000.bin failed. Make sure the controller is ONLINE and try this step again.");
                    MessageBox.Show("Transfer sc5000.bin failed. Make sure the controller is ONLINE and try this step again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 4
                    break;
                case 41:
                    log(1, "STEP 4 => The controller type you input in Step 3 is for SR8241, but robot type you selected is SR7173. Check them once more!");
                    MessageBox.Show("The controller type you input in Step 3 is for SR8241, but robot type you selected is SR7173. Check them once more!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 4
                    break;
                case 42:
                    log(1, "STEP 4 => The controller type you input in Step 3 is for SR7173, but robot type you selected is SR8241. Check them once more!");
                    MessageBox.Show("The controller type you input in Step 3 is for SR7173, but robot type you selected is SR8241. Check them once more!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 4
                    break;
                case 43:
                    log(1, "STEP 4 => sc5000.bin was successfully transferred, but failed to transfer 5KSTDRV.bin. Make sure the controller is ONLINE and try this step again.");
                    MessageBox.Show("sc5000.bin was successfully transferred, but failed to transfer 5KSTDRV.bin. Make sure the controller is ONLINE and try this step again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 4
                    break;
                case 44:
                    log(1, "STEP 4 => Update motor driver firmware failed by some unkown reason. Press button to try again.");
                    MessageBox.Show("Update motor driver firmware failed by some unkown reason. Press button to try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 4
                    break;
                case 45:
                    log(1, "STEP 4 => Could not update motor driver because switch to write mode failed. Try this step again.");
                    MessageBox.Show("Could not update motor driver because switch to write mode failed. Try this step again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 4
                    break;
                case 49:
                    log(0, "[STEP 4] New SC5000 Version: " + Form1.form1.sc_s); //step 4で更新後SC5000 versionをlogに記録
                    log(0, "[STEP 4] New Motor Driver Version: " + Form1.form1.dr_s); //step 4で更新後driver versionをlogに記録
                    log(2, "STEP 4");
                    DialogResult dr = MessageBox.Show("The bin files were successfully transferred and updated. Confirm that controller was rebooted twice (Manual + Automatic). After controller back to ONLINE status, press OK to move to step 5.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);//message for step 4
                    if (dr == DialogResult.OK)
                    {
                        Form1.form1.tabControl1.SelectTab(5);//step 5に移動
                    }
                    else
                    {
                    }
                        break;
                    //step 5
                case 50:
                    log(1, "STEP 5 => Could not find RBCONST.DEF. Make sure the firmware package was specified in STEP 0 and unzipped.");
                    MessageBox.Show("Could not find RBCONST.DEF. Make sure the firmware package was specified in STEP 0 and unzipped.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 5
                    break;
                case 51:
                    log(1, "STEP 5 => Could not find corresponding robot in RBCONST.DEF. Please contact Sankyo engineer.");
                    MessageBox.Show("Could not find corresponding robot in RBCONST.DEF. Please contact Sankyo engineer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 5
                    break;
                case 52:
                    log(1, "STEP 5 => Could not find template G-HWDEF. Make sure the firmware package was specified in STEP 0 and unzipped.");
                    MessageBox.Show("Could not find template G-HWDEF. Make sure the firmware package was specified in STEP 0 and unzipped.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 5
                    break;
                case 53:
                    log(1, "STEP 5 =>Transfer G-HWDEF failed. Make sure the controller is ONLINE and try this step again.");
                    MessageBox.Show("Transfer G-HWDEF failed. Make sure the controller is ONLINE and try this step again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 5
                    break;
                case 54:
                    log(1, "STEP 5 => Creat G-HWDEF failed beacuse tool could not get information from STEP 1. Try STEP 1 and 5 again.");
                    MessageBox.Show("Creat G-HWDEF failed beacuse tool could not get information from STEP 1. Try STEP 1 and 5 again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 5
                    break;
                case 55:
                    log(1, "STEP 5 => Reboot controller failed. Try STEP 5 again.");
                    MessageBox.Show("Reboot controller failed. Try STEP 5 again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 5
                    break;
                case 59:
                    log(2, "STEP 5");
                    DialogResult pr = MessageBox.Show("The G-HWDEF was  transferred and controller was rebooted successfully, press OK to move to STEP 6.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);//message for step 5
                    if (pr == DialogResult.OK)
                    {
                        Form1.form1.tabControl1.SelectTab(6);//step 6に移動
                    }
                    else
                    {
                    }
                    break;
                    //step 6
                case 60:
                    log(1, "STEP 6 =>Writing information to the motors failed.The controller may not has G-HWDEF. Try STEP 5 and 6 again.");
                    MessageBox.Show("Writing information to the motors failed. The controller may not has G-HWDEF. Try STEP 5 and 6 again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 6
                    break;
                case 69:
                    log(2, "STEP 6");
                    MessageBox.Show("Informations were successfully written into the motors encoder. Continue to STEP 7.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);//message for step 6
                    break;
                //step 7
                case 70:
                    log(1, "STEP 7 =>Transfer RBCONST.DEF failed. Make sure the controller is ONLINE and try this step again.");
                    MessageBox.Show("Transfer RBCONST.DEF failed. Make sure the controller is ONLINE and try this step again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 7
                    break;
                case 71:
                    log(1, "STEP 7 =>Transfer template  HWDEF failed. Make sure the controller is ONLINE and try this step again.");
                    MessageBox.Show("Transfer template  HWDEF failed. Make sure the controller is ONLINE and try this step again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 7
                    break;
                case 72:
                    log(1, "STEP 7 => Could not find RBCONST.DEF or template HWDEF. Make sure the firmware package was specified in STEP 0 and unzipped. ");
                    MessageBox.Show("Could not find RBCONST.DEF or template HWDEF. Make sure the firmware package was specified in STEP 0 and unzipped. ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 7
                    break;
                case 79:
                    log(2, "STEP 7");
                    MessageBox.Show("All files were successfully transfered to controller. Continue to STEP 8.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);//message for step 7
                    break;
                    //step 8
                case 80:
                    log(1, "STEP 8 =>Transfer firmware package failed. Make sure the controller is ONLINE and try this step again.");
                    MessageBox.Show("Transfer firmware package failed. Make sure the controller is ONLINE and try this step again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 8
                    break;
                case 81:
                    log(1, "STEP 8 =>Release ABS lost failed.");
                    MessageBox.Show("Release ABS lost failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 8
                    break;
                case 82:
                    log(1, "STEP 8 => Could not find firmware package. Make sure it was specified in STEP 0 and unzipped.");
                    MessageBox.Show("Could not find firmware package. Make sure it was specified in STEP 0 and unzipped.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 8
                    break;
                case 83:
                    log(1, "STEP 8 => Tool lost SSLID because you have shut down the tool before. Use BACKUP to recorvery robot and do the operation from STEP 0.");
                    MessageBox.Show("Tool lost SSLID because you have shut down the tool before. Use BACKUP to recorvery robot and do the operation from STEP 0.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 8
                    break;
                case 89:
                    log(2, "STEP 8");
                    DialogResult sr = MessageBox.Show("Controller was successfully rebooted. Move to STEP 9.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);//message for step 8
                    if (sr == DialogResult.OK)
                    {
                        Form1.form1.tabControl1.SelectTab(7);//step 9に移動
                    }
                    else
                    {
                    }
                    break;
                    //step 9
                case 90:
                    log(1, "STEP 9 => The robot information read from motor encoder are different from STEP 5 and 6. Confirm G-HWDEF failed.");
                    MessageBox.Show("The robot information read from motor encoder are different from STEP 5 and 6. Confirm G-HWDEF failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 9
                    break;
                case 91:
                    log(1, "STEP 9 => Controller is reporting error. Confirm G-HWDEF failed.");
                    MessageBox.Show("Controller is reporting error. Confirm G-HWDEF failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 9
                    break;
                case 92:
                    log(1, "STEP 9 => Tool needs RBCONST.DEF to confirm G-HWDEF but could not find it. Make sure the firmware package was specified in STEP 0 and unzipped.");
                    MessageBox.Show("Tool needs RBCONST.DEF to confirm G-HWDEF but could not find it. Make sure the firmware package was specified in STEP 0 and unzipped.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 9
                    break;
                case 99:
                    log(2, "STEP 9");
                    MessageBox.Show("Confirm G-HWDEF successfully. Press OK to back to step 2 to confirm robot position ", "", MessageBoxButtons.OK, MessageBoxIcon.Information);//message for step 0
                    break;

                //New Combinationのmessage
                case 200:
                    log(1, "STEP 0 =>Firmware package was not selected.");
                    MessageBox.Show("Firmware package was not selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 0
                    break;
                case 201:
                    log(1, "STEP 0 =>Execute operation \"New\" failed.");
                    MessageBox.Show("Execute operation \"New\" failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);//message for step 0
                    break;
                case 202:
                    log(2, "STEP 0");
                    MessageBox.Show("Execute operation \"New\" successfully.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);//message for step 0
                    break;
            }
        }
    }
}
