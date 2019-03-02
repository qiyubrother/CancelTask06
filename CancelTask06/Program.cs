using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using ElevatorActions;

namespace CancelTask06
{
    class Program
    {
        static void Main(string[] args)
        {
            var configFileName = "RobotSetting.json";
            if (args.Length == 0)
            {
                configFileName = "RobotSetting.json";
            }
            else
            {
                configFileName = args[0];
            }

            try
            {
                var webSocket = new ClientWebSocket();
                #region 变量定义
                var j = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(configFileName));
                var userName = j["CompanyTag"].ToString();
                var curTime = DateTime.Now.ToFileTimeUtc().ToString();
                var robotSn = j["RobotSN"].ToString();
                var robotMac = j["RobotMac"].ToString();
                var nonce = j["Nonce"].ToString();
                var fromFloorNo = j["FromFloorNo"].ToString();
                var toFloorNo = j["ToFloorNo"].ToString();
                var moduleName = j["ModuleName"].ToString();
                var robotInElevatorSecond = Convert.ToInt32(j["RobotInElevatorSecond"].ToString()) * 1000;
                var robotOutElevatorSecond = Convert.ToInt32(j["RobotOutElevatorSecond"].ToString()) * 1000;
                var robotHeartBeatSecond = Convert.ToInt32(j["RobotHeartBeatSecond"].ToString()) * 1000;
                // 延迟关门时间（秒）
                var elevatorOpenDoorSecond = Convert.ToInt32(j["ElevatorOpenDoorSecond"].ToString()) * 1000;
                Console.WriteLine($"robotSn:{robotSn}");
                var taskEndExit = Convert.ToBoolean(j["TaskEndExit"].ToString());
                var checkSum = CodeHelper.GetSignature(userName, curTime, robotSn, robotMac, nonce);
                //var timeout = Convert.ToInt32(j["TaskTimeoutSecond"].ToString()) * 1000;
                var timeout = 60000; // 最长等待1分钟
                var url = $"{j["Url"].ToString()}?username={userName}&curtime={curTime}&robotsn={robotSn}&robotmac={robotMac}&nonce={nonce}&checksum={checkSum}";
                #endregion

                DebugHelper.PrintTraceMessage($"[机器人][参数]超时接收时间(timeout)::{timeout}");

                #region 机器人连接到云端服务器
                {
                    #region 发送【连接到云端服务器】请求
                    var buffer = new byte[512];
                    DebugHelper.PrintTxMessage(url);
                    do
                    {
                        try
                        {
                            DebugHelper.PrintTraceMessage($"[机器人]请求连接到服务程序...");

                            if (webSocket.State == WebSocketState.Closed)
                            {
                                webSocket.Dispose();
                                webSocket = new ClientWebSocket();
                            }
                            webSocket.ConnectAsync(new Uri(url), CancellationToken.None).Wait();
                            break;
                        }
                        catch (Exception ex)
                        {
                            //Console.WriteLine($"连接到服务器程序失败，等待30秒后尝试重新连接...");
                            DebugHelper.PrintErrorMessage($"[Exception]{ex.Message}");
                            System.Threading.Thread.Sleep(timeout);
                        }
                    } while (true);
                    #endregion
                    #region 接收返回结果
                    var rst = webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    rst.Wait();
                    if (rst.Result.MessageType == WebSocketMessageType.Text)
                    {
                        string msg = Encoding.UTF8.GetString(buffer, 0, rst.Result.Count);
                        DebugHelper.PrintRxMessage(msg);
                    }
                    else
                    {
                        // 错误的消息类型
                        DebugHelper.PrintErrorMessage("[机器人][连接到服务器]错误的消息类型...");
                        return;
                    }
                    #endregion
                }
                #endregion
                #region 获取服务器时间
                {
                    #region 变量定义
                    var buffer = new byte[512];
                    #endregion
                    #region 发送【获取服务器时间】请求
                    DebugHelper.PrintTraceMessage($"[机器人]获取服务器时间[SendAsync]...");
                    var tx = JsonConvert.SerializeObject(new ActionGetServerTimeTx());
                    DebugHelper.PrintTxMessage(tx);
                    var s = webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(tx)), WebSocketMessageType.Text, true, CancellationToken.None);
                    #endregion
                    #region 接收返回结果
                    DebugHelper.PrintTraceMessage($"[机器人]获取服务器时间[ReceiveAsync][S]...");
                    var rst = webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (rst.Wait(timeout))
                    {
                        DebugHelper.PrintTraceMessage($"[机器人]获取服务器时间[ReceiveAsync][E]...");
                        if (rst.Result.MessageType == WebSocketMessageType.Text)
                        {
                            string msg = Encoding.UTF8.GetString(buffer, 0, rst.Result.Count);
                            DebugHelper.PrintRxMessage(msg);
                        }
                        else
                        {
                            // 错误的消息类型
                            DebugHelper.PrintErrorMessage($"[机器人][获取服务器时间]错误的消息类型...");
                            return;
                        }
                    }
                    else
                    {
                        DebugHelper.PrintTraceMessage($"[机器人]获取服务器时间[ReceiveAsync][E]...");
                        DebugHelper.PrintErrorMessage($"[机器人][获取服务器时间]超时，忽略...");
                    }
                    #endregion
                }
                #endregion
                #region 发送心跳包
                SendHeartbeat(webSocket);
                #endregion
                #region 取消所有任务
                {
                    var buffer = new byte[512];
                    #region 发送【取消所有任务】请求
                    DebugHelper.PrintTraceMessage($"[机器人]取消所有任务[SendAsync]...");
                    var tx = JsonConvert.SerializeObject(new ActionCancelTaskTx());
                    DebugHelper.PrintTxMessage(tx);
                    var s = webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(tx)), WebSocketMessageType.Text, true, CancellationToken.None);
                    #endregion
                    #region 接收返回结果
                    DebugHelper.PrintTraceMessage($"[机器人]等待接收取消任务的回文（最长{timeout / 1000}秒）...");
                    var rst = webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (rst.Wait(10000)) // 最长等待10秒
                    {
                        DebugHelper.PrintTraceMessage($"[机器人]取消所有任务[ReceiveAsync][E]...");
                        if (rst.Result.MessageType == WebSocketMessageType.Text)
                        {
                            string msg = Encoding.UTF8.GetString(buffer, 0, rst.Result.Count);
                            DebugHelper.PrintRxMessage(msg);
                        }
                        else
                        {
                            // 错误的消息类型
                            DebugHelper.PrintErrorMessage($"[机器人][{rst.Result.MessageType}][取消所有任务]错误的消息类型...");
                            return;
                        }
                    }
                    else
                    {
                        DebugHelper.PrintErrorMessage($"[机器人][取消所有任务]等待超时，放弃等待...");
                    }
                    #endregion
                    return;
                }
                #endregion
            }

            catch (Exception ex)
            {
                DebugHelper.PrintErrorMessage($"[机器人][异常]{ex.Message}");
            }
            finally
            {
                DebugHelper.PrintTraceMessage($"按任意键推出...");
                Console.ReadKey();
            }

        }

        static void SendHeartbeat(ClientWebSocket webSocket)
        {
            var jo = new JObject();
            jo["ActionName"] = "Heartbeat";

            string msg = jo.ToString();
            DebugHelper.PrintTxMessage(msg);
            DebugHelper.PrintTraceMessage($"[机器人]异步发送心跳包...");
            webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)), WebSocketMessageType.Text, true, CancellationToken.None);

            var buffer = new byte[512];
            DebugHelper.PrintTraceMessage($"[机器人][等待心跳包回文][S]...");
            var rst = webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            DebugHelper.PrintTraceMessage($"[机器人][等待心跳包回文][E]...");
            if (!rst.Wait(5000))
            {
                DebugHelper.PrintTraceMessage($"[机器人][等待心跳包回文]超时，放弃...");
                return;
            }
            else
            {
                if (rst.Result.MessageType == WebSocketMessageType.Text)
                {
                    string rxMsg = Encoding.UTF8.GetString(buffer, 0, rst.Result.Count);
                    DebugHelper.PrintRxMessage(rxMsg);
                }
                else
                {
                    // 错误的消息类型
                    DebugHelper.PrintErrorMessage($"[{DateTime.Now}][机器人][等待电梯到达指令][目标楼层]错误的消息类型...");
                    return;
                }
            }
        }
    }
}
