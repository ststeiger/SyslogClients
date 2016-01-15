using System;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;

namespace Syslogd
{
	public class SyslogdService : ServiceBase
	{
		private Syslogd syslogd;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private Container components = null;

		public SyslogdService()
		{
			// This call is required by the Windows.Forms Component Designer.
			InitializeComponent();
			
			syslogd = new Syslogd();
		}

		// The main entry point for the process
		static void Main()
		{
			ServiceBase[] ServicesToRun;
	
			ServicesToRun = new ServiceBase[] { new SyslogdService() };

			ServiceBase.Run(ServicesToRun);	
		}

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// Service1
			//
			Properties.Settings settings = new Properties.Settings();
			this.ServiceName = settings.ServiceName;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Start this service.
		/// </summary>
		protected override void OnStart(string[] args)
		{
			syslogd.Start();
		}
 
		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
			syslogd.Stop();
		}
	}
}
