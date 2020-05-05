//Coded with help of kerbalism's open source and Gotmachine's advce.

using System;
using System.Collections.Generic;
using UnityEngine;


namespace EngineDecay
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class PayToPlayAddon : MonoBehaviour
    {
        public void Start()
        {
            UnityEngine.Debug.Log("=== P2PAddon has been started ===");
            GameEvents.onVesselRecovered.Add(Read);
        }

        public void OnDestroy()
        {
            GameEvents.onVesselRecovered.Remove(Read);
        }

        public void Read(ProtoVessel v, bool wtf)
        {
            UnityEngine.Debug.Log("=== onVesselRacovered has successfully called Read ===");

            List<ProtoPartSnapshot> parts = v.protoPartSnapshots;

            if(ReliabilityProgress.fetch == null)
            {
                throw new Exception("ReliabilityProgress SCENARIO had not been booted when it was time to retrieve usage experience");
            }

            if (parts != null)
            {
                foreach (ProtoPartSnapshot part in parts)
                {
                    ConfigNode engineDecay = part.FindModule("EngineDecay")?.moduleValues;

                    if (engineDecay != null)
                    {
                        ReliabilityProgress.fetch.Improve(engineDecay.name, float.Parse(engineDecay.GetValue("usageExperienceCoeff")), float.Parse(engineDecay.GetValue("r")));
                    }
                }
            }
            else
            {
                UnityEngine.Debug.Log("given ProtoVessel.protoPartSnapshots was null, cannot retrieve usage experience of the recovered vessel");
            }
        }
    }
}
