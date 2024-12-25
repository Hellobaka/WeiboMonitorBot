using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeiboMonitor_netframework;

namespace me.cqp.luohuaming.WeiboMonitorBot.PublicInfos
{
    public class AppConfig : ConfigBase
    {
        public AppConfig(string path)
            : base(path)
        {
            LoadConfig();
            Instance = this;
        }

        public static AppConfig Instance { get; private set; }

        public static List<long> Weibos { get; private set; } = [];

        public static List<MonitorItem> Monitor { get; private set; } = [];

        public override void LoadConfig()
        {
            Weibos = GetConfig("Weibos", new List<long>());
            Monitor = GetConfig("Monitor", new List<MonitorItem>());

            Config.CustomFont = GetConfig("CustomFont", "Microsoft YaHei");
            Config.CustomFontPath = GetConfig("CustomFontPath", "");
            Config.RefreshInterval = GetConfig("RefreshInterval", 120 * 1000);
            Config.RetryCount = GetConfig("RetryCount", 3);
            Config.DebugMode = GetConfig("DebugMode", false);
            Config.DynamicFilters = GetConfig("DynamicFilters", new List<string>() { });
        }
    }
}