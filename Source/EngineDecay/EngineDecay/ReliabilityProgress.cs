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
                procSRBs[nd.name] = new ProcSRBProgress(nd);
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
                ConfigNode procSRBPart = engines.AddNode(i.Current.Key);
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

        #endregion

        class ProcSRBProgress
        {
            Dictionary<ProcSRBData, float> models;

            public ProcSRBProgress() 
            {
                models = new Dictionary<ProcSRBData, float>();
            }
            public ProcSRBProgress(ConfigNode node)
            {
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
                string []a = name.Split('|');
                if (a.Length == 3)
                {
                    diameter = float.Parse(a[0]);
                    thrust = float.Parse(a[1]);
                    bellName = a[2];
                }
                else
                {
                    throw new Exception("Error while reading procedural SRB model data: number of \"fields\" is not 3");
                }
            }

            public override string ToString()
            {
                return (string.Format("{0}|{1}|{2}", diameter, thrust, bellName));
            }

            public bool fits (ProcSRBData data)
            {
                return ((thrust * 1.1 > data.thrust) && (thrust * 0.9 < data.thrust) && (diameter * 1.04 > data.diameter) && (diameter * 0.96 < data.diameter) && (bellName == data.bellName));
            }
        }
    }
}
