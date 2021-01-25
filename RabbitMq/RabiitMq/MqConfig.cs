using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hxh.Tools
{
    public class MqConfig
    {
        //是否重连
        public bool AutomaticRecoveryEnabled { get; set; }

        public TimeSpan NetworkRecoveryInterval { get; set; }
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

    }
}
