using System;
using System.Collections.Generic;
using System.Text;

namespace ElevatorActions
{
    public class ActionBaseTx
    {
        public virtual string ActionName { get; set; }
    }

    public class ActionBaseRx
    {
        /// <summary>
        /// 机器人状态
        /// </summary>
        public string RRRR_STATUS { get; set; }
        /// <summary>
        /// 机器人SN
        /// </summary>
        public string RobotSN { get; set; }
        /// <summary>
        /// 指令名称
        /// </summary>
        public virtual string ActionName { get; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorInfo { get; set; }
    }
}
