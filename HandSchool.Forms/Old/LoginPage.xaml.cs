﻿using HandSchool.Models;
using HandSchool.ViewModels;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HandSchool.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : PopContentPage
    {
        MemoryStream image_mem;
        
        internal LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            ShowIsBusyDialog = true;
            ViewModel = viewModel;
            ShowCancel = true;

#if __IOS__
            Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(this, true);
#endif

            UpdateCaptchaInfomation();
        }

        internal async void Response(object sender, LoginStateEventArgs e)
        {
            switch (e.State)
            {
                case LoginState.Processing:
                    await DisplayAlert("正在登录", "正在登录中，请稍后……", "知道了");
                    break;
                case LoginState.Succeeded:
                    await CloseAsync();
                    break;
                case LoginState.Failed:
                    await DisplayAlert("登录失败", $"登录失败，{e.InnerError}。", "知道了");
                    UpdateCaptchaInfomation();
                    break;
            }
        }

        public async void UpdateCaptchaInfomation()
        {
            var viewModel = ViewModel as LoginViewModel;
            viewModel.SetIsBusy(true, "正在加载资源……");

            if (!await viewModel.Form.PrepareLogin())
            {
                await DisplayAlert("登录失败", "登录失败，出现了一些问题。", "知道了");
            }

            if (viewModel.Form.CaptchaSource == null)
            {
                CaptchaBox.IsVisible = false;
                AutoLoginBox.IsVisible = true;
            }
            else
            {
                CaptchaBox.IsVisible = true;
                AutoLoginBox.IsVisible = false;

                if (image_mem != null)
                    image_mem.Close();
                image_mem = new MemoryStream(viewModel.Form.CaptchaSource, false);
                CaptchaImage.Source = ImageSource.FromStream(() => image_mem);
            }

            viewModel.SetIsBusy(false);
        }
    }
}