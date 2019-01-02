﻿using HandSchool.Internal;
using HandSchool.Internal.HtmlObject;
using HandSchool.Models;
using HandSchool.Services;
using HandSchool.ViewModels;
using Xamarin.Forms;

namespace HandSchool.JLU.InfoQuery
{
    [Entrance("jlu", "一键教学评价", "一键教学评价，省去麻烦事。", EntranceType.InfoEntrance)]
    [Hotfix("https://raw.githubusercontent.com/yang-er/HandSchool/master/HandSchool/HandSchool/JLU/InfoQuery/teacheval.js.ver", "jlu_teacheval.js")]
    class TeachEvaluate : HotfixController
    {
        protected override void HandlePostReturnValue(string[] ops, ref string ret)
        {
            if (ret == "") ret = "{\"error\":\"null\"}";
            base.HandlePostReturnValue(ops, ref ret);
        }

        public TeachEvaluate()
        {
            HtmlDocument = new Bootstrap
            {
                Children =
                {
                    "<p class=\"mt-3\">本功能可以帮助你完成评教。蓝色代表可以评价，黄色代表需要手动登录网页评价，绿色代表评价完成。</p>".ToRawHtml(),
                    "<table class=\"table\" id=\"evalItemList\">" +
                    "<tr><th>教师</th><th>学院</th>" + 
                    Core.OnPlatform("", "", "<th>教学任务</th>") + 
                    "</tr></table>".ToRawHtml()
                },
                JavaScript =
                {
                    $"var list = []; var i = 0, len = 0; " +
                    $"var uwp = {(Core.RuntimePlatform == "UWP" ? "true" : "false")};",
                    HotfixAttribute.ReadContent(this) ?? "invokeCSharpAction('msg;模块热更新出现问题，请重启应用尝试。')"
                }
            };

            Menu.Add(new InfoEntranceMenu("开始", new Command(() => Evaluate("solve()")), "\uE8B0"));
        }
    }
}
