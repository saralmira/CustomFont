using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace CustomFont
{
    public partial class main : Form
    {
        public main()
        {
            InitializeComponent();
            loadfromtxt(Properties.Settings.Default.fontdatapath);
            cprbox.Text = Properties.Settings.Default.countperrow.ToString();
            startCodeBox.Text = Convert.ToString(Properties.Settings.Default.startcode, 16);
            checkBox1.Checked = Properties.Settings.Default.autoaddtofont;
        }

        private void cprbox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                uint cpr = Convert.ToUInt32(cprbox.Text);
                if (cpr > 0)
                {
                    Properties.Settings.Default.countperrow = cpr;
                }
            }
            catch { }
        }

        private void inputbox_TextChanged(object sender, EventArgs e)
        {
            this.SuspendLayout();
            this.CustomFont.InputString = inputbox.Text;
            inputcodebox.Text = this.CustomFont.FormatBytes(this.CustomFont.InputBytes, sepcharbox.Text);
            outputcodebox.Text = this.CustomFont.FormatBytes(this.CustomFont.OutputBytes, sepcharbox2.Text);
            outputbox.Text = this.CustomFont.FormatCustomFont(Properties.Settings.Default.countperrow);
            this.ResumeLayout();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.autoaddtofont = checkBox1.Checked;
            this.CustomFont.AutoAddToCustomFont = checkBox1.Checked;
        }

        private void main_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.CustomFont.SaveCustomFont();
            Properties.Settings.Default.Save();
        }

        private void loadfromtxt(string path)
        {
            if (!File.Exists(path))
                return;
            Properties.Settings.Default.fontdatapath = path;
            CustomFont = new CustomFont(path);
        }

        private void mloadfromtxt_Click(object sender, EventArgs e)
        {
            if (txtFileDialog.ShowDialog() == DialogResult.OK)
            {
                loadfromtxt(txtFileDialog.FileName);
            }
        }

        private CustomFont CustomFont;

        private void button1_Click(object sender, EventArgs e)
        {
            this.CustomFont.Clear();
            outputbox.Text = this.CustomFont.FormatCustomFont(Properties.Settings.Default.countperrow);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                UInt16 c = Convert.ToUInt16(startCodeBox.Text, 16);
                if (c > 0)
                {
                    Properties.Settings.Default.startcode = c;
                    this.CustomFont.StartCode = c;
                }
            }
            catch { }
        }
    }
}
