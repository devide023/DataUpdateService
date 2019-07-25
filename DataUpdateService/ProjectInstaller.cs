using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DataUpdateService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        private Process p = new Process();
        public ProjectInstaller()
        {
            InitializeComponent();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
        }

        private void serviceProcessInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {

        }

        private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {
            string Cmdstring = "sc start dataupdate"; //CMD命令
            p.StandardInput.WriteLine(Cmdstring);
            p.StandardInput.WriteLine("exit");
        }

        private void serviceInstaller1_BeforeUninstall(object sender, InstallEventArgs e)
        {
            string Cmdstring = "sc stop dataupdate"; //CMD命令
            p.StandardInput.WriteLine(Cmdstring);
            p.StandardInput.WriteLine("exit");
        }
    }
}
