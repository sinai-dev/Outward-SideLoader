using System.Linq;

namespace SideLoader
{
    public class SL_InstrumentClose : SL_EffectCondition
    {
        public int MainInstrumentID;
        public int[] OtherInstrumentIDs;
        public float Range;

        public override void ApplyToComponent<T>(T component)
        {
            var comp = component as InstrumentClose;

            if (ResourcesPrefabManager.Instance.GetItemPrefab(this.MainInstrumentID) is Instrument instrument)
                comp.Instrument = instrument;

            comp.OtherInstruments = this.OtherInstrumentIDs
                                        .Select(it => ResourcesPrefabManager.Instance.GetItemPrefab(it) as Instrument)
                                        .ToArray();

            comp.Range = this.Range;
        }

        public override void SerializeEffect<T>(T component)
        {
            var comp = component as InstrumentClose;

            MainInstrumentID = comp.Instrument?.ItemID ?? -1;
            OtherInstrumentIDs = comp.OtherInstruments?
                                     .Select(it => it?.ItemID ?? -1)
                                     .ToArray();
            Range = comp.Range;
        }
    }
}
