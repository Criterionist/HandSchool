﻿using Foundation;
using HandSchool.iOS;
using HandSchool.Views;
using System;
using System.IO;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(HybridWebView), typeof(HybridWebViewRenderer))]
namespace HandSchool.iOS
{
    public class HybridWebViewRenderer : ViewRenderer<HybridWebView, WKWebView>, IWKScriptMessageHandler
    {
        const string JavaScriptFunction = "function invokeCSharpAction(data){window.webkit.messageHandlers.invokeAction.postMessage(data);}";
        const string DomainPattern = @"(http|https)://(?<domain>[^(:|/]*)";
        WKUserContentController userController;
        WKHttpCookieStore cookieStore;

        protected override void OnElementChanged(ElementChangedEventArgs<HybridWebView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                userController = new WKUserContentController();
                var script = new WKUserScript(new NSString(JavaScriptFunction), WKUserScriptInjectionTime.AtDocumentEnd, false);
                userController.AddUserScript(script);
                userController.AddScriptMessageHandler(this, "invokeAction");

                var config = new WKWebViewConfiguration
                {
                    UserContentController = userController,
                    IgnoresViewportScaleLimits = false
                };

                cookieStore = config.WebsiteDataStore.HttpCookieStore;
                var webView = new WKWebView(Frame, config);
                SetNativeControl(webView);
            }

            if (e.OldElement != null)
            {
                userController.RemoveAllUserScripts();
                userController.RemoveScriptMessageHandler("invokeAction");
                var hybridWebView = e.OldElement as HybridWebView;
                hybridWebView.Cleanup();
            }

            if (e.NewElement != null)
            {
                if (Element.Html == "" || Element.Html is null)
                {
                    if (Element.Uri.Contains("://"))
                    {
                        string fileName = Element.Uri;
                        Control.WeakNavigationDelegate = new NavigationDelegate(e.NewElement);
                        Control.LoadRequest(new NSUrlRequest(new NSUrl(fileName)));
                    }
                    else
                    {
                        string fileName = Path.Combine(NSBundle.MainBundle.BundlePath, "WebWrapper", Element.Uri);
                        Control.LoadRequest(new NSUrlRequest(new NSUrl(fileName, false)));
                    }
                }
                else
                {
                    string fileName = Path.Combine(NSBundle.MainBundle.BundlePath, "WebWrapper");
                    Control.LoadHtmlString(Element.Html, new NSUrl(fileName, true));
                }

                Element.JavaScriptRequested += (eval) => Control.EvaluateJavaScript(eval, null);
            }
        }

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            Element.InvokeAction((NSString)message.Body);
        }
    }

    public class NavigationDelegate : WKNavigationDelegate
    {
        WeakReference<HybridWebView> inner;

        public override void DidFailProvisionalNavigation(WKWebView webView, WKNavigation navigation, NSError error)
        {
            if (inner.TryGetTarget(out var target))
            {
                target.NotifyLoadComplete();
            }
        }

        public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            if (inner.TryGetTarget(out var target))
            {
                if (webView.IsLoading == false) target.NotifyLoadComplete();
            }
        }

        public override void DidFailNavigation(WKWebView webView, WKNavigation navigation, NSError error)
        {
            if (inner.TryGetTarget(out var target))
            {
                target.NotifyLoadComplete();
            }
        }

        public NavigationDelegate(HybridWebView target)
        {
            inner = new WeakReference<HybridWebView>(target);
        }

        public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
        {
            if (inner.TryGetTarget(out var target))
            {
                var nav2 = navigationAction.Request.Url.AbsoluteString;

                if (navigationAction.NavigationType != WKNavigationType.LinkActivated)
                {
                    decisionHandler(WKNavigationActionPolicy.Allow);
                }
                else
                {
                    decisionHandler(WKNavigationActionPolicy.Cancel);
                    target.RaiseSubUrlRequest(nav2);
                }
            }
            else
            {
                decisionHandler(WKNavigationActionPolicy.Allow);
            }
        }
    }
}
