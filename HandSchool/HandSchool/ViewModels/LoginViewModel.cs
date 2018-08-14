﻿using HandSchool.Internal;
using HandSchool.Models;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

#if __UWP__
using LoginPage = HandSchool.Views.LoginDialog;
#else
using LoginPage = HandSchool.Views.LoginPage;
#endif

namespace HandSchool.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public Command LoginCommand { get; set; }
        public ILoginField Form { get; }
        public LoginPage Page { get; set; }
        
        private LoginViewModel(ILoginField form)
        {
            Form = form;
        }

        public static async Task<bool> RequestAsync(ILoginField form)
        {
            var viewModel = new LoginViewModel(form);
            viewModel.LoginCommand = new Command(viewModel.Login);
            viewModel.View = new ViewResponse(null);
            viewModel.Page = new LoginPage(viewModel);
            await viewModel.Page.ShowAsync();
            return form.IsLogin;
        }
        
        async void Login()
        {
            if (IsBusy)
            {
                Page.Response(this, new LoginStateEventArgs(LoginState.Processing));
                return;
            }

            IsBusy = true;

            View.SetIsBusy(true, "正在登录……");
            Form.LoginStateChanged += Page.Response;

            try
            {
                await Form.Login();
            }
            finally
            {
                IsBusy = false;
                View.SetIsBusy(false);
                Form.LoginStateChanged -= Page.Response;
            }
        }
    }
}
