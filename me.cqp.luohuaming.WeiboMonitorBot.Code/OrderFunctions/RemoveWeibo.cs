using me.cqp.luohuaming.WeiboMonitorBot.PublicInfos;
using me.cqp.luohuaming.WeiboMonitorBot.Sdk.Cqp.EventArgs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WeiboMonitor;

namespace me.cqp.luohuaming.WeiboMonitorBot.Code.OrderFunctions
{
    public class RemoveWeibo : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;

        public string GetOrderStr() => "#移除微博";

        public bool Judge(string destStr) => destStr.Replace("＃", "#").StartsWith(GetOrderStr());//这里判断是否能触发指令

        public FunctionResult Progress(CQGroupMessageEventArgs e)//群聊处理
        {
            FunctionResult result = new FunctionResult
            {
                Result = true,
                SendFlag = true,
            };
            SendText sendText = new SendText
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
                sendText.MsgToSend.Add("用户UID或序号格式不正确");
                return result;
            }
            var weiboList = AppConfig.Weibos;
            var group = AppConfig.Monitor;
            var groupItem = group.FirstOrDefault(x => x.GroupId == e.FromGroup);
            if (groupItem != null)
            {
                if (!groupItem.TargetId.Contains(uid))
                {
                    if (groupItem.TargetId.Count >= uid)
                    {
                        uid = groupItem.TargetId[(int)(uid - 1)];
                    }
                    else
                    {
                        sendText.MsgToSend.Add("用户UID或序号格式不正确");
                        return result;
                    }
                }
                groupItem.TargetId.Remove(uid);
            }
            AppConfig.Instance.SetConfig("Monitor", group);
            bool existFlag = false;
            foreach (var item in group)
            {
                if (item.TargetId.Contains(uid))
                {
                    existFlag = true;
                    break;
                }
            }
            if (weiboList.Any(x => x == uid) && !existFlag)
            {
                weiboList.Remove(uid);
                UpdateChecker.Instance.RemoveWeibo(uid);
                AppConfig.Instance.SetConfig("Weibos", weiboList);
            }
            sendText.MsgToSend.Add("删除成功");
            return result;
        }

        public FunctionResult Progress(CQPrivateMessageEventArgs e)//私聊处理
        {
            FunctionResult result = new()
            {
                Result = false,
                SendFlag = false,
            };
            return result;
        }
    }
}
