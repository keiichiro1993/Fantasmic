﻿using FantasmicCommon.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace FantasmicCommon.Utils.MediaPlayerHelper
{
    public class MediaActionPlayer
    {
        MediaPlayerElement mediaPlayerElement;
        List<MediaAction> actions;

        CancellationTokenSource tokenSource;
        CancellationToken ct;

        //Scene currentScene;
        SerialUtil serial;
        BTServer btServer;

        CoreDispatcher dispatcher;

        bool isInitialized;
        bool isMediaStarted;

        public bool IsPlaying { get; set; }
        public enum MediaActionPlayerType { Client, Server };

        MediaActionPlayerType playerType;

        Scene.Scenes previousScene;

        public MediaActionPlayer(MediaPlayerElement mediaPlayerElement, List<MediaAction> actions, CoreDispatcher dispatcher, MediaActionPlayerType playerType)
        {
            IsPlaying = false;
            isInitialized = false;
            isMediaStarted = false;
            this.playerType = playerType;
            tokenSource = new CancellationTokenSource();
            ct = tokenSource.Token;

            if (actions == null || actions.Count == 0)
            {
                throw new ArgumentNullException("アクションが設定されていません。");
            }
            else
            {
                this.actions =
                    (from action in actions
                     orderby action.MediaTimeSpan ascending
                     select action).ToList();
            }

            if (mediaPlayerElement == null)
            {
                throw new ArgumentNullException("Media Player が設定されていません。");
            }
            else
            {
                this.mediaPlayerElement = mediaPlayerElement;
            }

            if (dispatcher == null)
            {
                throw new ArgumentNullException("Dispatcher が設定されていません。");
            }
            else
            {
                this.dispatcher = dispatcher;
            }
        }

        ~MediaActionPlayer()
        {
            tokenSource.Dispose();
        }

        private async Task Init()
        {
            if (playerType == MediaActionPlayerType.Server)
            {
                //Bluetooth の準備
                btServer = new BTServer();
                await btServer.InitializeRfcommServer();
            }
            //シリアルとシーン切り替えの準備
            /*serial = new SerialUtil();
            await serial.InitSerial();
            await serial.SendData(new Scene(Scene.Scenes.Arabian, 7));*/
            previousScene = actions[0].MediaScene.CurrentScene;
            
            //await serial.SendData(new Scene(Scene.Scenes.Arabian, 7));
            mediaPlayerElement.MediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;
            isInitialized = true;
        }

        public void ResetMediaAction(List<MediaAction> actions)
        {
            if (actions == null || actions.Count == 0)
            {
                throw new ArgumentNullException("アクションが設定されていません。");
            }
            else
            {
                this.actions =
                    (from action in actions
                     orderby action.MediaTimeSpan ascending
                     select action).ToList();
            }
        }

        public async void Play()
        {
            IsPlaying = true;
            if (!isInitialized)
            {
                await Init();
            }

            //毎回USBシリアルを検出するように。
            if (serial != null)
            {
                await serial.DisposeSerial();
            }
            serial = new SerialUtil();
            await serial.InitSerial();
            await serial.SendData(new Scene(Scene.Scenes.Arabian, 7));


            if (ct.IsCancellationRequested)
            {
                tokenSource.Dispose();
                tokenSource = new CancellationTokenSource();
                ct = tokenSource.Token;
            }


            await dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                mediaPlayerElement.MediaPlayer.Play();
            });
        }

        private async void PlaybackSession_PlaybackStateChanged(Windows.Media.Playback.MediaPlaybackSession sender, object args)
        {
            if (!isMediaStarted)
            {
                if (sender.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Playing)
                {
                    isMediaStarted = true;
                    await Task.Run(async () =>
                    {
                        await dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
                        {
                            var count = 0;
                            try
                            {
                                while (count < actions.Count && mediaPlayerElement.MediaPlayer.PlaybackSession.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Playing)
                                {
                                    if (ct.IsCancellationRequested)
                                    {
                                        mediaPlayerElement.MediaPlayer.Pause();
                                        ct.ThrowIfCancellationRequested();
                                    }

                                    var mediaAction = actions[count];

                                    if (TimeSpan.Compare(mediaAction.MediaTimeSpan, mediaPlayerElement.MediaPlayer.PlaybackSession.Position) <= 0)
                                    {
                                        Debug.WriteLine("/Count: " + count.ToString() + " /Action: " + mediaAction.MediaScene.CurrentSequence);

                                        var serialTask = serial.SendData(mediaAction.MediaScene);
                                        if (playerType == MediaActionPlayerType.Server)
                                        {
                                            if (previousScene != mediaAction.MediaScene.CurrentScene)
                                            {
                                                previousScene = mediaAction.MediaScene.CurrentScene;
                                                var btTask = btServer.SendMessage(mediaAction.MediaScene);
                                                while (!(serialTask.IsCompleted && btTask.IsCompleted))
                                                {
                                                    await Task.Delay(100);
                                                }
                                            }
                                            else
                                            {
                                                while (!serialTask.IsCompleted)
                                                {
                                                    await Task.Delay(100);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            while (!serialTask.IsCompleted)
                                            {
                                                await Task.Delay(100);
                                            }
                                        }


                                        count++;
                                    }

                                    await Task.Delay(200);
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                return;
                            }
                        });
                    });
                }
            }
            else
            {
                if (isMediaStarted && sender.PlaybackState != Windows.Media.Playback.MediaPlaybackState.Playing && sender.PlaybackState != Windows.Media.Playback.MediaPlaybackState.Buffering)
                {
                    IsPlaying = false;
                    isMediaStarted = false;
                }
            }
        }

        public void CancelPlay()
        {
            tokenSource.Cancel();
            mediaPlayerElement.MediaPlayer.Pause();
        }
    }

}
