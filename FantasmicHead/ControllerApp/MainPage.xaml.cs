using ControllerApp.ViewModel;
using FantasmicCommon.Utils.BTClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
        BTSender sender;
        MainPageViewModel viewModel;
        public MainPage()
        {
            this.InitializeComponent();
            sender = new BTSender();
            sender.InitializeCompleted += Sender_InitializeCompleted;
            viewModel = new MainPageViewModel();
        }

        private void Sender_InitializeCompleted(object sender, EventArgs e)
        {
            viewModel.MainMessage = "Initialized" + (e as BTInitEventArgs).ConnectionHostName;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            StatusTextBox.DataContext = viewModel;
            Task.Run(() => sender.Initialize());
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
