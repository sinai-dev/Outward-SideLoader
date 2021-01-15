using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SideLoader
{
    public class SL_PlayTimedVFX : SL_PlayVFX, ICustomEffect
    {
        public Type ComponentModel => typeof(PlayTimedVFX);

        /// <summary>
        /// Delay after which the PlayVFX will be force-stopped.
        /// </summary>
        public float AutoStopTime;

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            (component as PlayTimedVFX).AutoStopTime = this.AutoStopTime;
        }

        public override void SerializeEffect<T>(T effect)
        {
            base.SerializeEffect(effect);

            this.AutoStopTime = (effect as PlayTimedVFX).AutoStopTime;
        }
    }

    public class PlayTimedVFX : PlayVFX, ICustomComponent
    {
        public Type SLTemplateModel => typeof(SL_PlayTimedVFX);

        public float AutoStopTime;

        protected override void ActivateLocally(Character _affectedCharacter, object[] _infos)
        {
            base.ActivateLocally(_affectedCharacter, _infos);

            if (AutoStopTime > 0f)
                StartCoroutine(DelayedStopCoroutine());
        }

        private IEnumerator DelayedStopCoroutine()
        {
            yield return new WaitForSeconds(AutoStopTime);

            if (At.GetField(this as PlayVFX, "m_startVFX") is VFXSystem vfx)
                vfx.Stop();
            else
                SL.LogWarning("SL_PlayTimedVFX.DelayedStopCoroutine - vfx was null after delay");
        }
    }
}
