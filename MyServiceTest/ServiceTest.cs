using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MyServiceTest
{
    public partial class ServiceTest : ServiceBase
    {
        RTX rtx = new RTX();
        public ServiceTest()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //启动服务
            rtx.startService();
        }

        protected override void OnStop()
        {
            rtx.stopService();
        }
    }
}
