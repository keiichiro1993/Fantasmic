using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;

namespace FantasmicCommon.Models
{
    public class MediaAction
    {
        public MediaAction(TimeSpan mediaTimeSpan, Scene mediaScene)
        {
            if (mediaTimeSpan != null && mediaScene != null)
            {
                MediaTimeSpan = mediaTimeSpan;
                MediaScene = mediaScene;
            }
            else
            {
                throw new ArgumentNullException("MediaActionの値をnullにすることはできません。");
            }
        }

        public TimeSpan MediaTimeSpan { get; set; }
        public Scene MediaScene { get; set; }
    }

    public class MediaActionSet
    {
        public MediaSource MovieMediaSource { get; set; }
        public List<MediaAction> MediaActions { get; set; }
        public Scene.Scenes MediaScene { get; set; }

        public MediaActionSet(MediaSource mediaSource, List<MediaAction> mediaActions)
        {
            if (mediaSource != null && mediaActions != null)
            {
                MovieMediaSource = mediaSource;
                MediaActions = mediaActions;
                MediaScene = MediaActions[0].MediaScene.CurrentScene;
            }
            else
            {
                throw new ArgumentNullException("MediaActionの値をnullにすることはできません。");
            }
        }

    }
}
