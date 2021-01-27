namespace SideLoader
{
    public class SL_ShootBlastHornetControl : SL_ShootBlast
    {
        public int BurstSkillID;
        public int HealSkillID;

        public float Acceleration;
        public float Speed;
        public float SpeedDistLerpWhenCloseMult;

        public float DistStayOnTarget;
        public float EndEffectTriggerDist;
        public float EnvironmentCheckRadius;

        public float TimeFlight;
        public float TimeStayOnTarget;

        public float PassiveTimeFlight;
        public float PassiveTimeStayOnTarget;

        public float HornetLookForTargetRange;
        public float HornetPassiveAttackTimer;
        public float HornetPassiveTargetRange;

        public override void ApplyToComponent<T>(T component)
        {
            base.ApplyToComponent(component);

            var comp = component as ShootBlastHornetControl;

            if (ResourcesPrefabManager.Instance.GetItemPrefab(BurstSkillID) is Skill burstSkill)
            {
                comp.BurstSkill = burstSkill;
            }
            else
            {
                SL.Log("SL_ShootBlastHornetControl - Could not get Burst Skill ID " + BurstSkillID);
            }

            if (ResourcesPrefabManager.Instance.GetItemPrefab(HealSkillID) is Skill healSkill)
            {
                comp.HealSkill = healSkill;
            }
            else
            {
                SL.Log("SL_ShootBlastHornetControl - Could not get Heal Skill ID " + HealSkillID);
            }

            comp.Acceleration = this.Acceleration;
            comp.DistStayOnTarget = this.DistStayOnTarget;
            comp.EndEffectTriggerDist = this.EndEffectTriggerDist;
            comp.EnvironmentCheckRadius = this.EnvironmentCheckRadius;
            comp.HornetLookForTargetRange = this.HornetLookForTargetRange;
            comp.HornetPassiveAttackTimer = this.HornetPassiveAttackTimer;
            comp.HornetPassiveTargetRange = this.HornetPassiveTargetRange;
            comp.PassiveTimeFlight = this.PassiveTimeFlight;
            comp.PassiveTimeStayOnTarget = this.PassiveTimeStayOnTarget;
            comp.Speed = this.Speed;
            comp.SpeedDistLerpWhenCloseMult = this.SpeedDistLerpWhenCloseMult;
            comp.TimeFlight = this.TimeFlight;
            comp.TimeStayOnTarget = this.TimeStayOnTarget;
        }

        public override void SerializeEffect<T>(T effect)
        {
            base.SerializeEffect(effect);

            var comp = effect as ShootBlastHornetControl;

            BurstSkillID = comp.BurstSkill?.ItemID ?? -1;
            HealSkillID = comp.HealSkill?.ItemID ?? -1;

            Acceleration = comp.Acceleration;
            DistStayOnTarget = comp.DistStayOnTarget;
            EndEffectTriggerDist = comp.EndEffectTriggerDist;
            EnvironmentCheckRadius = comp.EnvironmentCheckRadius;
            HornetLookForTargetRange = comp.HornetLookForTargetRange;
            HornetPassiveAttackTimer = comp.HornetPassiveAttackTimer;
            HornetPassiveTargetRange = comp.HornetPassiveTargetRange;
            PassiveTimeFlight = comp.PassiveTimeFlight;
            PassiveTimeStayOnTarget = comp.PassiveTimeStayOnTarget;
            Speed = comp.Speed;
            SpeedDistLerpWhenCloseMult = comp.SpeedDistLerpWhenCloseMult;
            TimeFlight = comp.TimeFlight;
            TimeStayOnTarget = comp.TimeStayOnTarget;
        }
    }
}
