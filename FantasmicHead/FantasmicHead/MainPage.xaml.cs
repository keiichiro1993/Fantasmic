using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.System.Threading;
using Windows.UI.Core;
using FantasmicCommon.Models;
using FantasmicCommon.Utils;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace FantasmicHead
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Scene currentState;
        private SerialUtil serial;
        private BTServer btServer;

        public MainPage()
        {
            this.InitializeComponent();
            btServer = new BTServer();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //フルスクリーンに
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TryEnterFullScreenMode();

            //Bluetooth の準備
            await btServer.InitializeRfcommServer();

            //シリアルとシーン切り替えの準備
            currentState = new Scene(Scene.Scenes.Arabian, 3);
            serial = new SerialUtil();
            await serial.InitSerial();

            //mediaPlayerElement.MediaPlayer.Play();

            await serial.SendData(new Scene(Scene.Scenes.Arabian, 7));
            List<ThreadPoolTimer> timers = new List<ThreadPoolTimer>
            {
                ThreadPoolTimer.CreateTimer(async (ThreadPoolTimer handlertimer) =>
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        mediaPlayerElement.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Videos/fantasmic_aladdin480p2.mp4"));
                    //await serial.SendData(new Scene(Scene.Scenes.Arabian, 0));
                    mediaPlayerElement.MediaPlayer.Play();
                    });
                }, new TimeSpan(0, 0, 1)),

                ThreadPoolTimer.CreateTimer(async (ThreadPoolTimer handlertimer) =>
                {
                    await serial.SendData(new Scene(Scene.Scenes.Arabian, 0));
                    btServer.SendMessage("test1");
                }, new TimeSpan(0, 0, 4)),

                ThreadPoolTimer.CreateTimer(async (ThreadPoolTimer handlertimer) =>
                {
                    await serial.SendData(new Scene(Scene.Scenes.Arabian, 1));
                    btServer.SendMessage("test2");
                }, new TimeSpan(0, 0, 14)),

                ThreadPoolTimer.CreateTimer(async (ThreadPoolTimer handlertimer) =>
                {
                    await serial.SendData(new Scene(Scene.Scenes.Arabian, 2));
                    btServer.SendMessage("test3");
                }, new TimeSpan(0, 0, 19)),

                ThreadPoolTimer.CreateTimer(async (ThreadPoolTimer handlertimer) =>
                {
                    await serial.SendData(new Scene(Scene.Scenes.Arabian, 3));
                    btServer.SendMessage("test4");
                }, new TimeSpan(0, 0, 24)),

                ThreadPoolTimer.CreateTimer(async (ThreadPoolTimer handlertimer) =>
                {
                    await serial.SendData(new Scene(Scene.Scenes.Arabian, 0));
                    btServer.SendMessage("test5");
                }, new TimeSpan(0, 0, 27)),

                ThreadPoolTimer.CreateTimer(async (ThreadPoolTimer handlertimer) =>
                {
                    await serial.SendData(new Scene(Scene.Scenes.Arabian, 4));
                    btServer.SendMessage("test6");
                }, new TimeSpan(0, 0, 30)),

                ThreadPoolTimer.CreateTimer(async (ThreadPoolTimer handlertimer) =>
                {
                    await serial.SendData(new Scene(Scene.Scenes.Arabian, 2));
                    btServer.SendMessage("test7");
                }, new TimeSpan(0, 0, 34)),

                ThreadPoolTimer.CreateTimer(async (ThreadPoolTimer handlertimer) =>
                {
                    await serial.SendData(new Scene(Scene.Scenes.Arabian, 3));
                    btServer.SendMessage("test8");
                }, new TimeSpan(0, 0, 35)),

                ThreadPoolTimer.CreateTimer(async (ThreadPoolTimer handlertimer) =>
                {
                    await serial.SendData(new Scene(Scene.Scenes.Arabian, 2));
                    btServer.SendMessage("test9");
                }, new TimeSpan(0, 0, 37)),

                ThreadPoolTimer.CreateTimer(async (ThreadPoolTimer handlertimer) =>
                {
                    await serial.SendData(new Scene(Scene.Scenes.Arabian, 4));
                    btServer.SendMessage("test10");
                }, new TimeSpan(0, 0, 51)),

                ThreadPoolTimer.CreateTimer(async (ThreadPoolTimer handlertimer) =>
                {
                    await serial.SendData(new Scene(Scene.Scenes.Arabian, 1));
                    btServer.SendMessage("test11");
                }, new TimeSpan(0, 0, 56)),

                ThreadPoolTimer.CreateTimer(async (ThreadPoolTimer handlertimer) =>
                {
                    await serial.SendData(new Scene(Scene.Scenes.Arabian, 6));
                    btServer.SendMessage("test12");
                }, new TimeSpan(0, 1, 6))
            };
            //Task.Run(() => Receiving());
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            await serial.SendData(currentState);
            ((Button)sender).IsEnabled = true;
        }
    }
}
