﻿using ControllerApp.ViewModel;
using FantasmicCommon.Utils.BTClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace ControllerApp
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        BTClient btClient;
        MainPageViewModel viewModel;
        public MainPage()
        {
            this.InitializeComponent();
            btClient = new BTClient(this);
            btClient.InitializeCompleted += Sender_InitializeCompleted;
            btClient.MessageRecieved += BtClient_MessageRecieved;
            viewModel = new MainPageViewModel();
            viewModel.DeviceInfoCollection = btClient.DeviceInfoCollection;
        }

        private void BtClient_MessageRecieved(object sender, EventArgs e)
        {
            viewModel.MainMessage = (e as BTMessageRecievedEventArgs).RecievedMessage;
        }

        private void Sender_InitializeCompleted(object sender, EventArgs e)
        {
            viewModel.StatusMessage = (e as BTInitEventArgs).ConnectionHostName;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.DataContext = viewModel;
            btClient.Initialize();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            //viewModel.MainMessage = "count: " + viewModel.DeviceInfoCollection.Count();
           // btClient.ConnectReciever(DeviceListBox.SelectedItem as DeviceInformation);
            //this.btClient.StopDeviceWatcher();//クライアント側は２つ以上のデバイスと接続する必要ない
        }
    }
}
