//Coded with help of kerbalism's open source and Gotmachine's advce.
//Kerbalism project was virtually the main step to make PayToPlay any possible.

using System;
using System.Collections.Generic;

namespace EngineDecay
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new[] { GameScenes.FLIGHT, GameScenes.EDITOR, GameScenes.TRACKSTATION, GameScenes.SPACECENTER, GameScenes.LOADING, GameScenes.LOADINGBUFFER })]
    public class ReliabilityProgress : ScenarioModule
    {
        public static ReliabilityProgress fetch;

        Dictionary<string, float> exponents;
        Dictionary<string, ProcSRBProgress> procSRBs;

        #region Constructors

        public ReliabilityProgress()
        {
            exponents = new Dictionary<string, float>();
            procSRBs = new Dictionary<string, ProcSRBProgress>();

            fetch = this;
        }

        #endregion

        #region ScenarioModule stuff

        public override void OnLoad(ConfigNode node)
        {
            foreach (ConfigNode.Value val in node.GetNodes("Engines")[0].values)
            {
                exponents[val.name] = float.Parse(val.value);
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
            Dictionary<string, float>.Enumerator i = exponents.GetEnumerator();
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

        public float GetExponent(string partName)
        {
            if (PayToPlaySettings.ReliabilityProgress)
            {
                if (!exponents.ContainsKey(partName))
                {
                    exponents[partName] = PayToPlaySettings.StartingReliability;
                }
                return exponents[partName];
            }
            else
            {
                return 8;
            }
        }

        public void Improve(string partName, float coeff, float generationExp)
        {
            if(!exponents.ContainsKey(partName))
            {
                exponents[partName] = PayToPlaySettings.StartingReliability;
            }


            float newExp = generationExp + (9 - generationExp) * coeff;
            if (newExp > 8)
            {
                newExp = 8;
            }

            if (newExp > exponents[partName])
            {
                exponents[partName] = newExp;
            }
        }

        public float CheckProcSRBProgress(string partName, ref float diameter, ref float thrust, ref string bellName)           // -1 if no registered model fits specified stats
        {                                                                                                                       // It CHANGES procedural data of EngineDecay in order to specify which model it is to prevent possible reliability progress issues
            ProcSRBProgress partProgress = procSRBs[partName];
            if (partProgress != null)
            {
                Dictionary<ProcSRBData, float>.Enumerator i = partProgress.models.GetEnumerator();

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

                return -1;
            }
            else
            {
                return -1;
            }
        }

        public void CreateModel(string partName, float diameter, float thrust, string bellName)
        {
            ProcSRBProgress partProgress = procSRBs[partName];
            if (partProgress == null)
            {
                partProgress = procSRBs[partName] = new ProcSRBProgress();
            }

            partProgress.models[new ProcSRBData(diameter, thrust, bellName)] = PayToPlaySettings.StartingReliability;
        }

        public void ImproveProcedural(string partName, float diameter, float thrust, string bellName, float coeff, float generationExp)
        {
            float newExp = generationExp + (9 - generationExp) * coeff;
            if (newExp > 8)
            {
                newExp = 8;
            }

            ProcSRBProgress partProgress = procSRBs[partName];
            if (partProgress == null)
            {
                partProgress = procSRBs[partName] = new ProcSRBProgress();
            }

            ProcSRBData key = new ProcSRBData(diameter, thrust, bellName);
            if (partProgress.models.TryGetValue(key , out float oldExp))
            {
                if (newExp > oldExp)
                {
                    partProgress.models[key] = newExp;
                }
            }
            else
            {
                partProgress.models[key] = newExp;
            }
        }

        #endregion

        class ProcSRBProgress
        {
            public Dictionary<ProcSRBData, float> models;

            public ProcSRBProgress() 
            {
                models = new Dictionary<ProcSRBData, float>();
            }
            public ProcSRBProgress(ConfigNode node)
            {
                models = new Dictionary<ProcSRBData, float>();
                foreach (ConfigNode.Value val in node.values)
                {
                    models[new ProcSRBData(val.name)] = float.Parse(val.value);
                }
            }

            public void ToConfigNode(ConfigNode node)
            {
                Dictionary<ProcSRBData, float>.Enumerator i = models.GetEnumerator();

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
                return ((thrust * 1.1 > _thrust) && (thrust * 0.9 < _thrust) && (diameter * 1.04 > _diameter) && (diameter * 0.96 < _diameter) && (bellName == _bellName));
            }
        }
    }
}
