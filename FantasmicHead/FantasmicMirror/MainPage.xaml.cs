using FantasmicCommon.Models;
using FantasmicCommon.Utils;
using FantasmicCommon.Utils.BTClient;
using FantasmicCommon.Utils.MediaPlayerHelper;
using FantasmicMirror.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.UI.Popups;
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
        BTClient btClient;
        SceneData sceneData;
        public MainPage()
        {
            this.InitializeComponent();
            mediaActionPlayer = null;

            btClient = new BTClient(this);
            btClient.InitializeCompleted += Sender_InitializeCompleted;
            btClient.MessageRecieved += BtClient_MessageRecieved;
            sceneData = new SceneData();
        }

        MediaActionPlayer mediaActionPlayer;
        Scene currentScene;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            btClient.Initialize();

            //フルスクリーンに
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TryEnterFullScreenMode();

            var mediaSet = sceneData.MediaActionSets[0];
            while (mediaActionPlayer != null && mediaActionPlayer.IsPlaying)
            {
                await Task.Delay(500);
            }

            try
            {
                if (mediaActionPlayer == null)
                {
                    mediaActionPlayer = new MediaActionPlayer(mediaPlayerElement, mediaSet.MediaActions, Dispatcher, MediaActionPlayer.MediaActionPlayerType.Client);//ほんとはViewModel実装してDispatcherをなくしたい。。。
                }
                else
                {
                    currentScene = mediaSet.MediaActions[0].MediaScene;
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

        private async void BtClient_MessageRecieved(object sender, EventArgs e)
        {
            //debugBox.Text = (e as BTMessageRecievedEventArgs).RecievedMessage;
            debugBox.Text = "";
            Regex reg = new Regex("Request Change:Scene(?<scene>.*?):Mode");
            Match match = reg.Match((e as BTMessageRecievedEventArgs).RecievedMessage);
            var requestedScene = (Scene.Scenes)int.Parse(match.Groups["scene"].Value);

            Debug.WriteLine("Requested Scene: " + requestedScene.ToString());

            if (currentScene?.CurrentScene != requestedScene)
            {
                if (mediaActionPlayer.IsPlaying)
                {
                    mediaActionPlayer.CancelPlay();
                }

                var mediaSet = sceneData.MediaActionSets.Where(x => x.MediaScene == requestedScene).FirstOrDefault();
                currentScene = mediaSet.MediaActions[0].MediaScene;
                if (mediaSet != null)
                {
                    while (mediaActionPlayer.IsPlaying)
                    {
                        await Task.Delay(200);
                    }

                    mediaActionPlayer.ResetMediaAction(mediaSet.MediaActions);
                    mediaPlayerElement.Source = mediaSet.MovieMediaSource;
                    mediaActionPlayer.Play();
                }
            }
        }

        private void Sender_InitializeCompleted(object sender, EventArgs e)
        {
            String message = (e as BTInitEventArgs).ConnectionHostName;
            if ((e as BTInitEventArgs).IsHeadDetected)
            {
                message += " Head has been found!";
                Debug.WriteLine(message);
            }
            debugBox.Text = message;
        }
    }
}
