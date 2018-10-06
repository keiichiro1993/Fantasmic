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
using FantasmicHead.Models;

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
            mediaActionPlayer = null;
        }


        MediaActionPlayer mediaActionPlayer;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //フルスクリーンに
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            var sceneData = new SceneData();

            foreach (var mediaSet in sceneData.MediaActionSets)
            {
                while(mediaActionPlayer != null && mediaActionPlayer.IsPlaying)
                {
                    await Task.Delay(500);
                }

                try
                {
                    if (mediaActionPlayer == null)
                    {
                        mediaActionPlayer = new MediaActionPlayer(mediaPlayerElement, mediaSet.MediaActions, Dispatcher);
                    }
                    else
                    {
                        mediaActionPlayer.ResetMediaAction(mediaSet.MediaActions);
                    }
                    mediaPlayerElement.Source = mediaSet.MovieMediaSource;
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
}
