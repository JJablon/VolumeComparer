using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace VolumeCompare
{
    public partial class Form1 : Form
    {
        string loc = Application.StartupPath;

        StreamReader to = null;
        StreamReader from = null;
        FileInfo fi_open_config_from;
        FileInfo fi_open_config_to;

        public long total, processed, output;
        System.Collections.Specialized.NameValueCollection files = new NameValueCollection();
        System.Collections.Specialized.NameValueCollection directories = new NameValueCollection();

        List<string> log1 = new List<string>();
        List<string> log2 = new List<string>();
        List<string> log3 = new List<string>();
        List<string> log4 = new List<string>();
        public Form1()
        {

            InitializeComponent();
            backgroundWorker1.WorkerSupportsCancellation = true;
            fi_open_config_from = new FileInfo(loc + "\\from.cs");
             fi_open_config_to = new FileInfo(loc + "\\to.cs");

            if (!fi_open_config_from.Exists)
            {
                 fi_open_config_from.CreateText();
            }
            else
            {
                from = fi_open_config_from.OpenText();
                string read = from.ReadToEnd();
                if (read.Length == 0)
                {

                }
                else
                {
                    this.textBox1.Text = read;
                    folderBrowserDialog1.SelectedPath = read;
                }


            }

           

            try { from.Close(); } catch (Exception) { }





            if (!fi_open_config_to.Exists) {  fi_open_config_to.CreateText();

            }
            else
            {
                to = fi_open_config_to.OpenText();
                string read = to.ReadToEnd();
                if (read.Length == 0)
                {

                }
                else
                {
                    this.textBox2.Text = read;
                    folderBrowserDialog2.SelectedPath = read;
                }
                try { to.Close(); } catch (Exception) { }
            }
           

        }


     
        public string Md5Sum(string file)
        {

            using (var stream = new BufferedStream(File.OpenRead(file), 1200000))
            {
                //SHA256Managed sha = new SHA256Managed();
                MD5 sha = new MD5CryptoServiceProvider();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }


        }
        public  string Md5SumByProcess(string file)
        {
            var p = new Process();
            p.StartInfo.FileName = loc + "\\md5sum.exe";
            p.StartInfo.Arguments = "\""+file+ "\"";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            p.WaitForExit();
            string output = p.StandardOutput.ReadToEnd();
            if (output.Length > 0)
                return output.Split(' ')[0].Substring(1).ToUpper();
            else return null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                this.textBox1.Text = folderBrowserDialog1.SelectedPath;
                StreamWriter s1 = new StreamWriter(fi_open_config_from.FullName);
                s1.Write(this.textBox1.Text);
                s1.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog2.ShowDialog() == DialogResult.OK)
            {
                this.textBox2.Text = folderBrowserDialog2.SelectedPath;
                StreamWriter s2 = new StreamWriter(fi_open_config_to.FullName);
                s2.Write(this.textBox2.Text);
                s2.Close();
            }
        }

        /*
        private void Process()
        {
            DirectoryInfo di = new DirectoryInfo(textBox1.Text);
            DirectoryInfo di2 = new DirectoryInfo(textBox2.Text);
            try
            {
                foreach (var fi in di.GetFileSystemInfos("*.*", SearchOption.AllDirectories))
                {
                    string s1 = Md5SumByProcess(fi.FullName);//.ToUpper();
                    string pth = fi.FullName.Replace(textBox1.Text, " ");
                    pth = textBox2.Text + "\\" + pth;
                    DirectoryInfo di11 = new DirectoryInfo(pth);
                    FileInfo fi11 = new FileInfo(pth);
                    if (di11.Exists) { }
                    else
                    {
                        if (fi11.Exists)
                        {
                            string s2 = Md5SumByProcess(fi11.FullName);//.ToUpper();
                            if (String.Compare(s1, s2) != 0) log1.Add(fi1.FullName + "             !=             " + fi11.FullName);
                            //listBox1.Items.Add( fi1.FullName+ "             !=             " + fi11.FullName);
                        }
                        else
                        {
                            //listBox1.Items.Add("Cant find:" + fi11.FullName);
                            log1.Add("Cant find:" + fi11.FullName);
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Nie można uzyskać dostępu do folderu");
            }
            catch (Exception e)
            {
                MessageBox.Show("Inny błąd: " + e.Message + "\n\n" + e.Data);
            }
        }
        */







        private void TreeScan(string sDir)
        {
            try { 
            foreach (string f in Directory.GetFiles(sDir))
            {
                FileInfo fi = new FileInfo(f);
                    if (!fi.Name.Contains("ntuser.dat".ToUpper())&& !fi.Name.Contains("ntuser.dat.LOG"))
                    {
                        total += fi.Length;
                        files.Add(f, fi.Length.ToString());
                    }
            }
            foreach (string d in Directory.GetDirectories(sDir))
            {
                try
                {
                    TreeScan(d);
                }
                catch (Exception)
                {
                    log2.Add("Cant access dir:   " + d);
                }
            }
            }
            catch (Exception)
            {
                log2.Add("Cant access dir:   " + sDir);
            }
        }

        

        DateTime dt2, dt1;
        private void run_Click(object sender, EventArgs e)
        {
           
            if (backgroundWorker1.IsBusy != true)
            {
                this.toolStripProgressBar1.Value = 0;
                this.toolStripStatusLabel1.Text = "0%";
                dt1 = DateTime.Now;
                toolStripStatusLabel2.Visible = false;
                toolStripStatusLabel3.Visible = false;
                toolStripStatusLabel4.Visible = false;
                if (checkBox1.Checked)
                {
                    clearLogs();
                }

                backgroundWorker1.RunWorkerAsync();

            }

        }
           private void clearLogs()
        {


            listBox3.Items.Clear();
            listBox2.Items.Clear();
            listBox1.Items.Clear();
            listBox4.Items.Clear();
            log1.Clear();
            log2.Clear();
            log3.Clear();
            log4.Clear();
        }
        private void bw1_DoWork(object sender, DoWorkEventArgs ea)
        {
            complete = false;
            BackgroundWorker worker = sender as BackgroundWorker;
            total = 0; processed = 0; output = 0;
            files.Clear();
            TreeScan(textBox1.Text);

            timer1.Enabled = true;
            DirectoryInfo di = new DirectoryInfo(textBox1.Text);
            DirectoryInfo di2 = new DirectoryInfo(textBox2.Text);


            try
            {
                IEnumerator myEnumerator = files.GetEnumerator();
                foreach (String fi in files.AllKeys)
                {
                    
                        long fi_size = Int64.Parse(files[fi]);
                        string pth = fi;
                        DirectoryInfo di11 = null; ;
                        FileInfo fi11=null;
                        processed += fi_size;
                        pth = pth.Replace(textBox1.Text, "");
                        string pth_short = pth.Replace(textBox1.Text, "");
                        pth = textBox2.Text + pth;
                         string s1 = "";
                         try
                         {
                               s1 = Md5Sum(pth);
                                di11 = new DirectoryInfo(pth);
                                fi11 = new FileInfo(pth);
                                if (processed < total)
                                    worker.ReportProgress((int)(((double)((double)((double)processed / (double)total)) * ((double)100))));
                                else worker.ReportProgress(0);

                        }
                        catch (Exception)
                        {log2.Add("Cant scan source file: " + fi); 
                        }
                    
                
                        if (di11!=null&&di11.Exists && fi11!=null) { log3.Add(" "); log3.Add(" "); log3.Add("Directory:   " + fi11.FullName + "    maches directory: " + pth_short+"with content as follows: "); }
                        else if (di11!=null&&s1!=null)
                        {
                            if (fi11.Exists)
                            {
                                try
                                {
                                    output += fi11.Length;
                                    string s2 = Md5Sum(fi11.FullName);//.ToUpper();
                                    if (String.Compare(s1, s2) != 0)
                                    {
                                        log1.Add(fi + "             !=             " + fi11.FullName);
                                        log3.Add("!!!!!" + fi + "             !=             " + fi11.FullName);
                                    }
                                    else
                                    {
                                        log3.Add("File:   " + fi11.FullName + "    maches file: " + pth_short);
                                        log3.Add(s1 + "   ====   " + s2);

                                    }
                                }
                                catch(Exception) { log2.Add("Cant scan destination file: " + fi11.FullName); }
                            }
                            else
                            {
                                log2.Add("Can't find file/directory:    " + fi11.FullName);
                            }
                        }

                }
              

            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Can't access the directory; insufficient priviledges");
            }
            catch (Exception e)
            {
                MessageBox.Show("Other error: " + e.Message + "\n\n" + e.ToString());
            }



            timer1.Enabled = false;
        }

        private void b_next_folder_Click(object sender, EventArgs e)
        {
            complete = false;
            try
            {
                string dirnameout = "";
                string path1 = new DirectoryInfo(this.textBox1.Text).Parent.FullName;
                string path2 = new DirectoryInfo(this.textBox2.Text).Parent.FullName;
                string dirname1 = new DirectoryInfo(this.textBox1.Text).Name;
                string dirname2 = new DirectoryInfo(this.textBox2.Text).Name;

                long cnt1 = 0;
                foreach (var dir in new DirectoryInfo(path1).GetDirectories())
                {
                    if (dir.FullName == this.textBox1.Text)//Path.Combine(path1, dirname1))
                    {
                        if (cnt1 < (new DirectoryInfo(path1).GetDirectories()).Length) { dirnameout = new DirectoryInfo(path1).GetDirectories()[cnt1 + 1].FullName; break; }
                        else { MessageBox.Show("Reached the end of the directory!"); }
                    }
                    cnt1++;

                }
                if (dirnameout != "") {
                    DirectoryInfo di = new DirectoryInfo(dirnameout);
                    DirectoryInfo di2 = new DirectoryInfo(Path.Combine(path2, new DirectoryInfo(dirnameout).Name));

                    if (di.Exists && di2.Exists) { 
                        this.textBox1.Text = dirnameout;


                         this.textBox2.Text = Path.Combine(path2, new DirectoryInfo(dirnameout).Name);

                         StreamWriter s1 = new StreamWriter(fi_open_config_from.FullName);
                          s1.Write(this.textBox1.Text);
                           s1.Close();
                        StreamWriter s2 = new StreamWriter(fi_open_config_to.FullName);
                        s2.Write(this.textBox2.Text);
                        s2.Close();

                        if (backgroundWorker1.IsBusy != true)
                        {
                            this.toolStripProgressBar1.Value = 0;
                            this.toolStripStatusLabel1.Text = "0%";
                            dt1 = DateTime.Now;
                            toolStripStatusLabel2.Visible = false;
                            toolStripStatusLabel3.Visible = false;
                            toolStripStatusLabel4.Visible = false;
                            if (checkBox1.Checked)
                            {
                                clearLogs();
                            }

                            backgroundWorker1.RunWorkerAsync();

                        }

                    }
                    else
                    {
                        if (!di.Exists)
                            MessageBox.Show("Cant find the next directory: " + di.FullName);
                         if(!di2.Exists)
                            MessageBox.Show("Cant find the mirrored version in copied directory:"+ di2.FullName);
                    }





            }
            }
            catch (Exception) { }
        }

        private void toolStripStatusLabel2_Click(object sender, EventArgs e)
        {
            if(this.toolStripStatusLabel2.Visible == true)
            {
                this.tabControl1.SelectedIndex = 1;
            }
        }

        private void bw1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                toolStripStatusLabel1.Text = (e.ProgressPercentage.ToString() + "%");
                toolStripProgressBar1.Value = e.ProgressPercentage;
            }
            catch (Exception) { }
        }
        bool complete = false;
        private void bw1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            complete = true;
            dt2 = DateTime.Now;
            if(this.toolStripProgressBar1.Value<2)
                this.toolStripProgressBar1.Value = 100;
            this.toolStripStatusLabel1.Text = "100%";
            // MessageBox.Show((dt2 - dt1).TotalMilliseconds + "");
            log4.Add("Processed " + ((float)total / 1024f / 1024f).ToString("F") +" MB of files within "+ (dt2 - dt1).Minutes+" minutes and "+(dt2-dt1).Seconds+" seconds " );
            log4.Add("" + ((float)output / 1024f / 1024f).ToString("F") + " MB of files in the copy ");
            if((log1.Count != 0||log2.Count!=0)&&this.checkBox1.Checked == true) //when errors found
            {
                this.toolStripStatusLabel2.Visible = true;
                
            }
            else
            {
                this.toolStripStatusLabel3.Visible = true;
                toolStripStatusLabel4.Visible = true;
                try
                {
                    toolStripStatusLabel4.Text = this.textBox1.Text.Remove(0, textBox1.Text.LastIndexOf('\\'));
                }
                catch (Exception) { }
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 4 && listBox3.Items.Count != log3.Count)
            {
                listBox3.Items.Clear();
                listBox3.Items.AddRange(log3.ToArray());

            }
            if (tabControl1.SelectedIndex == 3 && listBox4.Items.Count != log4.Count)
            {
                listBox4.Items.Clear();
                listBox4.Items.AddRange(log4.ToArray());

            }
            if (tabControl1.SelectedIndex == 2 && listBox2.Items.Count != log2.Count)
            {
                listBox2.Items.Clear();
                listBox2.Items.AddRange(log2.ToArray());

            }
            if (tabControl1.SelectedIndex == 1 && listBox1.Items.Count != log1.Count)
            {
                listBox1.Items.Clear();
                listBox1.Items.AddRange(log1.ToArray());

            }
        }
    }
}
