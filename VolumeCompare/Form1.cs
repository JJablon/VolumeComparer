using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
        public Form1()
        {

            InitializeComponent();
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

            from.Close();







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
                to.Close();
            }

        }


        public  bool compare (string f, string t)
        {
            return true;


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

        private void button3_Click(object sender, EventArgs e)
        {
            this.Process();
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
                            if(String.Compare(s1,s2) !=0) 
                            listBox1.Items.Add( fi1.FullName+ "             !=             " + fi11.FullName);
                        }
                        else
                        {
                            listBox1.Items.Add("Cant find:" + fi11.FullName);
                        }
                    }
                    //listBox1.Items.Add(fi.FullName + " : " + Md5SumByProcess(fi.FullName));
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








        /////////////////////////////////////////////////////



        private void Process2()
        {
            DirectoryInfo di = new DirectoryInfo(textBox1.Text);
            DirectoryInfo di2 = new DirectoryInfo(textBox2.Text);
            try
            {
                foreach (var fi in di.GetFileSystemInfos("*.*", SearchOption.AllDirectories))
                {
                    string s1 = Md5Sum(fi.FullName);//.ToUpper();
                    string pth = fi.FullName.Replace(textBox1.Text, " ");
                    pth = textBox2.Text + "\\" + pth;
                    DirectoryInfo di11 = new DirectoryInfo(pth);
                    FileInfo fi11 = new FileInfo(pth);
                    if (di11.Exists) { }
                    else
                    {
                        if (fi11.Exists)
                        {
                            string s2 = Md5Sum(fi11.FullName);//.ToUpper();
                            if (String.Compare(s1, s2) != 0)
                                listBox1.Items.Add(fi1.FullName + "             !=             " + fi11.FullName);
                        }
                        else
                        {
                            listBox1.Items.Add("Cant find:" + fi11.FullName);
                        }
                    }
                    //listBox1.Items.Add(fi.FullName + " : " + Md5SumByProcess(fi.FullName));
                }
            }
           // catch (UnauthorizedAccessException)
           // {
           //     MessageBox.Show("Nie można uzyskać dostępu do folderu");
          //  }
            catch (Exception e )
            {
                MessageBox.Show("Inny błąd: "+e.Message+"\n\n"+e.Data);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Process2();
        }
    }
}
