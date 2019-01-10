﻿using HandSchool.Internal;
using HandSchool.Services;
using HandSchool.ViewModels;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace HandSchool.Views
{
    /// <summary>
    /// 提供信息查询页面
    /// </summary>
    public sealed partial class WebViewPage : ViewPage
    {
        /// <summary>
        /// 信息入口点
        /// </summary>
        private IWebEntrance InfoEntrance { get; set; }

        /// <summary>
        /// 加载网页视图。
        /// </summary>
        public WebViewPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 触发脚本
        /// </summary>
        /// <param name="eval">脚本内容</param>
        public async void InvokeScript(string eval)
        {
            try
            {
                await WebView.InvokeScriptAsync("eval", new[] { eval });
            }
            catch (Exception ex)
            {
                this.WriteLog(ex);
            }
        }

        /// <summary>
        /// 显示的HTML字符串
        /// </summary>
        public string Html { get; set; }

        /// <summary>
        /// 导航的目标网址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// invokeCSharpAction的回调函数
        /// </summary>
        public Func<string, Task> Register { get; set; }

        /// <summary>
        /// 请求子网页的事件
        /// </summary>
        public event Action<string> SubUrlRequested;

        /// <summary>
        /// 网页加载完成的事件
        /// </summary>
        public event Action LoadCompleted;

        /// <summary>
        /// 网页浏览器加载完时调用。
        /// </summary>
        /// <param name="sender">网页浏览器</param>
        /// <param name="args">路由事件</param>
        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            if (Html != string.Empty && Html != null)
            {
                WebView.NavigateToString(Html.Replace("{webview_base_url}", "ms-appx-web:///WebWrapper//"));
            }
            else
            {
                WebView.Navigate(new Uri(Url));
                if (Url.Contains("://")) WebView.NavigationStarting += OnWebViewNavigating;
            }
        }

        /// <summary>
        /// 浏览器即将完成导航时开始进行。
        /// </summary>
        /// <param name="sender">网页浏览器</param>
        /// <param name="args">网页浏览器导航完成事件参数</param>
        private async void OnWebViewNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (args.IsSuccess)
            {
                // Inject JS script
                await WebView.InvokeScriptAsync("eval", new[] { "function invokeCSharpAction(data){window.external.notify(data);}" });
            }

            LoadCompleted?.Invoke();
        }

        /// <summary>
        /// 网页浏览器的外部通知。
        /// </summary>
        /// <param name="sender">浏览器</param>
        /// <param name="e">通知内容</param>
        private void OnWebViewScriptNotify(object sender, NotifyEventArgs e)
        {
            Register?.Invoke(e.Value);
        }

        /// <summary>
        /// 浏览器即将开始导航时开始进行。
        /// </summary>
        /// <param name="sender">网页浏览器</param>
        /// <param name="args">网页浏览器导航开始事件参数</param>
        private void OnWebViewNavigating(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (args.Uri?.OriginalString != Url)
            {
                args.Cancel = true;
                SubUrlRequested?.Invoke(args.Uri.OriginalString);
            }
        }

        /// <summary>
        /// 当导航到此页面时，注入需要的参数。
        /// </summary>
        /// <param name="e">导航事件参数</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            System.Diagnostics.Debug.Assert(e.Parameter is BaseController, "Error leading");
            var baseController = e.Parameter as BaseController;
            InfoEntrance = baseController;

            var meta = InfoEntrance.GetType().Get<EntranceAttribute>();
            ViewModel = e.Parameter as BaseController;
            ViewModel.Title = meta.Title;
            InfoEntrance.Evaluate = InvokeScript;

            Register = InfoEntrance.Receive;
            foreach (var key in InfoEntrance.Menu)
            {
                PrimaryMenu.Add(new AppBarButton
                {
                    Label = key.Name,
                    Command = key.Command,
                    CommandParameter = (Action<IWebEntrance>)OnEntranceRequested,
                    Icon = new FontIcon
                    {
                        FontFamily = new FontFamily("Segoe MDL2 Assets"),
                        Glyph = key.Icon
                    }
                });
            }

            if (InfoEntrance is IInfoEntrance info)
            {
                var sb = new StringBuilder();
                info.HtmlDocument.ToHtml(sb);
                Html = sb.ToString();
            }
            else if (InfoEntrance is IUrlEntrance urle)
            {
                Url = urle.HtmlUrl;
                SubUrlRequested += OnSubUrlRequested;

                if (Url.Contains("://"))
                {
                    ViewModel.SetIsBusy(true);
                    LoadCompleted += () => ViewModel.SetIsBusy(false);
                }
            }
        }

        /// <summary>
        /// 当子链接被请求时启动。
        /// </summary>
        /// <param name="url">子链接</param>
        private void OnSubUrlRequested(string url)
        {
            if (InfoEntrance is IUrlEntrance iu)
            {
                OnEntranceRequested(iu.SubUrlRequested(url));
            }
        }

        /// <summary>
        /// 需要打开一个新的入口点时处理。
        /// </summary>
        /// <param name="entrance">入口点对象</param>
        private void OnEntranceRequested(IWebEntrance entrance)
        {
            Frame.Navigate(typeof(WebViewPage), entrance);
        }
    }
}
