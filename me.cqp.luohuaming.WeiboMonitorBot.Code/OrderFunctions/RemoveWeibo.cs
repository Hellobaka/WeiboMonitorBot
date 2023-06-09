﻿using me.cqp.luohuaming.WeiboMonitorBot.PublicInfos;
using me.cqp.luohuaming.WeiboMonitorBot.Sdk.Cqp.EventArgs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var weiboList = ConfigHelper.GetConfig<List<long>>("Weibos");
            var group = ConfigHelper.GetConfig<JObject>("Monitor");
            if (group.ContainsKey(e.FromGroup))
            {
                var groupArr = group[e.FromGroup].ToObject<List<long>>();
                if (!groupArr.Any(x => x == uid))
                {
                    if (groupArr.Count >= uid)
                    {
                        uid = groupArr[(int)(uid - 1)];
                    }
                    else
                    {
                        sendText.MsgToSend.Add("用户UID或序号格式不正确");
                        return result;
                    }
                }
                group[e.FromGroup].Children().FirstOrDefault(x => x.Value<long>() == uid)?.Remove();
            }
            ConfigHelper.SetConfig("Monitor", group);
            bool existFlag = false;
            foreach (JProperty item in group.Properties())
            {
                if ((item.Value as JArray).Any(x => {
                    var p = (int)x;
                    return p == uid;
                }))
                {
                    existFlag = true;
                }
            }
            if (weiboList.Any(x => x == uid) && !existFlag)
            {
                weiboList.Remove(uid);
                MainSave.UpdateChecker.RemoveWeibo(uid);
                ConfigHelper.SetConfig("Weibos", weiboList);
            }
            sendText.MsgToSend.Add("删除成功");
            return result;
        }

        public FunctionResult Progress(CQPrivateMessageEventArgs e)//私聊处理
        {
            FunctionResult result = new FunctionResult
            {
                Result = false,
                SendFlag = false,
            };
            SendText sendText = new SendText
            {
                SendID = e.FromQQ,
            };

            sendText.MsgToSend.Add("这里输入需要发送的文本");
            result.SendObject.Add(sendText);
            return result;
        }
    }
}
