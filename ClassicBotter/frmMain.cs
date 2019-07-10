using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using ClassicBotter.Objects;
using System.Media;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;

namespace ClassicBotter
{
    public partial class frmMain : Form
    {
        Player player;

        Thread threadScript;
        Thread threadMain;
        Thread threadCavebot;

        SoundPlayer soundAlert;
        public static Hashtable ht;
        List<TextBox> hotkeyKeys;
        List<TextBox> hotkeyActions;
        List<uint> foods;
        //List<uint> loot;

        int spyFloor = 0;

        bool scriptWorking;
 //       bool GMOnline;   // REMOVIDO

        public frmMain()
        {
            if (!Memory.GetHandle())
                Application.Exit();

            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();

            InitKeyboard();
            InitMouse();

            Addresses.Version.SetAddresses();

            soundAlert = new SoundPlayer(Application.StartupPath + "/alert.wav");

            ht = new Hashtable();

            hotkeyKeys = new List<TextBox>();
            hotkeyActions = new List<TextBox>();
            InitHotkeys();

            LoadCavebotScripts();
            LoadScripts();
            LoadTags();

            foods = new List<uint>();
            foods.Add(2671);
            foods.Add(2666);
            foods.Add(3725);
            /*
            loot = new List<uint>();
            loot.Add(2148);
            loot.Add(2159);
            loot.Add(2150);
            loot.Add(2149);
            loot.Add(2398);*/

            var lib = new AutoCompleteStringCollection();
            lib.Add("$lclick");
            lib.Add("$rclick");
            lib.Add("$drag");
            lib.Add("$dragsmooth");
            lib.Add("$wait");
            lib.Add("$key");
            lib.Add("$gotoline");
            lib.Add("$gotolabel");
            lib.Add("$pausecavebot");
            lib.Add("$resumecavebot");
            lib.Add("$pausescript");
            lib.Add("$resumescript");
            lib.Add("$playsound");
            txtCavebotAction.AutoCompleteCustomSource = lib;
            txtCavebotAction.AutoCompleteMode = AutoCompleteMode.Suggest;
            txtCavebotAction.AutoCompleteSource = AutoCompleteSource.CustomSource;

            txtAction.AutoCompleteCustomSource = lib;
            txtAction.AutoCompleteMode = AutoCompleteMode.Suggest;
            txtAction.AutoCompleteSource = AutoCompleteSource.CustomSource;

            var libCE = new AutoCompleteStringCollection();
            libCE.Add("$hp");
            libCE.Add("$mp");
            libCE.Add("$cap");
            libCE.Add("$ring");
            libCE.Add("$level");
            libCE.Add("$mlevel");
            libCE.Add("$exp");
            libCE.Add("$manashield");
            libCE.Add("$haste");
            libCE.Add("$paralyze");
            libCE.Add("$neck");
            libCE.Add("$rhand");
            libCE.Add("$lhand");
            libCE.Add("$ring");
            libCE.Add("$ammo");
            libCE.Add("$posx");
            libCE.Add("$posy");
            libCE.Add("$target.hppc");
            libCE.Add("$target.posx");
            libCE.Add("$target.posy");
            libCE.Add("$target.posz");
            libCE.Add("$mppc");

            txtThing1.AutoCompleteCustomSource = libCE;
            txtThing1.AutoCompleteMode = AutoCompleteMode.Suggest;
            txtThing1.AutoCompleteSource = AutoCompleteSource.CustomSource;
            
        }

        private void LoadCavebotScripts()
        {
            lstCavebotScripts.Items.Clear();
            DirectoryInfo dInfo = new DirectoryInfo(Application.StartupPath + "/waypoints/");
            FileInfo[] files = dInfo.GetFiles("*.txt");
            foreach (FileInfo f in files)
            {
                string str = f.Name.Substring(0, f.Name.Length-4);
                lstCavebotScripts.Items.Add(str);
            }
        }

        private void LoadScripts()
        {
            lstScripts.Items.Clear();
            DirectoryInfo dInfo = new DirectoryInfo(Application.StartupPath + "/scripts/");
            FileInfo[] files = dInfo.GetFiles("*.txt");
            foreach (FileInfo f in files)
            {
                string str = f.Name.Substring(0, f.Name.Length - 4);
                lstScripts.Items.Add(str);
            }
        }

        private void InitKeyboard()
        {
            KeyboardHook.Enable();
            KeyboardHook.Add(Keys.Escape, StopAll);
            KeyboardHook.Add(Keys.Multiply, Recorder);
            KeyboardHook.Add(Keys.Add, SpyUp);
            KeyboardHook.Add(Keys.Subtract, SpyDown);
        }

        private void InitTags()
        {
            WinApi.RECT rect = new WinApi.RECT();
            WinApi.GetWindowRect(Memory.process.MainWindowHandle, out rect);
            Rectangle gameview = Client.GameView();
            gameview.Y += rect.top + 35;
            gameview.X += rect.left + 10;
            Point p1 = new Point(gameview.X, gameview.Y);
            Point p2 = new Point(gameview.X + gameview.Width, gameview.Y + gameview.Height);
            if(ht.ContainsKey("#topleft")) ht.Remove("#topleft");
            if (ht.ContainsKey("#bottomright")) ht.Remove("#bottomright");
            if (ht.ContainsKey("#self")) ht.Remove("#self");

            ht.Add("#topleft", p1);
            ht.Add("#bottomright", p2);
            ht.Add("#self", new Point(gameview.Width/2 + gameview.X,gameview.Height/2 + gameview.Y));

        }
        private bool SpyUp()
        {
            Client.StatusbarMessage = "Spying up";
            LevelSpy(++spyFloor);
            //if (spyFloor >= 6) spyFloor = 6;

            return false;
        }

        private bool SpyDown()
        {
            Client.StatusbarMessage = "Spying down";
            LevelSpy(--spyFloor);
            //if (spyFloor <= -6) spyFloor = -6;

            return false;
        }

        private bool LevelSpy(int floor)
        {
            int playerZ;
            int tempPtr;

            if (spyFloor == 0)
            {
                Memory.WriteBytes(Addresses.Client.LevelSpy1, Addresses.Client.LevelSpyDefault, 6);
                Memory.WriteBytes(Addresses.Client.LevelSpy2, Addresses.Client.LevelSpyDefault, 6);
                Memory.WriteBytes(Addresses.Client.LevelSpy3, Addresses.Client.LevelSpyDefault, 6);
                Client.StatusbarMessage = "Groundfloor";
                return false;
            }

            Memory.WriteBytes(Addresses.Client.LevelSpy1, Addresses.Client.Nops, 6);
            Memory.WriteBytes(Addresses.Client.LevelSpy2, Addresses.Client.Nops, 6);
            Memory.WriteBytes(Addresses.Client.LevelSpy3, Addresses.Client.Nops, 6);

            tempPtr = Memory.ReadInt(Addresses.Client.LevelSpyPtr);
            tempPtr += 0x1C;// Addresses.Client.LevelSpyAdd1;
            tempPtr = Memory.ReadInt(tempPtr);
            tempPtr += 0x25D8; //(int)Addresses.Client.LevelSpyAdd2;
            
            playerZ = (int)player.Z;

            if (playerZ <= 7)
            {
                if (playerZ - floor >= 0 && playerZ - floor <= 7)
                {
                    playerZ = 7 - playerZ;
                    Memory.WriteInt(tempPtr, playerZ + floor);
                    Debug.WriteLine(playerZ + floor);
                    return true;
                }
            }
            else
            {
                if (floor >= -2 && floor <= 2 && playerZ - floor < 16)
                {
                    Memory.WriteInt(tempPtr, 2 + floor);
                    return true;
                }
            }

            return false;
        }

        bool mLeft;
        bool mRight;
        private void InitMouse()
        {
            MouseHook.Enable();
            MouseHook.ButtonUp += new MouseHook.MouseButtonHandler(delegate(MouseButtons btn)
            {
                if (mLeft && mRight) Client.StatusbarMessage = player.LastRightClickId.ToString();
                if (btn == MouseButtons.Left)
                    mLeft = false;
                if (btn == MouseButtons.Right)
                    mRight = false;
                return true;
            });
            MouseHook.ButtonDown += new MouseHook.MouseButtonHandler(delegate(MouseButtons btn)
            {
                if (WinApi.GetForegroundWindow() == Memory.process.MainWindowHandle)
                {
                    if (btn == MouseButtons.Right)
                    {
                        mRight = true;
                    }
                    if (btn == MouseButtons.Left)
                    {
                        mLeft = true;
                    }
                }
                return true;
            });
        }

        private void InitHotkeys()
        {
            int count = 0;
            for (int i = 0; i < 10; i++)
            {
                CheckBox chkHotkey = new CheckBox();
                chkHotkey.Name = "chkHotkey" + count.ToString();
                chkHotkey.CheckedChanged += new EventHandler(chkHotkey_CheckedChanged);

                TextBox txtHotkeyKey = new TextBox();
                txtHotkeyKey.Name = "txtHotkeyKey" + count.ToString();
                txtHotkeyKey.ReadOnly = true;
                txtHotkeyKey.KeyDown += new KeyEventHandler(txtHotkeyKey_KeyDown);

                TextBox txtHotkeyAction = new TextBox();
                txtHotkeyAction.Name = "txtHotkeyAction" + count.ToString();
                txtHotkeyAction.Text = "";
                txtHotkeyAction.Width = 500;

                tableLayoutPanel1.Controls.Add(chkHotkey);
                tableLayoutPanel1.Controls.Add(txtHotkeyKey);
                tableLayoutPanel1.Controls.Add(txtHotkeyAction);
                hotkeyKeys.Add(txtHotkeyKey);
                hotkeyActions.Add(txtHotkeyAction);
                count++;
                tableLayoutPanel1.RowCount += 1;
            }
        }

        void chkHotkey_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox) sender;
            int num = Int32.Parse(cb.Name.Substring(cb.Name.Length - 1, 1));
            byte val = Byte.Parse(hotkeyKeys[num].Text);
            Keys k = (Keys)val;
            if (cb.Checked)
            {
                string line = hotkeyActions[num].Text;
                KeyboardHook.Add(k, new KeyboardHook.KeyPressed(delegate()
                {
                    EvaluateScript(line);
                    /*
                    if (line.Contains('<') || line.Contains('>') || line.Contains('=') || line.Contains("<>"))
                        TranslateToCE(line);
                    else
                        TranslateToCommand(line);
                     */
                        return false;
                    }));
            }
            else
            {
                KeyboardHook.Remove(k);
            }
        }

        void txtHotkeyKey_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            Debug.WriteLine(tb.Name);
            tb.Text = e.KeyValue.ToString();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            LoadPlayer();

            threadScript = new Thread(Script);
            threadScript.Start();
            threadMain = new Thread(Main);
            threadMain.Start();
            threadCavebot = new Thread(Cavebot);
            threadCavebot.Start();
           // txtGmLogin.Text = GMLastLogin();
        }

        private bool Recorder()
        {
            if(chkRecord.Checked)
            {
                Form f = new Forms.frmRecorder();
                f.Show();
                return false;
            }
            return true;
        }

        public void RefreshTags()
        {
            InitTags();
            lstTags.Items.Clear();
            foreach (string k in ht.Keys)
            {
                Point tmp;
                tmp = (Point)ht[k.ToString()];
                lstTags.Items.Add(k + " " + tmp.X + " " + tmp.Y);
            }
        }

        private void SaveTags()
        {
            TextWriter tw = new StreamWriter("tags.txt");
            for (int i = 0; i < lstTags.Items.Count; i++)
            {
                tw.WriteLine(lstTags.Items[i].ToString());
            }
            tw.Close();
        }

        private void LoadTags()
        {
            ht.Clear();
            //load tags from txt to hashmap
            string line;
            TextReader tr = new StreamReader("tags.txt");
            while ((line = tr.ReadLine()) != null)
            {
                string[] tagData = line.Split(' ');
                object tmpPoint = new Point(Int32.Parse(tagData[1]),Int32.Parse(tagData[2]));
                ht.Add(tagData[0], tmpPoint);
            }
            tr.Close();
            RefreshTags();
        }

        private void Main()
        {
            while (threadMain.IsAlive)
            {
                if (player != null)
                {
                    if (chkHeal.Checked)
                    {
                        int health = Int32.Parse(txtHealth.Text);
                        int mana = Int32.Parse(txtMana.Text);
                        string healhk = healHK.Text;
                        if (player.Health <= health && player.Mana >= mana) Utils.SendKeys(healhk);
                    }
                    if (Client.Connection == 8)
                    {
                        /*
                         * ALARMS
                         */

                        //GM
                        if (GamemasterOnScreen())
                        {
                            if (chkGMSound.Checked)
                                soundAlert.PlaySync();
                                Thread.Sleep(100);
                            if (chkGMFlash.Checked)
                                WinApi.FlashWindow(Memory.process.MainWindowHandle, false);
                            if (chkGMPause.Checked)
                                PauseAll();
                            if (chkGMLogout.Checked)
                                SendKeys.SendWait("^l");
                        }

                        if (PlayerOnScreen())
                        {
                            if (chkPlayerSound.Checked)
                                soundAlert.PlaySync();
                                Thread.Sleep(100);
                            if(chkPlayerFlash.Checked)
                                WinApi.FlashWindow(Memory.process.MainWindowHandle, false);
                            if (chkPlayerPause.Checked)
                                PauseAll();
                            if(chkPlayerLogout.Checked)
                                SendKeys.SendWait("^l");

                        }

                    }
                    Thread.Sleep(50);
                }
            }
        }

        private void PauseAll()
        {
            chkCavebot.Checked = false;
            chkScript.Checked = false;
        }

        int GMTicks = 1000;
        private bool GamemasterOnScreen()
        {
            foreach (Creature c in new Battlelist().GetCreatures())
            {
                if (c.Id != Memory.ReadInt(Addresses.Player.Id))
                {
                    if (c.Type == (byte)Constants.Type.PLAYER)
                    {
                        if (chkAnyFloor.Checked)
                        {
                            if (c.Name == "Joao" || c.Name.Substring(0, 3) == "ADM")
                                return true;
                        }
                        else
                        {
                            if (c.Z == player.Z)
                            {
                                if (c.Name == "Joao" || c.Name.Substring(0, 3) == "ADM")
                                    return true;
                            }
                        }
                    }
                }
            }
            //seems like no ADMon screen, maybe they wrote something?!
            string name = Client.LastMessageName;
            if (name != "")
            {

                if (name.Contains("Joao") || name.Substring(0, 3) == "ADM")
                {
                    return true;
                }
            }
            
   //         if (chkGmLogin.Checked)              /// Verifica se o gm está online a cada 60 segundos mais ou menos.
   //         {
   //             if (!GMOnline)
   //             {
   //                 if (++GMTicks > 1000)
   //                 {
   //                     GMTicks = 0;
   //                     if (CheckGMLogin())
   //                     {
   //                         GMOnline = true;
   //                     }
   //                 }
   //             }
   //             else
   //             {
   //                 return true;
   //             }
   //         }

            return false;
        }

        private bool PlayerOnScreen()
        {
            foreach (Creature c in new Battlelist().GetCreatures())
            {
                if (c.Id != Memory.ReadInt(Addresses.Player.Id))
                {
                    if (c.Type == (byte)Constants.Type.PLAYER)
                    {
                        if (chkAnyFloor.Checked)
                        {
                                bool isSafe = false;
                                foreach (string name in lstSafe.Items)
                                {
                                    if (name == c.Name)
                                    {
                                        isSafe = true;
                                    }
                                }
                                if (!isSafe)
                                    if (c.Name != " Joao" || c.Name.Substring(0, 3) != "ADM")
                                        return true;
                        }
                        else
                        {
                            if (c.Z == player.Z)
                            {
                                bool isSafe = false;
                                foreach (string name in lstSafe.Items)
                                {
                                    if (name == c.Name)
                                    {
                                        isSafe = true;
                                    }
                                }
                                if (!isSafe)
                                    if (c.Name != "Joao" || c.Name.Substring(0, 3) != "ADM")
                                        return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private void Script()
        {
            dataGridView1.Enabled = false;
            while (threadScript.IsAlive)
            {
                if (chkScript.Checked && Client.Connection == 8)
                {
                    foreach (string line in txtScript.Lines)
                    {
                        EvaluateScript(line);
                    }
                }
                if (chkCond.Checked)
                {
                    if (dataGridView1.Rows.Count > 1)
                    {
                        for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                        {
                            string[] tmpCond = new string[4];
                            for (int j = 0; j < 4; j++)
                            {
                                tmpCond[j] = (string)dataGridView1[j, i].Value;
                            }
                            ExecuteConditionalEvent(tmpCond);
                            Thread.Sleep(100);
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }

        private void ExecuteConditionalEvent(string[] cond)
        {
            // 0 = thing1
            // 1 = operator
            // 2 = thing2
            // 3 = action

            string str = "";
            foreach (string c in cond)
            {
                str += c;
            }
            Debug.WriteLine(str);
            if (IsConditionalEventTrue(cond[0], cond[1], cond[2]))
            {
                string cmd = "";
                for (int i = 3; i < cond.Length; i++)
                {
                    cmd += cond[i];
                    cmd += " ";
                }
                string[] command = new string[cond.Length - 3];
                for (int i = 0; i < command.Length; i++)
                {
                    command[i] = cond[i+3];

                }
                ExecuteCommand(command);
            }
        }

        private bool IsConditionalEventTrue(string thing1, string op, string thing2)
        {
            uint t1 = 0;
            uint t2 = UInt32.Parse(thing2);
            if (thing1.Contains('.'))
            {
                int pos = thing1.IndexOf('.');
                string part1 = thing1.Substring(0,pos);
                string part2 = thing1.Substring(pos+1, thing1.Length -pos-1);
                if (part1 == "$target")
                {
                    Creature creature = null;
                    foreach (Creature c in new Battlelist().GetCreatures())
                    {
                        if (c.Id == player.TargetId)
                        {
                            creature = c;
                            break;
                        }
                    }
                    if (creature == null) return false;
                    switch(part2)
                    {
                        case "hppc":
                            t1 = (uint)creature.HealthBar;
                            break;
                        case "posx":
                            t1 = (uint)creature.X;
                            break;
                        case "posy":
                            t1 = (uint)creature.Y;
                            break;
                        case "posz":
                            t1 = (uint)creature.Z;
                            break;
                    }
                }
            }
            else
            {
                switch (thing1)
                {
                    case "$hp":
                        t1 = (uint)player.Health;
                        break;
                    case "$hppc":
                        t1 = (uint)player.HealthBar;
                        break;
                    case "$mp":
                        t1 = (uint)player.Mana;
                        break;
                    case "$mppc":
                        t1 = (uint)player.ManaPercent;
                        break;
                    case "$targethp":
                        t1 = 100;
                        foreach (Creature c in new Battlelist().GetCreatures())
                        {
                            if (c.Type == (byte)Constants.Type.PLAYER)
                            {
                                if (c.Id == player.TargetId)
                                {
                                    t1 = (uint)c.HealthBar;
                                }
                            }
                        }
                        break;
                    case "$posx":
                        t1 = (uint)player.X;
                        break;
                    case "$posy":
                        t1 = (uint)player.Y;
                        break;
                    case "$posz":
                        t1 = (uint)player.Z;
                        break;
                    case "$neck":
                        t1 = player.SlotNeck;
                        break;
                    case "$rhand":
                        t1 = player.SlotRightHand;
                        break;
                    case "$lhand":
                        t1 = player.SlotLeftHand;
                        break;
                    case "$ring":
                        t1 = player.SlotRing;
                        break;
                    case "$ammo":
                        t1 = player.SlotAmmo;
                        break;
                    case "$cap":
                        t1 = (uint)player.Cap;
                        break;
                    case "$level":
                        t1 = (uint)player.Level;
                        break;
                    case "$mlevel":
                        t1 = (uint)player.MagicLevel;
                        break;
                    case "$exp":
                        t1 = (uint)player.Experience;
                        break;
                    case "$manashield":
                        t1 = Convert.ToUInt32(player.HasFlag(Constants.Flag.MANA_SHIELD));
                        break;
                    case "$haste":
                        t1 = Convert.ToUInt32(player.HasFlag(Constants.Flag.HASTE));
                        break;
                    case "$battle":
                        t1 = Convert.ToUInt32(player.HasFlag(Constants.Flag.BATTLE));
                        break;
                    case "$paralyze":
                        t1 = Convert.ToUInt32(player.HasFlag(Constants.Flag.PARALYZED));
                        break;
                    default:
                        return false;
                }
            }
            if (op == "<")
                if (t1 < t2) return true;
            if (op == ">")
                if (t1 > t2) return true;
            if (op == "<=")
                if (t1 < t2) return true;
            if (op == ">=")
                if (t1 > t2) return true;
            if (op == "==")
                if (t1 == t2) return true;
            if (op == "!=")
                if (t1 != t2) return true;

            return false;
        }

        private Player GetPlayer()
        {
            foreach (Creature c in new Battlelist().GetCreatures())
            {
                if (c.Id == Memory.ReadInt(Addresses.Player.Id))
                {
                    return new Player(c.Address);
                }
            }
            return null;
        }

        private string GMLastLogin()
        {
            return "never";  // Função desativada e codigo guardado para uso posterior.
            try
            {
                WebRequest request = WebRequest.Create("http://tibianic-hr.org/community/character/Joao/");
                request.Credentials = CredentialCache.DefaultCredentials;
                WebResponse response = request.GetResponse();
                Debug.WriteLine(((HttpWebResponse)response).StatusDescription);
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                while (reader.ReadLine() != null)
                {
                    string tmp = reader.ReadLine();
                    string srcBegin = "Last login:</td><td valign = \"top\">";
                    string srcEnd = "</td></tr><tr><td valign = \"top\">Account Status:";
                    //Last login:</td><td valign = "top">24 June 2012, 22:26:43 CET</td></tr><tr>
                    if (tmp == null) continue;
                    if (tmp.Contains("Last login:"))
                    {
                        int i = tmp.IndexOf(srcBegin);
                        i += srcBegin.Length;
                        int j = tmp.IndexOf(srcEnd);
                        string login = tmp.Substring(i, j - i);
                        return login;
                    }
                }
                reader.Close();
                response.Close();
            }
            catch (Exception e)
            {
                Debug.Write(e.StackTrace);
            }
            return "ERROR";
        }

        private bool CheckGMLogin()
        {
            return false; // Função desativada e c´´o
            Client.StatusbarMessage= "CHECKING IRYONG LOGIN";
            try
            {
                string lastlogin = "never";
                WebRequest request = WebRequest.Create("http://tibianic-hr.org/community/character/Joao/");
                request.Credentials = CredentialCache.DefaultCredentials;
                WebResponse response = request.GetResponse();
                Debug.WriteLine(((HttpWebResponse)response).StatusDescription);
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                while (reader.ReadLine() != null)
                {
                    string tmp = reader.ReadLine();
                    string srcBegin = "Last login:</td><td valign = \"top\">";
                    string srcEnd = "</td></tr><tr><td valign = \"top\">Account Status:";
                    //Last login:</td><td valign = "top">24 June 2012, 22:26:43 CET</td></tr><tr>
                    if (tmp == null) continue;
                    if (tmp.Contains("Last login:"))
                    {
                        int i = tmp.IndexOf(srcBegin);
                        i += srcBegin.Length;
                        int j = tmp.IndexOf(srcEnd);
                        string login = tmp.Substring(i, j - i);
                        Debug.WriteLine(login);
                        Debug.WriteLine(lastlogin);
                        if (login.Equals(lastlogin))
                        {
                            return false;
                        }
                        else
                        {
                            //MessageBox.Show(login + Environment.NewLine + lastlogin + Environment.NewLine + "Seems like ADMis ONLINE");
                            return true;
                        }
                    }
                }
                reader.Close();
                response.Close();
            }
            catch (Exception e)
            {
                Debug.Write(e.StackTrace);
            }
            return false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //TakeTheLoot();
            //CheckGMLogin();
            //TakeTheLoot();
            //CheckSpawn(true);
            //ExecuteCommand(txtCommand.Text);
            //player.isWalk = 1;
            //player.WalkTo(player.X-4,player.Y,player.Z);
            //Utils.SendKeys(player.Name);
            //Utils.SendKeys("hey");
            //ExecuteCommand(txtCommand.Text);
            //Debug.WriteLine(Client.LastMessageString);
            //ExecuteCommand(txtCommand.Text);
        }

        private bool StopAll()
        {
            chkScript.Checked = false;
            chkCavebot.Checked = false;
            chkCond.Checked = false;
            return true;
        }

        private void Exit()
        {
            threadScript.Abort();
            threadMain.Abort();
            threadCavebot.Abort();
            Application.Exit();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Exit();
        }

        private void btnRefreshTags_Click(object sender, EventArgs e)
        {
            RefreshTags();
        }

        private void lstTags_DoubleClick(object sender, EventArgs e)
        {
            if (lstTags.SelectedIndex != -1)
            {
                foreach (string k in ht.Keys)
                {
                    string theKey = k;
                    string selected = lstTags.SelectedItem.ToString();
                    int len = selected.IndexOf(' ');
                    if (selected.Substring(0, len) == theKey)
                    {
                        Point tmp;
                        tmp = (Point)ht[k.ToString()];
                        Cursor.Position = tmp;
                        return;
                    }
                }
            }
        }

        private void ExecuteCommand(string[] command)
        {
            scriptWorking = true;
            lock(this)
            {
                string sfrom, sto;
                Point pfrom, pto;
                int used = 0;
                switch (command[0])
                {
                    case "$rclick":
                        foreach (string k in ht.Keys)
                        {
                            if (command[1] == k.ToString())
                            {
                                Point tmp;
                                tmp = (Point)ht[k.ToString()];
                                //Input.ClickRightMouseButton(tmp.X, tmp.Y);
                                Utils.MakeRightClick(tmp.X, tmp.Y);
                                used = 2;
                                break;
                            }
                        }
                        break;
                    case "$lclick":
                        foreach (string k in ht.Keys)
                        {
                            if (command[1] == k.ToString())
                            {
                                Point tmp;
                                tmp = (Point)ht[k.ToString()];
                                //Input.ClickLeftMouseButton(tmp.X, tmp.Y);
                                Utils.MakeLeftClick(tmp.X, tmp.Y);
                                used = 2;
                                break;
                            }
                        }
                        break;
                    case "$wait":
                        scriptWorking = false;
                        int millis = Int32.Parse(command[1]);
                        used = 2;
                        Thread.Sleep(millis);
                        break;
                    case "$key":
                        Utils.SendKeys(command[1]);
                        used = 2;
                        break;
                    case "$drag":
                        sfrom = command[1];
                        sto = command[2];
                        pfrom = GetPoint(sfrom);
                        pto = GetPoint(sto);
                        Input.DragMouse(pfrom, pto);
                        //Utils.DragMouse(pfrom, pto);
                        used = 3;
                        break;
                    case "$dragsmooth":
                        sfrom = command[1];
                        sto = command[2];
                        pfrom = GetPoint(sfrom);
                        pto = GetPoint(sto);
                        Input.DragMouseSmooth(pfrom, pto);
                        used = 3;
                        break;
                    case "$gotoline":
                        try
                        {
                            int line = Int32.Parse(command[1]);
                            lstWaypoints.SelectedIndex = line;
                            used = 2;
                        }
                        catch (ArgumentOutOfRangeException e)
                        {
                            Debug.WriteLine(e.StackTrace);
                            throw;
                        }
                        break;
                    case "$gotolabel":
                        string thelabel;
                        if (command[1].Substring(command[1].Length - 1, 1) != ":")
                            thelabel = command[1] + ":";
                        else
                            thelabel = command[1];
                        try
                        {
                            for (int i = 0; i < lstWaypoints.Items.Count - 1; i++)
                            {
                                if (thelabel == lstWaypoints.Items[i].ToString())
                                {
                                    nextWaypoint(i);
                                    used = 2;
                                    break;
                                }
                            }
                        }
                        catch(Exception e)
                        {
                            Debug.WriteLine(e.StackTrace);
                        }
                        break;
                    case "$playsound":
                        soundAlert.Play();
                        used = 1;
                        break;
                    case "$pausescript":
                        chkScript.Checked = false;
                        used = 1;
                        break;
                    case "$resumescript":
                        chkScript.Checked = true;
                        used = 1;
                        break;
                    case "$pausecavebot":
                        chkCavebot.Checked = false;
                        used = 1;
                        break;
                    case "$resumecavebot":
                        chkCavebot.Checked = true;
                        used = 1;
                        break;
                    case "$target":
                        //get target
                        Point p1 = GetPoint("#topleft");
                        Point p2 = GetPoint("#bottomright");
                        int tileX = (p2.X - p1.X) / 14;
                        int tileY = (p2.Y - p1.Y) / 11;
                        Creature target = null;
                        foreach (Creature c in new Battlelist().GetCreatures())
                        {
                            if (c.Id == player.TargetId)
                            {
                                target = c;
                                break;
                            }
                        }
                        if (target == null)
                        {
                            used = 1;
                            break;
                        }
                        //get offsets
                        int offsetX, offsetY;
                        offsetX = target.X - player.X;
                        offsetY = target.Y - player.Y;
                        offsetX *= tileX;
                        offsetY *= tileY;
                        int pX = (p2.X - p1.X) / 2 + p1.X;
                        int pY = (p2.Y - p1.Y) / 2 + p1.Y;
                        offsetX += pX;
                        offsetY += pY;
                        Input.ClickLeftMouseButton(offsetX, offsetY);
                        used = 1;
                        break;
                    default:
                        scriptWorking = false;
                        return;
                }
                //check remainder
                if (command.Length > used)
                {
                    string[] remainder = new string[command.Length - used];
                    for(int i = 0; i < remainder.Length; i++)
                    {
                        remainder[i] = command[i + used];
                    }
                    ExecuteCommand(remainder);
                }
            }
            scriptWorking = false;
        }

        private Point GetPoint(string key)
        {
            Point p;
            p = new Point(-1, -1);
            foreach (string k in ht.Keys)
            {
                if (key == k.ToString())
                {
                    p = (Point)ht[k.ToString()];
                    return p;
                }
            }
            MessageBox.Show("Could not find tag: " + key);
            return p;
        }

        private void Cavebot()
        {
            Point p1 = GetPoint("#topleft");    // new Point(324, 31);
            Point p2 = GetPoint("#bottomright"); //new Point(1416, 830);
            //Point pAttack = GetPoint("#firstbattle");
            int tileX = (p2.X - p1.X) / 14;
            int tileY = (p2.Y - p1.Y) / 11;
            int tileSize = 55;
            while (threadCavebot.IsAlive)
            {
            theLabel:
                if (scriptWorking)
                {
                    Thread.Sleep(100);
                    goto theLabel;
                }
                if (chkCavebot.Checked && Client.Connection == 8)
                {
                    if (player.TargetId == 0)
                    {
                        //is there any targets?
                        bool foundTarget = false;
                        Creature target = null;
                        int fromX = 0;
                        int fromY = 0;
                        foreach (Creature c in new Battlelist().GetCreatures())
                        {
                            if (c.Type == (byte)Constants.Type.CREATURE)
                            {
                                if (c.Z == player.Z)
                                {
                                    int x = Math.Abs(player.X - c.X);
                                    int y = Math.Abs(player.Y - c.Y);
                                    int rng = Int32.Parse(txtRange.Text);
                                    if (x <= rng && y <= rng)
                                    {
                                        if (target == null) target = c;
                                        if (c.Location.DistanceTo(target.Location) < player.Location.DistanceTo(target.Location))
                                        {
                                            target = c;
                                        }
                                    }
                                }
                            }
                        }
                        if (target != null)
                        {
                            //int x = Math.Abs(player.X - target.X);
                            //int y = Math.Abs(player.Y - target.Y);
                            fromX = target.X - player.X;
                            fromY = target.Y - player.Y;
                            fromX *= tileX;
                            fromY *= tileY;
                            int pX = (p2.X - p1.X) / 2 + p1.X; //865
                            int pY = (p2.Y - p1.Y) / 2 + p1.Y; // 425
                            fromX += pX;
                            fromY += pY;
                            foundTarget = true;
                        }
                        if (foundTarget)
                        {
                            //Input.ClickRightMouseButton(fromX, fromY);
                            Utils.MakeRightClick(fromX, fromY);
                            Thread.Sleep(500);
                            continue;
                        }

                        //if not walk
                        Walker();
                    }
                    else
                    {
                        //we are attacking
                        foreach (Creature c in new Battlelist().GetCreatures())
                        {
                            if (c.Type == (byte)Constants.Type.CREATURE)
                            {
                                if (c.Id == player.TargetId)
                                {
                                    int x = Math.Abs(player.X - c.X);
                                    int y = Math.Abs(player.Y - c.Y);
                                    int rng = Int32.Parse(txtRange.Text);
                                    if (x > rng || y > rng)
                                    {
                                        //target out of range, stop attacking
                                        player.TargetId = 0;
                                    }
                                    if (c.HealthBar < 1)
                                    {
                                        //dedz
                                        int fromX = c.X - player.X;
                                        int fromY = c.Y - player.Y;
                                        fromX *= tileSize;
                                        fromY *= tileSize;
                                        Thread.Sleep(1000);
                                        int pX = (p2.X - p1.X) / 2 + p1.X; //865
                                        int pY = (p2.Y - p1.Y) / 2 + p1.Y; // 425
                                        //Input.ClickRightMouseButton(fromX += pX, fromY+= pY); //player pos
                                        Utils.MakeRightClick(fromX += pX, fromY += pY);
                                        Thread.Sleep(200); //wait for container to open
                                        TakeTheLoot();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                Thread.Sleep(10);
            }
        }

        private void Walker()
        {
            if (lstWaypoints.SelectedIndex == -1) lstWaypoints.SelectedIndex = 0;
            
            string str = lstWaypoints.SelectedItem.ToString();
            if (str.Contains('$'))
            {
                EvaluateScript(str);
                Thread.Sleep(500);
                
                nextWaypoint();
                return;
            }
            else if (str.Contains(':'))
            {
                nextWaypoint();
            }
            else
            {
                string[] pos = str.Split(',');
                Location loc = new Location(Int32.Parse(pos[1]), Int32.Parse(pos[2]), Int32.Parse(pos[3]));
                if (pos[0] == "W")
                {
                    player.WalkTo(loc);
                    if (player.Location.IsAdjacentTo(loc, 0) || player.Z != loc.Z)
                    {
                        nextWaypoint();
                    }
                }
                else if (pos[0] == "N")
                {
                    player.WalkTo(loc);
                    if (player.Location.IsAdjacentTo(loc, 4) || player.Z != loc.Z)
                    {
                        nextWaypoint();
                    }
                }
                if (Client.StatusbarMessage == "There is no way.")
                {
                    Client.StatusbarMessage = "";
                    nextWaypoint();
                }
            }
        }

        private void nextWaypoint()
        {
            Thread.Sleep(1000);
            if (lstWaypoints.SelectedIndex >= lstWaypoints.Items.Count - 1)
                lstWaypoints.SelectedIndex = 0;
            else
                lstWaypoints.SelectedIndex = lstWaypoints.SelectedIndex + 1;
        }

        private void nextWaypoint(int num)
        {
            lstWaypoints.SelectedIndex = num;
            nextWaypoint();
        }

        private void btnAddWaypoint_Click(object sender, EventArgs e)
        {
            if (cmbEmplacement.Text == "" || cmbEmplacement.Text == "C") lstWaypoints.Items.Add("W, " + new Location(player.X, player.Y, player.Z));
            if (cmbEmplacement.Text == "N") lstWaypoints.Items.Add("W, " + new Location(player.X, player.Y - 1, player.Z));
            if (cmbEmplacement.Text == "E") lstWaypoints.Items.Add("W, " + new Location(player.X + 1, player.Y, player.Z));
            if (cmbEmplacement.Text == "S") lstWaypoints.Items.Add("W, " + new Location(player.X, player.Y + 1, player.Z));
            if (cmbEmplacement.Text == "W") lstWaypoints.Items.Add("W, " + new Location(player.X - 1, player.Y, player.Z));
        }

        private void btnNode_Click(object sender, EventArgs e)
        {
            if (cmbEmplacement.Text == "" || cmbEmplacement.Text == "C") lstWaypoints.Items.Add("N, " + new Location(player.X, player.Y, player.Z));
            if (cmbEmplacement.Text == "N") lstWaypoints.Items.Add("N, " + new Location(player.X, player.Y - 1, player.Z));
            if (cmbEmplacement.Text == "E") lstWaypoints.Items.Add("N, " + new Location(player.X + 1, player.Y, player.Z));
            if (cmbEmplacement.Text == "S") lstWaypoints.Items.Add("N, " + new Location(player.X, player.Y + 1, player.Z));
            if (cmbEmplacement.Text == "W") lstWaypoints.Items.Add("N, " + new Location(player.X - 1, player.Y, player.Z));
        }

        private Point GetItemPosition(int cont, int slot)
        {
            int x = 0, y = 0;

            for (int i = 0; i < cont + 1; i++)
            {
                int gui = Memory.ReadInt(Addresses.Client.GuiPointer);
                int offset = Memory.ReadInt(gui + 0x24);
                offset = Memory.ReadInt(offset + 0x24);

                for(int j = 0; j < i; j++)
                    offset = Memory.ReadInt(offset + 0x10);
                
                offset = Memory.ReadInt(offset + 0x44);
                y = y + Memory.ReadInt(offset + 0x20);
                y += 15;
            }

            return new Point(x, y);
        }

        private void TakeTheLoot()
        {
            Point lastSlot = GetPoint("#newcont");
            Point pTake = GetPoint("#takefrom");
            Point pRelease = GetPoint("#putat");
            int tries = 0;
            foreach (Objects.Container cont in new Inventory().GetContainers())
            {
                //if (cont.Name.ToLower().Contains("bag") || cont.Name.ToLower().Contains("backpack")) continue;
                if (cont.Name.ToLower().Contains("backpack") || cont.Name.ToLower().Contains("bag"))
                {
                    if (cont.Number == 1)
                    {
                        foreach (Item i in cont.GetItems())
                        {
                            if (i.Location.ContainerSlot == 0 && !lstLoot.Items.Contains(i.Id.ToString()))//i.Id == 1987)
                            {
                                Input.DragMouse(GetPoint("#putat"), GetPoint("#self"));
                                if (i.Count > 0)
                                {
                                    Thread.Sleep(200);
                                    Utils.SendKeys("ENTER");
                                }
                            }
                        }
                    }
                    if (cont.Amount >= cont.Volume)
                    {
                        Input.ClickRightMouseButton(lastSlot.X, lastSlot.Y);
                        Thread.Sleep(500);
                    }
                }
                if (cont.Number == 1 || cont.Number > 8) continue;
            looter:
                foreach (Item i in cont.GetItems().Reverse<Item>())
                {
                    if (lstLoot.Items.Contains(i.Id.ToString()))
                    {
                        Point p = GetItemPosition(cont.Number, i.Location.ContainerSlot);
                        pTake = GetPoint("#takefrom");
                        pTake.Y += p.Y;

                        pTake.X += 33 * i.Location.ContainerSlot;

                        Input.DragMouse(pTake, pRelease);
                        if (i.Count > 0)
                        {
                            Thread.Sleep(200);
                            Utils.SendKeys("ENTER");
                        }

                        Thread.Sleep(300);
                        continue;
                    }
                    else if (foods.Contains(i.Id) && cont.Number == 2)
                    {
                        Point p = GetItemPosition(cont.Number, i.Location.ContainerSlot);
                        Point pEat = GetPoint("#takefrom");
                        pEat.Y += p.Y;
                        pEat.X += 33 * i.Location.ContainerSlot;
                        Input.ClickRightMouseButton(pEat.X, pEat.Y);
                        Thread.Sleep(500);
                    }
                }
                //find and open bag if it exit
                bool foundBag = false;
                foreach (Item i in cont.GetItems().Reverse<Item>())
                {
                    if (i.Id == 1987)
                    {
                        Point p = GetItemPosition(cont.Number, i.Location.ContainerSlot);
                        pTake = GetPoint("#takefrom");
                        pTake.Y += p.Y;
                        pTake.X += 33 * i.Location.ContainerSlot;
                        Cursor.Position = pTake;
                        Input.ClickRightMouseButton(pTake.X, pTake.Y);
                        Thread.Sleep(300);
                        foundBag = true;
                    }
                }
                tries++;
                if (tries > 2) return;
                if (foundBag)
                {
                    foundBag = false;
                    goto looter;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(txtCavebotAction.Text.Length > 0)
                lstWaypoints.Items.Add(txtCavebotAction.Text);
        }

        private void btnCavebotClear_Click(object sender, EventArgs e)
        {
            lstWaypoints.Items.Clear();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (txtCavebotSave.Text == "") return;
            string file = txtCavebotSave.Text;
            TextWriter tw = new StreamWriter(Application.StartupPath + "/waypoints\\" + file + ".txt");
            for (int i = 0; i < lstWaypoints.Items.Count; i++)
            {
                tw.WriteLine(lstWaypoints.Items[i].ToString());
            }
            tw.Close();
            LoadCavebotScripts();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (lstCavebotScripts.SelectedIndex == -1) return;
            lstWaypoints.Items.Clear();
            string line;
            string file = lstCavebotScripts.Items[lstCavebotScripts.SelectedIndex].ToString();
            TextReader tr = new StreamReader(Application.StartupPath + "/waypoints\\" + file + ".txt");
            while ((line = tr.ReadLine()) != null)
            {
                lstWaypoints.Items.Add(line);
            }
            tr.Close();
        }

        private void btnTagsLoad_Click(object sender, EventArgs e)
        {
            LoadTags();
        }

        private void btnTagsSave_Click(object sender, EventArgs e)
        {
            SaveTags();
        }
        
        private void lstWaypoints_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblCavebotLine.Text = "Current line: " + lstWaypoints.SelectedIndex;
        }

        private void lstWaypoints_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                lstWaypoints.Items.RemoveAt(lstWaypoints.SelectedIndex);
            }
        }

        private void chkScript_CheckedChanged(object sender, EventArgs e)
        {
            if (chkScript.Checked)
                txtScript.ReadOnly = true;
            else
                txtScript.ReadOnly = false;
        }

        private void lstWaypoints_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = lstWaypoints.SelectedIndex;
            string text = lstWaypoints.Items[index].ToString();
            if(text.Contains('$'))
            {
                if (InputBox.Show("Change action", text, ref text) == DialogResult.OK)
                {
                    lstWaypoints.Items[index] = text;
                }
            }
        }

        private void txtSafe_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                lstSafe.Items.Add(txtSafe.Text);
                txtSafe.Text = "";
            }
        }

        private void lstSafe_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if(lstSafe.SelectedIndex != -1)
                    lstSafe.Items.RemoveAt(lstSafe.SelectedIndex);
            }
        }

        private void btnLoadScript_Click(object sender, EventArgs e)
        {
            if (lstScripts.SelectedIndex == -1) return;
            string line;
            string file = lstScripts.Items[lstScripts.SelectedIndex].ToString();
            TextReader tr = new StreamReader(Application.StartupPath + "/scripts\\" + file + ".txt");
            txtScript.Text = "";
            while ((line = tr.ReadLine()) != null)
            {
                txtScript.AppendText(line + Environment.NewLine);
            }
            tr.Close();
        }

        private void btnSaveScript_Click(object sender, EventArgs e)
        {
            if (txtSaveScript.Text == "") return;
            string file = txtSaveScript.Text;
            TextWriter tw = new StreamWriter(Application.StartupPath + "/scripts\\" + file + ".txt");
            foreach (string line in txtSaveScript.Lines)
            {
                tw.WriteLine(line);
            }
            tw.Close();
            LoadScripts();
        }
        
        private void button1_Click_1(object sender, EventArgs e)
        {
            //Rectangle re = Client.GameView();
            ////MessageBox.Show(Client.GameView().ToString());
            //re.Y += 28;
            //Point p = new Point(re.X, re.Y);
            //Point p2 = new Point(re.X + re.Width, re.Y + re.Height);
            //Cursor.Position = p2;
            //TakeTheLoot();
            //SendKeys.SendWait("^l");
            
            string line = txtCommand.Text;
            EvaluateScript(line);
        }

        private void EvaluateScript(string line)
        {
            if (line.Contains('<') || line.Contains('>') || line.Contains("<=") || line.Contains(">=") || line.Contains("==") || line.Contains("!="))
                TranslateToCE(line);
            else
                TranslateToCommand(line);
        }

        private void TranslateToCE(string ce)
        {
            string[] str = ce.Split(' ');
            ExecuteConditionalEvent(str);
        }

        private void TranslateToCommand(string command)
        {
            string[] str = command.Split(' ');
            ExecuteCommand(str);
        }


        //74 address animated text
        void anim(int x, int y, int z, int col, string msg)
        {
            //43fb10 adr
            Memory.WriteInt(0x043FB10, x);
            Memory.WriteInt(0x043FB10, y);
            Memory.WriteInt(0x043FB10, z);
            Memory.WriteInt(0x043FB10, col);
            Memory.WriteString(0x043FB10, msg);
        }
        
        private void fullLightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fullLightToolStripMenuItem.Checked)
            {
                Client.SetLight(true);
            }
            else
            {
                Client.SetLight(false);
            }
        }

        private void nameSpyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (nameSpyToolStripMenuItem.Checked)
            {
                Client.NameSpyOn();
            }
            else
            {
                Client.NameSpyOff();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Utils.DragMouse(GetPoint("#sd"),GetPoint("#uh"));

            //Client.StatusbarMessage = player.ManaPercent.ToString();
            //TakeTheLoot();
        }

        private void onTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (onTopToolStripMenuItem.Checked)
                this.TopMost = true;
            else
                this.TopMost = false;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Exit();
        }

        private void btnAddCond_Click(object sender, EventArgs e)
        {
            if (ConditionalEventIsValid())
            {
                string[] ce = new string[4];
                ce[0] = txtThing1.Text;
                ce[1] = cmbOperator.Text;
                ce[2] = txtThing2.Text;
                ce[3] = txtAction.Text;
            }
        }

        private bool ConditionalEventIsValid()
        {
            if(txtThing1.Text.Length > 0 && cmbOperator.Text.Length > 0 && txtThing2.Text.Length > 0 && txtAction.Text.Length > 0)
            {
                dataGridView1.Rows.Add(txtThing1.Text,cmbOperator.Text,txtThing2.Text,txtAction.Text);
                return true;
            }
            return false;
        }

        private void btnLabel_Click(object sender, EventArgs e)
        {
            if (txtCavebotLabel.Text.Length > 0)
                lstWaypoints.Items.Add(txtCavebotLabel.Text + ":");
        }

        private void reloadPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Memory.GetHandle();
            LoadPlayer();
            WinApi.FlashWindow(Memory.process.MainWindowHandle, false);
        }

        private void LoadPlayer()
        {
            player = GetPlayer();
            this.Text = player.Name + " | ClassicBotter " + Application.ProductVersion;
        }

        private void lstLoot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete)
                lstLoot.Items.RemoveAt(lstLoot.SelectedIndex);
        }

        private void txtLoot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                lstLoot.Items.Add(txtLoot.Text);
                txtLoot.Text = "";
            }
        }

        private void txtRange_TextChanged(object sender, EventArgs e)
        {
            int rng;

            Int32.TryParse(txtRange.Text, out rng);

            if (rng <= 0 || rng >= 5)
                txtRange.Text = "5";
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


    }
}
