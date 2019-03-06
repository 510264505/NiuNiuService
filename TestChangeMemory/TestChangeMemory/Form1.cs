using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestChangeMemory
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //血量初始值
        private int value = 1000;
        /// <summary>
        /// 刷新界面：将最新的血量显示在界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            this.label_display.Text = value.ToString();
        }

        /// <summary>
        /// 更新血量：将自定义的数值写入血量变量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            int iVaule = -1;
            bool ParseResult = int.TryParse(this.textBox_value.Text, out iVaule);
            if (ParseResult)
            {
                value = iVaule;
                this.label_display.Text = this.textBox_value.Text;
            }
        }

    }
}
