using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineDecay
{
    //[KSPAddon(KSPAddon.Startup.Instantly, true)]
    class PayToPlayAddon
    {
        public void Start()
        {
            GameEvents.onVesselRecovered.Add(ModuleReader.Read);
        }
    }

    public static class ModuleReader
    {
        public static void Read(ProtoVessel v, bool wtf)
        {
            Vessel vessel = v.vesselRef;

            foreach (Part p in vessel.parts)
            {
                EngineDecay engineDecay = p.FindModuleImplementing<EngineDecay>();
                engineDecay.OnRecovered();
            }
        }
    }
}
