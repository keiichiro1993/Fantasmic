using FantasmicCommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;

namespace FantasmicMirror.Models
{
    public class SceneData
    {
        public readonly List<MediaActionSet> MediaActionSets = new List<MediaActionSet>
        {
            new MediaActionSet(
                MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Videos/FantasmicVillans.mp4")),
                new List<MediaAction>
                {
                    new MediaAction(new TimeSpan(0, 0, 0), new Scene(Scene.Scenes.Villans, 0)),
                    new MediaAction(new TimeSpan(0, 0, 57), new Scene(Scene.Scenes.Villans, 1)),
                }),
            new MediaActionSet(
                MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Videos/fantasmic_aladdin480p2.mp4")),
                new List<MediaAction>
                {
                    new MediaAction(new TimeSpan(0, 0, 3), new Scene(Scene.Scenes.Arabian, 0)),
                }),
            new MediaActionSet(
                MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Videos/FantasmicPrincess.mp4")),
                new List<MediaAction>
                {
                    new MediaAction(new TimeSpan(0, 0, 0), new Scene(Scene.Scenes.Princess, 0)),
                })
        };
    }
}
