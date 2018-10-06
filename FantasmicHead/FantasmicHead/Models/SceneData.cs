using FantasmicCommon.Models;
using FantasmicCommon.Utils.MediaPlayerHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;

namespace FantasmicHead.Models
{
    public class SceneData
    {
        public readonly List<MediaActionSet> MediaActionSets = new List<MediaActionSet>
        {
            new MediaActionSet(
                MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Videos/fantasmic_aladdin480p2.mp4")),
                new List<MediaAction>
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
                }),
            new MediaActionSet(
                MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Videos/FantasmicPrincess.mp4")),
                new List<MediaAction>
                {
                    new MediaAction(new TimeSpan(0, 0, 0), new Scene(Scene.Scenes.Princess, 0)),
                    new MediaAction(new TimeSpan(0, 0, 18), new Scene(Scene.Scenes.Princess, 2)),
                    new MediaAction(new TimeSpan(0, 0, 21), new Scene(Scene.Scenes.Princess, 1)),
                    new MediaAction(new TimeSpan(0, 0, 30), new Scene(Scene.Scenes.Princess, 4)),
                    new MediaAction(new TimeSpan(0, 0, 40), new Scene(Scene.Scenes.Princess, 1)),
                    new MediaAction(new TimeSpan(0, 0, 50), new Scene(Scene.Scenes.Princess, 4)),
                    new MediaAction(new TimeSpan(0, 1, 0), new Scene(Scene.Scenes.Princess, 1)),
                    new MediaAction(new TimeSpan(0, 1, 20), new Scene(Scene.Scenes.Princess, 2)),
                    new MediaAction(new TimeSpan(0, 1, 22), new Scene(Scene.Scenes.Princess, 1)),
                    new MediaAction(new TimeSpan(0, 1, 31), new Scene(Scene.Scenes.Princess, 3))
                })
        };
    }
}
