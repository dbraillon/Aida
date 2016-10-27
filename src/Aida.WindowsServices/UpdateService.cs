using Aida.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Aida.WindowsServices
{
    public partial class UpdateService : ServiceBase
    {
        public UpdateService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Voice v = new Voice(CultureInfo.CurrentCulture);
            v.Say("Hello! I'm glad to see you again!");
        }

        protected override void OnStop()
        {
        }
    }
}
