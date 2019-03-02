using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorActions
{
    /// <summary>
    /// 获取服务时间上送报文
    /// </summary>
    public class ActionGetServerTimeTx : ActionBaseTx
    {
        /// <summary>
        /// 获取服务时间动作名
        /// </summary>
        public override string ActionName { get => "ServerTime"; }
    }

    public class ActionGetServerTimeRx : ActionBaseRx
    {
        /// <summary>
        /// 获取服务时间动作名
        /// </summary>
        public override string ActionName { get => "ServerTime_Back"; }
        public string ServerTime { get => DateTime.Now.ToString(); }
    }
}
