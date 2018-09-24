using FantasmicCommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasmicCommon.Utils.MediaPlayerHelper
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
}
