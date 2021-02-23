﻿//Coded with help of kerbalism's open source and Gotmachine's advce.

using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


namespace EngineDecay
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class PayToPlayAddon : MonoBehaviour
    {
        public static PayToPlayAddon fetch;

        Dictionary<string, List<string>> reliabilityStatuses;

        public static string RandomStatus(string statusType)
        {
            List<string> a;
            if (fetch.reliabilityStatuses.TryGetValue(statusType, out a))
            {
                if (a.Count > 0)
                {
                    return a[UnityEngine.Random.Range(0, fetch.reliabilityStatuses[statusType].Count)];     // Random string from the list
                }
                else
                {
                    return statusType;
                }
            }
            else
            {
                return statusType;
            }
        }

        public void Start()
        {
            GameEvents.onVesselRecovered.Add(ReadRecoveredVessel);

            fetch = this;

            reliabilityStatuses = new Dictionary<string, List<string>>();

            try
            {
                string[] fileNames = Directory.GetFiles("GameData/PayToPlay/Data/ReliabilityStatuses/");

                foreach (string i in fileNames)
                {
                    string setName = Path.GetFileNameWithoutExtension(i);           // Last part of the name without .txt appendix
                    reliabilityStatuses[setName] = new List<string>(File.ReadAllLines(i));
                }
            }
            catch (Exception)
            {
                Debug.LogError("PayToPlayAddon could not read reliability status strings from files PayToPlay/Data/ReliabilityStatuses/*.txt");
            }

            List<string> dummy = new List<string>();
        }

        public void OnDestroy()
        {
            GameEvents.onVesselRecovered.Remove(ReadRecoveredVessel);

            fetch = null;
        }

        public void ReadRecoveredVessel(ProtoVessel v, bool wtf)
        {
            if (PayToPlaySettingsFeatures.ReliabilityProgress)
            {
                Debug.Log("P2P: onVesselRecovered has successfully called Read");

                List<ProtoPartSnapshot> parts = v.protoPartSnapshots;

                if (ReliabilityProgress.fetch == null)
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
                            if (engineDecay.values.GetValue("procPart") == "False")
                            {
                                string engineModelId = engineDecay.GetValue("engineModelId");
                                float usageExperienceCoeff = float.Parse(engineDecay.GetValue("usageExperienceCoeff"));

                                ReliabilityProgress.fetch.Improve(engineModelId,
                                    usageExperienceCoeff, float.Parse(engineDecay.GetValue("r")));

                                if (usageExperienceCoeff > 0 && engineDecay.HasNode("SIBLINGS"))
                                {
                                    float thisModelR = ReliabilityProgress.fetch.GetReliabilityData(engineModelId).r;
                                    foreach (ConfigNode.Value i in engineDecay.GetNode("SIBLINGS").values)
                                    {
                                        ReliabilityProgress.fetch.SiblingImproveIfLess(i.name, thisModelR * float.Parse(i.value));
                                    }
                                }

                            }
                            else
                            {
                                ReliabilityProgress.fetch.ImproveProcedural(engineDecay.GetValue("engineModelId"),
                                    float.Parse(engineDecay.values.GetValue("procSRBDiameter")), 
                                    float.Parse(engineDecay.values.GetValue("procSRBThrust")), engineDecay.values.GetValue("procSRBBellName"),
                                    float.Parse(engineDecay.GetValue("usageExperienceCoeff")), float.Parse(engineDecay.GetValue("r")));
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("given ProtoVessel.protoPartSnapshots was null, cannot retrieve usage experience of the recovered vessel");
                }
            }
        }
    }
}
