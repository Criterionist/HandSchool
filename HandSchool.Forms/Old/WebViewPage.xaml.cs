﻿using HandSchool.Internal;
using HandSchool.Services;
using HandSchool.ViewModels;
using System;
using System.Reflection;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using EntAttr = HandSchool.Services.EntranceAttribute;

namespace HandSchool.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class WebViewPage : PopContentPage
	{
        private IWebEntrance InfoEntrance { get; }

		public WebViewPage(IWebEntrance entrance)
		{
			InitializeComponent();
            ShowIsBusyDialog = true;
            var meta = entrance.GetType().GetCustomAttribute(typeof(EntAttr)) as EntAttr;
            Title = meta.Title;

            InfoEntrance = entrance;
            var baseController = InfoEntrance as BaseController;
            ViewModel = baseController;

            if (entrance is IInfoEntrance ie)
            {
                var sb = new StringBuilder();
                ie.HtmlDocument.ToHtml(sb);
                WebView.Html = sb.ToString();
            }
            else if (entrance is IUrlEntrance iu)
            {
                WebView.Uri = iu.HtmlUrl;
                WebView.Cookie = iu.Cookie;
                WebView.OpenWithPost = iu.OpenWithPost;
                WebView.SubUrlRequested += OnSubUrlRequested;

                if (WebView.Uri.Contains("://"))
                {
                    baseController.SetIsBusy(true);
                    WebView.LoadCompleted += () => baseController.SetIsBusy(false);
                }
            }

            foreach (var key in InfoEntrance.Menu)
            {
                ToolbarItems.Add(new ToolbarItem
                {
                    Text = key.Name,
                    Command = key.Command,
                    CommandParameter = (Action<IWebEntrance>)OnEntranceRequested
                });
            }

            entrance.Evaluate = WebView.JavaScript;
            WebView.RegisterAction(entrance.Receive);
        }

        protected virtual void OnSubUrlRequested(string req)
        {
            if (InfoEntrance is IUrlEntrance iu)
            {
                OnEntranceRequested(iu.SubUrlRequested(req));
            }
        }

        protected virtual void OnEntranceRequested(IWebEntrance ent)
        {
            new WebViewPage(ent).ShowAsync(Navigation);
        }
    }
}