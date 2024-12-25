using me.cqp.luohuaming.WeiboMonitorBot.PublicInfos;
using me.cqp.luohuaming.WeiboMonitorBot.Sdk.Cqp;
using me.cqp.luohuaming.WeiboMonitorBot.Sdk.Cqp.EventArgs;
using me.cqp.luohuaming.WeiboMonitorBot.Sdk.Cqp.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using WeiboMonitor;
using WeiboMonitor.API;
using WeiboMonitor_netframework;

namespace me.cqp.luohuaming.WeiboMonitorBot.Code
{
    public class Event_StartUp : ICQStartup
    {
        public void CQStartup(object sender, CQStartupEventArgs e)
        {
            MainSave.AppDirectory = e.CQApi.AppDirectory;
            MainSave.CQApi = e.CQApi;
            MainSave.CQLog = e.CQLog;
            MainSave.ImageDirectory = PublicInfos.CommonHelper.GetAppImageDirectory();
            AppConfig appConfig = new(Path.Combine(MainSave.AppDirectory, "Config.json"));
            appConfig.LoadConfig();
            appConfig.EnableAutoReload();
            foreach (var item in Assembly.GetAssembly(typeof(Event_GroupMessage)).GetTypes())
            {
                if (item.IsInterface)
                    continue;
                foreach (var instance in item.GetInterfaces())
                {
                    if (instance == typeof(IOrderModel))
                    {
                        IOrderModel obj = (IOrderModel)Activator.CreateInstance(item);
                        if (obj.ImplementFlag == false)
                            break;
                        MainSave.Instances.Add(obj);
                    }
                }
            }
            if (!Directory.Exists(Path.Combine(MainSave.AppDirectory, "Assets")))
            {
                MainSave.CQLog.Warning("资源文件不存在，请放置文件后重载插件");
                return;
            }
            Config.BaseDirectory = MainSave.AppDirectory;
            Config.PicSaveBasePath = MainSave.ImageDirectory;
            LogHelper.InfoMethod = (type, message, status) =>
            {
                if (!status)
                {
                    MainSave.CQLog.Warning(type, message);
                }
                else
                {
                    MainSave.CQLog.Debug(type, message);
                }
            };
            new Thread(() =>
            {
                GetTimeLine.OnTimeLineUpdate += Update_OnTimeLineUpdate;

                foreach (var item in AppConfig.Weibos)
                {
                    GetTimeLine.AddWeibo(item);
                }
                MainSave.CQLog.Info("载入成功", $"监视了 {AppConfig.Weibos.Count} 个微博");
            }).Start();
        }

        private void Update_OnTimeLineUpdate(WeiboMonitor.Model.TimeLine_Object timeLine, long uid, string pic)
        {
            var group = AppConfig.Monitor;
            foreach (var id in group)
            {
                if (id.TargetId.Any(x => x == uid))
                {
                    StringBuilder sb = new();
                    sb.Append($"{timeLine.user.screen_name} 更新了微博, https://weibo.com/{uid}/{timeLine.idstr}");
                    if (string.IsNullOrEmpty(pic) is false)
                        sb.Append(CQApi.CQCode_Image(pic));
                    MainSave.CQApi.SendGroupMessage(Convert.ToInt64(id), sb.ToString());
                }
            }
        }
    }
}
