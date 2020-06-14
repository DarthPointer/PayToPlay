//Coded with help of kerbalism's open source and Gotmachine's advce.

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
            return fetch.reliabilityStatuses[statusType]
                [UnityEngine.Random.Range(0, fetch.reliabilityStatuses[statusType].Count - 1)];     // Random string from corresponding list
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
                    FileStream fileStream = new FileStream(i, FileMode.Open);
                    StreamReader reader = new StreamReader(fileStream);

                    string setName = i.Split('/')[i.Split('/').Length - 1].Split('.')[0];           // Last part of the name without .txt appendix
                    List<string> stringList = new List<string>();

                    while (!reader.EndOfStream)
                    {
                        stringList.Add(reader.ReadLine());
                    }

                    reliabilityStatuses[setName] = stringList;

                    reader.Close();
                    // fileStream.Close();              Do we need it?
                }
            }
            catch (Exception)
            {
                Debug.LogError("PayToPlayAddon could not read reliability status strings from files PayToPlay/Data/ReliabilityStatuses/*.txt");

                reliabilityStatuses["HeavilyReused"] = new List<string>();
                reliabilityStatuses["HeavilyReused"].Add("Further reuse is considered to be unsafe");

                reliabilityStatuses["LowReliabilityModel"] = new List<string>();
                reliabilityStatuses["LowReliabilityModel"].Add("Needs some testing before safe use");

                reliabilityStatuses["PoorEngineCondition"] = new List<string>();
                reliabilityStatuses["PoorEngineCondition"].Add("It is recommended to perform maintenance ASAP");
            }

            List<string> dummy = new List<string>();

            if (!reliabilityStatuses.TryGetValue("HeavilyReused", out dummy))
            {
                reliabilityStatuses["HeavilyReused"] = new List<string>();
                reliabilityStatuses["HeavilyReused"].Add("Further reuse is considered to be unsafe");
            }
            else if (reliabilityStatuses["HeavilyReused"].Count == 0)
            {
                reliabilityStatuses["HeavilyReused"].Add("Further reuse is considered to be unsafe");
            }

            if (!reliabilityStatuses.TryGetValue("LowReliabilityModel", out dummy))
            {
                reliabilityStatuses["LowReliabilityModel"] = new List<string>();
                reliabilityStatuses["LowReliabilityModel"].Add("Further reuse is considered to be unsafe");
            }
            else if (reliabilityStatuses["LowReliabilityModel"].Count == 0)
            {
                reliabilityStatuses["LowReliabilityModel"].Add("Further reuse is considered to be unsafe");
            }

            if (!reliabilityStatuses.TryGetValue("PoorEngineCondition", out dummy))
            {
                reliabilityStatuses["PoorEngineCondition"] = new List<string>();
                reliabilityStatuses["PoorEngineCondition"].Add("Further reuse is considered to be unsafe");
            }
            else if (reliabilityStatuses["PoorEngineCondition"].Count == 0)
            {
                reliabilityStatuses["PoorEngineCondition"].Add("Further reuse is considered to be unsafe");
            }
        }

        public void OnDestroy()
        {
            GameEvents.onVesselRecovered.Remove(ReadRecoveredVessel);
            fetch = null;
        }

        public void ReadRecoveredVessel(ProtoVessel v, bool wtf)
        {
            if (PayToPlaySettings.ReliabilityProgress)
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
                                ReliabilityProgress.fetch.Improve(part.partName, float.Parse(engineDecay.GetValue("usageExperienceCoeff")), float.Parse(engineDecay.GetValue("r")));
                            }
                            else
                            {
                                ReliabilityProgress.fetch.ImproveProcedural(part.partName, float.Parse(engineDecay.values.GetValue("procSRBDiameter")), 
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
