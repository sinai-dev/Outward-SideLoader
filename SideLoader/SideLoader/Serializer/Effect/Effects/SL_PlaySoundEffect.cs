using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SideLoader
{
    public class SL_PlaySoundEffect : SL_Effect
    {
        public List<GlobalAudioManager.Sounds> Sounds = new List<GlobalAudioManager.Sounds>();
        public bool Follow;
        public float MinPitch;
        public float MaxPitch;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as PlaySoundEffect;

            comp.Sounds = this.Sounds.ToArray();
            comp.Follow = this.Follow;
            comp.MinPitch = this.MinPitch;
            comp.MaxPitch = this.MaxPitch;
        }

        public override void SerializeEffect<T>(T effect, SL_Effect holder)
        {
            var template = holder as SL_PlaySoundEffect;
            var comp = effect as PlaySoundEffect;

            template.Follow = comp.Follow;
            template.MaxPitch = comp.MaxPitch;
            template.MinPitch = comp.MinPitch;
            template.Sounds = comp.Sounds.ToList();
        }
    }
}
