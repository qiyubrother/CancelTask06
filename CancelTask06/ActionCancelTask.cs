using System;
using System.Collections.Generic;
using System.Text;


namespace ElevatorActions
{
    /// <summary>
    /// 取消任务上送报文
    /// </summary>
    public class ActionCancelTaskTx : ActionBaseTx
    {
        /// <summary>
        /// 取消任务动作名
        /// </summary>
        public override string ActionName { get => "CancelTask"; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public string Timestamp => ((long)(DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1))).TotalSeconds).ToString();  // 相差秒数
    }

    public class ActionCancelTaskRx : ActionBaseRx
    {
        /// <summary>
        /// 取消任务动作名
        /// </summary>
        public override string ActionName { get => "CancelTask_Back"; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public string Timestamp => ((long)(DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1))).TotalSeconds).ToString();  // 相差秒数
    }
}
