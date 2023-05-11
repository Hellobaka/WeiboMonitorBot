using me.cqp.luohuaming.WeiboMonitorBot.PublicInfos;
using me.cqp.luohuaming.WeiboMonitorBot.Sdk.Cqp.EventArgs;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text;

namespace me.cqp.luohuaming.WeiboMonitorBot.Code.OrderFunctions
{
    public class WeiboList : IOrderModel
    {
        public bool ImplementFlag { get; set; } = true;

        public string GetOrderStr()
        {
            return "#微博列表";
        }

        public bool Judge(string destStr)
        {
            return destStr.Replace("＃", "#").StartsWith(GetOrderStr());//这里判断是否能触发指令
        }

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
            StringBuilder sb = new();
            sb.AppendLine("微博列表");
            int index = 1;
            foreach (var item in MainSave.UpdateChecker.GetBangumiList())
            {
                var group = ConfigHelper.GetConfig<JObject>("Monitor");
                if (group.ContainsKey(e.FromGroup) && group[e.FromGroup].Any(x => x.Value<long>() == item.Item1))
                {
                    sb.AppendLine($"{index}. {item.Item2} - {item.Item1}");
                    index++;
                }
            }
            sendText.MsgToSend.Add(sb.ToString());
            result.SendObject.Add(sendText);
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
