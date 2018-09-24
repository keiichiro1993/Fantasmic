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
using FantasmicCommon.Utils.MediaPlayerHelper;
using Windows.UI.Popups;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace FantasmicHead
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //フルスクリーンに
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TryEnterFullScreenMode();


            var actions = new List<MediaAction>
            {
                new MediaAction(new TimeSpan(0, 0, 3), new Scene(Scene.Scenes.Arabian, 0)),
                new MediaAction(new TimeSpan(0, 0, 13), new Scene(Scene.Scenes.Arabian, 1)),
                new MediaAction(new TimeSpan(0, 0, 18), new Scene(Scene.Scenes.Arabian, 2)),
                new MediaAction(new TimeSpan(0, 0, 23), new Scene(Scene.Scenes.Arabian, 3)),
                new MediaAction(new TimeSpan(0, 0, 26), new Scene(Scene.Scenes.Arabian, 0)),
                new MediaAction(new TimeSpan(0, 0, 29), new Scene(Scene.Scenes.Arabian, 4)),
                new MediaAction(new TimeSpan(0, 0, 33), new Scene(Scene.Scenes.Arabian, 2)),
                new MediaAction(new TimeSpan(0, 0, 34), new Scene(Scene.Scenes.Arabian, 3)),
                new MediaAction(new TimeSpan(0, 0, 36), new Scene(Scene.Scenes.Arabian, 2)),
                new MediaAction(new TimeSpan(0, 0, 50), new Scene(Scene.Scenes.Arabian, 4)),
                new MediaAction(new TimeSpan(0, 0, 55), new Scene(Scene.Scenes.Arabian, 1)),
                new MediaAction(new TimeSpan(0, 1, 5), new Scene(Scene.Scenes.Arabian, 6))
            };
            mediaPlayerElement.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Videos/fantasmic_aladdin480p2.mp4"));

            try
            {
                var mediaActionPlayer = new MediaActionPlayer(mediaPlayerElement, actions, Dispatcher);
                mediaActionPlayer.Play();
            }
            catch (Exception ex)
            {
                var dlg = new MessageDialog(ex.Message, ex.ToString());
                await dlg.ShowAsync();
            }
        }
    }
}
