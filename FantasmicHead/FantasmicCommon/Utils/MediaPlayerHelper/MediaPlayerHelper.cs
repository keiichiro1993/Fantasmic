using FantasmicCommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace FantasmicCommon.Utils.MediaPlayerHelper
{
    public class MediaActionPlayer
    {
        MediaPlayerElement mediaPlayerElement;
        List<MediaAction> actions;

        CancellationTokenSource tokenSource;
        CancellationToken ct;

        public MediaActionPlayer(MediaPlayerElement mediaPlayerElement, List<MediaAction> actions)
        {
            var tokenSource = new CancellationTokenSource();
            ct = tokenSource.Token;
            if (actions == null || actions.Count == 0)
            {
                throw new InvalidOperationException("アクションが設定されていません。");
            }
            else
            {
                this.actions = actions;
            }

            if (mediaPlayerElement == null)
            {
                throw new InvalidOperationException("Media Player が設定されていません。");
            }
            else
            {
                this.mediaPlayerElement = mediaPlayerElement;
            }
        }

        ~MediaActionPlayer()
        {
            tokenSource.Dispose();
        }

        public async void Play()
        {
            mediaPlayerElement.MediaPlayer.Play();
            await Task.Run(async () =>
            {
                while (actions.Count != 0 && mediaPlayerElement.MediaPlayer.PlaybackSession.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Playing)
                {
                    if (ct.IsCancellationRequested)
                    {
                        mediaPlayerElement.MediaPlayer.Pause();
                        ct.ThrowIfCancellationRequested();
                    }
                    await Task.Delay(300);
                }
            });
        }

        public void CancelPlay()
        {
            tokenSource.Cancel();
        }
    }

    public class MediaAction
    {
        public TimeSpan timeSpan { get; set; }
        public Scene scene { get; set; }
    }
}
