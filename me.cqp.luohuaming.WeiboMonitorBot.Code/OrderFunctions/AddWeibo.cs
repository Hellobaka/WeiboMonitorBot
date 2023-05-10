using me.cqp.luohuaming.WeiboMonitorBot.PublicInfos;
using me.cqp.luohuaming.WeiboMonitorBot.Sdk.Cqp.EventArgs;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using WeiboMonitor.API;

namespace me.cqp.luohuaming.WeiboMonitorBot.Code.OrderFunctions
{
    public class AddWeibo : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;

        public string GetOrderStr() => "#添加微博";

        public bool Judge(string destStr) => destStr.Replace("＃", "#").StartsWith(GetOrderStr());//这里判断是否能触发指令

        public FunctionResult Progress(CQGroupMessageEventArgs e)//群聊处理
        {
            FunctionResult result = new()
            {
                Result = true,
                SendFlag = true,
            };
            SendText sendText = new()
            {
                SendID = e.FromGroup,
            };
            result.SendObject.Add(sendText);
            var args = e.Message.Text.Replace(GetOrderStr(), "");
            if (string.IsNullOrEmpty(args))
            {
                sendText.MsgToSend.Add("请填写用户UID");
                return result;
            }
            if (!long.TryParse(args, out long uid))
            {
                sendText.MsgToSend.Add("用户UID格式不正确");
                return result;
            }
            var weiboList = ConfigHelper.GetConfig<List<long>>("Weibos");
            var group = ConfigHelper.GetConfig<JObject>("Monitor");
            GetTimeLine weibo = null;
            if (!weiboList.Any(x => x == uid))
            {
                weiboList.Add(uid);
                weibo = MainSave.UpdateChecker.AddWeibo(uid);
            }
            if (group.ContainsKey(e.FromGroup))
            {
                if (group[e.FromGroup].Contains(uid) && weiboList.Any(x => x == e.FromGroup))
                {
                    sendText.MsgToSend.Add("重复添加");
                    return result;
                }
                (group[e.FromGroup] as JArray).Add(uid);
            }
            else
            {
                group.Add(new JProperty(e.FromGroup, new JArray(uid)));
            }
            if (weibo != null)
            {
                ConfigHelper.SetConfig("Weibos", weiboList);
                ConfigHelper.SetConfig("Monitor", group);

                sendText.MsgToSend.Add($"{weibo.UserName} 添加微博检查成功");
            }
            else
            {
                sendText.MsgToSend.Add("添加失败，可能用户不存在或Cookie失效");
            }

            return result;
        }

        public FunctionResult Progress(CQPrivateMessageEventArgs e)//私聊处理
        {
            FunctionResult result = new()
            {
                Result = false,
                SendFlag = false,
            };
            SendText sendText = new()
            {
                SendID = e.FromQQ,
            };

            sendText.MsgToSend.Add("这里输入需要发送的文本");
            result.SendObject.Add(sendText);
            return result;
        }
    }
}
