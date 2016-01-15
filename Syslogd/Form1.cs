using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Diagnostics;

namespace SyslogdTest
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			syslogd = new Syslogd.Syslogd();
		}

		private Syslogd.Syslogd syslogd;

		private void Form1_Load(object sender, EventArgs e)
		{
			Start();
		}

		private void Start()
		{
			if (syslogd != null)
				syslogd.Start();
			this.button1.Enabled = false;
			this.button2.Enabled = true;
		}

		private void Stop()
		{
			if (syslogd != null)
				syslogd.Stop();
			this.button1.Enabled = true;
			this.button2.Enabled = false;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Start();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			Stop();
		}
	}
}