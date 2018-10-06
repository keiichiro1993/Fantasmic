using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasmicCommon.Models
{
    public class Scene
    {
        public enum Scenes { Arabian, Intro, Princess, Villans };

        public int CurrentSequence { get; set; }
        public Scenes CurrentScene { get; set; }

        public Scene(Scenes scene, int sequence)
        {
            CurrentSequence = sequence;
            CurrentScene = scene;
        }
    }
}
