using System;
using System.Reflection;
using System.Diagnostics;
using System.ServiceProcess;
using System.ComponentModel;
using System.Configuration.Install;

//
// Make reference to c:\windows\system32\hnetcfg.dll
//
using NetFwTypeLib;

namespace Syslogd
{
	/// <summary>
	/// Summary description for ProjectInstaller.
	/// </summary>
	[RunInstaller(true)]
	public class SyslogdInstaller : Installer
	{
		private Properties.Settings settings;
		private ServiceProcessInstaller serviceProcessInstaller1;
		private ServiceInstaller serviceInstaller1;
		/// <summary>
		/// Required designer variable.
		/// </summary>

		public SyslogdInstaller()
		{
			// Get user settings
			settings = new Properties.Settings();

			// This call is required by the Designer.
			InitializeComponent();

			// This call is required is you use network things going through XP firewalls
			FireWall();
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.serviceProcessInstaller1 = new ServiceProcessInstaller();
			this.serviceInstaller1 = new ServiceInstaller();
			// 
			// serviceProcessInstaller1
			// 
			this.serviceProcessInstaller1.Account = ServiceAccount.LocalSystem;
			this.serviceProcessInstaller1.Password = null;
			this.serviceProcessInstaller1.Username = null;
			// 
			// serviceInstaller1
			// 
			this.serviceInstaller1.ServiceName = settings.ServiceName;
			this.serviceInstaller1.Description = settings.Description;
			this.serviceInstaller1.DisplayName = settings.DisplayName;
			this.serviceInstaller1.StartType = ServiceStartMode.Automatic;
			// 
			// ProjectInstaller
			// 
			this.Installers.AddRange(new Installer[] {
								this.serviceProcessInstaller1,
								this.serviceInstaller1});
				}
		#endregion

		#region Firewall
		/// <summary>
		/// Activate current executable for firewall access.
		/// </summary>
		private void FireWall()
		{
			try
			{
				Type NetFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
				INetFwMgr mgr = (INetFwMgr)Activator.CreateInstance(NetFwMgrType);

				Type NetFwAuthorizedApplicationType = Type.GetTypeFromProgID("HNetCfg.FwAuthorizedApplication", false);
				INetFwAuthorizedApplication app = (INetFwAuthorizedApplication)Activator.CreateInstance(NetFwAuthorizedApplicationType);

				app.Name = settings.ServiceName;
				app.Enabled = true;
				app.ProcessImageFileName = Assembly.GetExecutingAssembly().Location;
				app.Scope = NET_FW_SCOPE_.NET_FW_SCOPE_ALL;

				mgr.LocalPolicy.CurrentProfile.AuthorizedApplications.Add(app);
			}
			catch (Exception exception)
			{
				EventLog.WriteEntry("SyslogdInstaller", exception.ToString());
			}
		}
		#endregion

	}
}
