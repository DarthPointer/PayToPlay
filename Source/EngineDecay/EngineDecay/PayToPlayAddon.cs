using System.Collections.Generic;
using UnityEngine;


namespace EngineDecay
{
    [KSPAddon(KSPAddon.Startup.EditorAny | KSPAddon.Startup.Flight | KSPAddon.Startup.SpaceCentre | KSPAddon.Startup.TrackingStation, true)]
    class PayToPlayAddon : MonoBehaviour
    {
        public void Awake()
        {
            GameEvents.onVesselRecovered.Add(ModuleReader.Read);
        }
    }

    public static class ModuleReader
    {
        public static void Read(ProtoVessel v, bool wtf)
        {
            List<ProtoPartSnapshot> parts =  v.protoPartSnapshots;

            foreach (ProtoPartSnapshot part in parts)
            {
                ConfigNode engineDecay = part.FindModule("EngineDecay").moduleValues;

                if (engineDecay != null)
                {
                    if (engineDecay.GetValue("reliabilityStatus") == "failed")
                    {
                        ReliabilityProgress.fetch.Improve(part.partName, 0.3f, float.Parse(engineDecay.GetValue("r")));
                    }
                    else
                    {
                        float usedBurnTime = float.Parse(engineDecay.GetValue("usedBurnTime"));
                        float setBurnTime = float.Parse(engineDecay.GetValue("setBurnTime"));

                        if(usedBurnTime / setBurnTime > 1)
                        {
                            ReliabilityProgress.fetch.Improve(part.partName, 0.1f, float.Parse(engineDecay.GetValue("r")));
                        }
                        else
                        {
                            ReliabilityProgress.fetch.Improve(part.partName, 0.1f * usedBurnTime / setBurnTime, float.Parse(engineDecay.GetValue("r")));
                        }
                    }
                }
            }
        }
    }
}
