using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasmicCommon.Models
{
    public class Scene
    {
        public enum Scenes { Arabian, Intro };

        public int CurrentMode { get; set; }
        public Scenes CurrentScene { get; set; }

        public Scene(Scenes scene, int mode)
        {
            CurrentMode = mode;
            CurrentScene = scene;
        }
    }
}
