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
        FileInfo fi1;
        FileInfo fi2;

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
            fi1 = new FileInfo(loc + "\\from.cs");
             fi2 = new FileInfo(loc + "\\to.cs");

            if (!fi1.Exists)
            {
                 fi1.CreateText();
            }
            else
            {
                from = fi1.OpenText();
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





            if (!fi2.Exists) {  fi2.CreateText();

            }
            else
            {
                to = fi2.OpenText();
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
                StreamWriter s1 = new StreamWriter(fi1.FullName);
                s1.Write(this.textBox1.Text);
                s1.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog2.ShowDialog() == DialogResult.OK)
            {
                this.textBox2.Text = folderBrowserDialog2.SelectedPath;
                StreamWriter s2 = new StreamWriter(fi2.FullName);
                s2.Write(this.textBox2.Text);
                s2.Close();
            }
        }


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








        private void TreeScan(string sDir)
        {
            foreach (string f in Directory.GetFiles(sDir))
            {
                FileInfo fi = new FileInfo(f);
                total += fi.Length;
                files.Add(f, fi.Length.ToString());
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

        


        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }
        DateTime dt2, dt1;
        private void button5_Click(object sender, EventArgs e)
        {
            dt1 = DateTime.Now;
            if (backgroundWorker1.IsBusy != true)
            {
                if(checkBox1.Checked) listBox3.Items.Clear();
                if (checkBox1.Checked) listBox2.Items.Clear();
                if (checkBox1.Checked) listBox1.Items.Clear();
                if (checkBox1.Checked) listBox4.Items.Clear();

                backgroundWorker1.RunWorkerAsync();

            }

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs ea)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            total = 0; processed = 0; output = 0;

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
                    string s1 = Md5Sum(pth);
                    processed += fi_size;
                    pth = pth.Replace(textBox1.Text, " ");
                    string pth_short = pth.Replace(textBox1.Text, " ");
                    pth = textBox2.Text + "\\" + pth;
                    DirectoryInfo di11 = new DirectoryInfo(pth);
                    FileInfo fi11 = new FileInfo(pth);
                    worker.ReportProgress((int)(((double)((double)((double)processed / (double)total)) * ((double)100))));

                    if (di11.Exists) { }
                    else
                    {
                        if (fi11.Exists)
                        {
                            output += fi11.Length;
                            string s2 = Md5Sum(fi11.FullName);//.ToUpper();
                            if (String.Compare(s1, s2) != 0)
                                log1.Add(fi1.FullName + "             !=             " + fi11.FullName);
                            else
                            {
                                log3.Add("File:   " + fi11.FullName + "    maches file:" + pth_short);
                                log3.Add(s1 + "   ====   " + s2);
                            }
                        }
                        else
                        {
                            log2.Add("Cant find file:    " + fi11.FullName);
                        }
                    }

                }
                listBox1.Items.AddRange(log1.ToArray());
                listBox2.Items.AddRange(log2.ToArray());

            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Can't access the directory; insufficient priviledges");
            }
            catch (Exception e)
            {
                MessageBox.Show("Other error: " + e.Message + "\n\n" + e.Data);
            }



            timer1.Enabled = false;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripStatusLabel1.Text = (e.ProgressPercentage.ToString() + "%");
            toolStripProgressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            dt2 = DateTime.Now;
            // MessageBox.Show((dt2 - dt1).TotalMilliseconds + "");
            log4.Add("Processed " + ((float)total / 1024f / 1024f).ToString("F") +" MB of files within "+ (dt2 - dt1).Minutes+" minutes and "+(dt2-dt1).Seconds+" seconds " );
            log4.Add("" + ((float)output / 1024f / 1024f).ToString("F") + " MB of files in the copy ");
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
            // MessageBox.Show("aaa3");
        }
    }
}
