using FantasmicCommon.Models;
using FantasmicCommon.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace FantasmicMirror
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
            /*currentState = new Scene(Scene.Scenes.Arabian, 3);
            serial = new SerialUtil();
            await serial.InitSerial();*/

            mediaPlayerElement.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Videos/Fantasia_part2_2.mp4"));
            await Task.Delay(500);
            mediaPlayerElement.MediaPlayer.Play();
        }
    }
}
