//Coded with help of kerbalism's open source and Gotmachine's advce.
//Kerbalism project was virtually the main step to make PayToPlay any possible.

using System;
using System.Collections.Generic;

namespace EngineDecay
{
    public class ReliabilityProgressData
    {
        public float r;
        public bool reliabilityIsVisible;

        public ReliabilityProgressData(float _r, bool _reliabilityIsVisible)
        {
            r = _r;
            reliabilityIsVisible = _reliabilityIsVisible;
        }

        public ReliabilityProgressData(string s)
        {
            string[] fields = s.Split(';');
            r = 2;
            reliabilityIsVisible = false;

            if (fields.Length == 0)
            {
                Lib.LogWarning("For some reason reliability progress record was empty");
            }
            else if (fields.Length == 1)
            {
                Lib.LogWarning("Reliability progress record was missing visibility flag, ignore this if you use a save from P2P 1.4.3 or below");

                try
                {
                    r = float.Parse(fields[0]);

                    if (r != PatToPlaySettingsDifficultyNumbers.StartingReliability)
                    {
                        reliabilityIsVisible = true;
                    }
                }
                catch (FormatException)
                {
                    Lib.LogWarning("Reliability progress record contains misfomatted exponent (float)");
                }
            }
            else
            {
                try
                {
                    r = float.Parse(fields[0]);
                }
                catch (FormatException)
                {
                    Lib.LogWarning("Reliability progress record contains misfomatted exponent (needs float)");
                }

                try
                {
                    reliabilityIsVisible = bool.Parse(fields[1]);
                }
                catch (FormatException)
                {
                    Lib.LogWarning("Reliability progress record contains misfomatted reliability visibility flag (needs bool)");
                }

                if (fields.Length > 2)
                {
                    Lib.LogWarning("Reliability progress record contains more than two fields splitted with ';'");
                }
            }
        }

        public override string ToString()
        {
            return $"{r}; {reliabilityIsVisible}";
        }
    }

    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new[] { GameScenes.FLIGHT, GameScenes.EDITOR, GameScenes.TRACKSTATION, GameScenes.SPACECENTER, GameScenes.LOADING, GameScenes.LOADINGBUFFER })]
    public class ReliabilityProgress : ScenarioModule
    {
        public static ReliabilityProgress fetch;

        Dictionary<string, ReliabilityProgressData> records;
        Dictionary<string, ProcSRBProgress> procSRBs;

        #region Constructors

        public ReliabilityProgress()
        {
            records = new Dictionary<string, ReliabilityProgressData>();
            procSRBs = new Dictionary<string, ProcSRBProgress>();

            fetch = this;
        }

        #endregion

        #region ScenarioModule stuff

        public override void OnLoad(ConfigNode node)
        {
            foreach (ConfigNode.Value val in node.GetNodes("Engines")[0].values)
            {
                records[val.name] = new ReliabilityProgressData(val.value);
            }


            foreach (ConfigNode nd in node.GetNodes("Engines")[0].nodes)
            {
                if (nd != null)
                {
                    procSRBs[nd.name] = new ProcSRBProgress(nd);
                }
            }
        }

        public override void OnSave(ConfigNode node)
        {
            Dictionary<string, ReliabilityProgressData>.Enumerator i = records.GetEnumerator();
            ConfigNode engines = node.AddNode("Engines");
            while (i.MoveNext())
            {
                engines.AddValue(i.Current.Key, i.Current.Value);
            }

            Dictionary<string, ProcSRBProgress>.Enumerator j = procSRBs.GetEnumerator();
            while (j.MoveNext())
            {
                ConfigNode procSRBPart = engines.AddNode(j.Current.Key);
                j.Current.Value.ToConfigNode(procSRBPart);
            }
        }

        private void OnDestroy()
        {
            fetch = null;
        }

        #endregion

        #region Logic

        public ReliabilityProgressData GetReliabilityData(string partName)
        {
            Lib.Log($"Getting reliability progress for {partName}");

            if (PayToPlaySettingsFeatures.ReliabilityProgress)
            {
                if (!records.ContainsKey(partName))
                {
                    Lib.Log($"Reliability progress record for {partName} not found, creating it");

                    float r = PatToPlaySettingsDifficultyNumbers.StartingReliability;
                    if (PayToPlaySettingsFeatures.RandomStartingReliability)
                    {
                        Lib.Log($"Applying random starting reliability bonus for {partName}");
                        r += UnityEngine.Random.Range(0f, 1f) * PatToPlaySettingsDifficultyNumbers.RandomStartingReliabilityBonusLimit;
                    }
                    r = Math.Min(r, 8);

                    records[partName] = new ReliabilityProgressData(r, !PayToPlaySettingsFeatures.HideStartingReliability);
                }
                return records[partName];
            }
            else
            {
                return new ReliabilityProgressData(8, true);
            }
        }

        public void Improve(string partName, float coeff, float generationExp)
        {
            if(!records.ContainsKey(partName))
            {
                records[partName] = new ReliabilityProgressData(PatToPlaySettingsDifficultyNumbers.StartingReliability, !PayToPlaySettingsFeatures.HideStartingReliability);
            }

            float oldExp = records[partName].r;
            float newExp = generationExp + (9 - generationExp) * coeff;
            if (newExp > 8)
            {
                newExp = 8;
            }

            if (newExp > oldExp)
            {
                records[partName].r = newExp;
            }
            if (coeff > 0)
            {
                records[partName].reliabilityIsVisible = true;
            }
        }

        public ReliabilityProgressData CheckProcSRBProgress(string partName, ref float diameter, ref float thrust, ref string bellName)           // -1 if no registered model fits specified stats
        {                                                                                                                       // It CHANGES procedural data of EngineDecay in order to specify which model it is to prevent possible reliability progress issues
            if (PayToPlaySettingsFeatures.ReliabilityProgress)
            {
                ProcSRBProgress partProgress;
                if (procSRBs.TryGetValue(partName, out partProgress))
                {
                    Dictionary<ProcSRBData, ReliabilityProgressData>.Enumerator i = partProgress.models.GetEnumerator();

                    while (i.MoveNext())
                    {
                        if (i.Current.Key.Fits(diameter, thrust, bellName))
                        {
                            diameter = i.Current.Key.diameter;
                            thrust = i.Current.Key.thrust;
                            bellName = i.Current.Key.bellName;

                            return i.Current.Value;
                        }
                    }

                    return new ReliabilityProgressData(-1, !PayToPlaySettingsFeatures.HideStartingReliability);
                }
                else
                {
                    return new ReliabilityProgressData(-1, !PayToPlaySettingsFeatures.HideStartingReliability);
                }
            }
            else
            {
                return new ReliabilityProgressData(8, true);
            }
        }

        public void CreateModel(string partName, float startingReliability, float diameter, float thrust, string bellName)
        {
            ProcSRBProgress partProgress;
            if (!procSRBs.TryGetValue(partName, out partProgress))
            {
                partProgress = procSRBs[partName] = new ProcSRBProgress();
            }

            partProgress.models[new ProcSRBData(diameter, thrust, bellName)] = new ReliabilityProgressData(startingReliability, !PayToPlaySettingsFeatures.HideStartingReliability);
        }

        public void ImproveProcedural(string partName, float diameter, float thrust, string bellName, float coeff, float generationExp)
        {
            if (PayToPlaySettingsFeatures.ReliabilityProgress)
            {
                float newExp = generationExp + (9 - generationExp) * coeff;
                if (newExp > 8)
                {
                    newExp = 8;
                }

                ProcSRBProgress partProgress;
                if (!procSRBs.TryGetValue(partName, out partProgress))
                {
                    partProgress = procSRBs[partName] = new ProcSRBProgress();
                }

                ProcSRBData key = new ProcSRBData(diameter, thrust, bellName);
                if (partProgress.models.TryGetValue(key, out ReliabilityProgressData oldRecord))
                {
                    if (newExp > oldRecord.r)
                    {
                        partProgress.models[key].r = newExp;
                    }

                    if (coeff > 0)
                    {
                        partProgress.models[key].reliabilityIsVisible = true;
                    }
                }
                else
                {
                    partProgress.models[key] = new ReliabilityProgressData(newExp, true);
                }
            }
        }

        #endregion

        class ProcSRBProgress
        {
            public Dictionary<ProcSRBData, ReliabilityProgressData> models;

            public ProcSRBProgress() 
            {
                models = new Dictionary<ProcSRBData, ReliabilityProgressData>();
            }
            public ProcSRBProgress(ConfigNode node)
            {
                models = new Dictionary<ProcSRBData, ReliabilityProgressData>();
                foreach (ConfigNode.Value val in node.values)
                {
                    models[new ProcSRBData(val.name)] = new ReliabilityProgressData(val.value);
                }
            }

            public void ToConfigNode(ConfigNode node)
            {
                Dictionary<ProcSRBData, ReliabilityProgressData>.Enumerator i = models.GetEnumerator();

                while (i.MoveNext())
                {
                    node.AddValue(i.Current.Key.ToString(), i.Current.Value);
                }
            }
        }

        class ProcSRBData
        {
            public float diameter;
            public float thrust;
            public string bellName;

            public ProcSRBData() { }
            public ProcSRBData(string name)
            {
                string []a = name.Split('_');
                if (a.Length == 4)
                {
                    diameter = float.Parse(a[1].Replace('d', '.'));
                    thrust = float.Parse(a[2].Replace('d', '.'));
                    bellName = a[3];
                }
                else
                {
                    throw new Exception("Error while reading procedural SRB model data: number of separators '_' is not 3");
                }
            }
            public ProcSRBData(float _diameter, float _thrust, string _bellName)
            {
                diameter = _diameter;
                thrust = _thrust;
                bellName = _bellName;
            }

            public override string ToString()
            {
                return (string.Format("m_{0}_{1}_{2}", diameter.ToString().Replace('.', 'd'), thrust.ToString().Replace('.', 'd'), bellName));
            }

            public bool Fits (float _diameter, float _thrust, string _bellName)
            {
                float maxDiameter = diameter * (1 + PatToPlaySettingsDifficultyNumbers.ProcSRBDiameterModelMarginPercent/100);
                float minDiameter = diameter * (1 - PatToPlaySettingsDifficultyNumbers.ProcSRBDiameterModelMarginPercent/100);

                float maxThrust = thrust * (1 + PatToPlaySettingsDifficultyNumbers.ProcSRBThrustModelMarginPercent / 100);
                float minThrust = thrust * (1 - PatToPlaySettingsDifficultyNumbers.ProcSRBThrustModelMarginPercent / 100);

                return ((maxDiameter >= _diameter) && (minDiameter <= _diameter) && (maxThrust >= _thrust) && (minThrust <= _thrust) && (bellName == _bellName));
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }

                if (obj as ProcSRBData == null)
                {
                    return false;
                }

                return (diameter == (obj as ProcSRBData).diameter) && (thrust == (obj as ProcSRBData).thrust) && (bellName == (obj as ProcSRBData).bellName);
            }

            public override int GetHashCode()
            {
                return (int)diameter + (int)thrust + bellName.GetHashCode();
            }
        }
    }
}
