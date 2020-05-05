//Coded with help of kerbalism's open source and Gotmachine's advce.
//Kerbalism project was virtually the main step to make PayToPlay any possible

using System;
using System.Collections.Generic;

namespace EngineDecay
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, new[] { GameScenes.FLIGHT, GameScenes.EDITOR, GameScenes.TRACKSTATION, GameScenes.SPACECENTER })]
    public class ReliabilityProgress : ScenarioModule
    {
        public static ReliabilityProgress fetch;

        Dictionary<string, float> exponents;

        #region Constructors

        public ReliabilityProgress()
        {
            exponents = new Dictionary<string, float>();
            fetch = this;
        }

        public ReliabilityProgress(ConfigNode node)
        {
            int c = 0;
            foreach (ConfigNode.Value val in node.values)
            {
                if (c >= 2)
                {
                    exponents[val.name] = float.Parse(val.value);
                }
                c++;
            }
        }

        #endregion

        #region ScenarioModule stuff

        public override void OnLoad(ConfigNode node)
        {
            foreach (ConfigNode.Value val in node.GetNodes("Engines")[0].values)
            {
                exponents[val.name] = float.Parse(val.value);
            }
        }

        public override void OnSave(ConfigNode node)
        {
            Dictionary<string, float>.Enumerator i = exponents.GetEnumerator();
            ConfigNode Engines = node.AddNode("Engines");
            while (i.MoveNext())
            {
                Engines.AddValue(i.Current.Key, i.Current.Value);
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
            if(!exponents.ContainsKey(partName))
            {
                exponents[partName] = 2;
            }
            return exponents[partName];
        }

        public void Improve(string partName, float coeff, float generationExp)
        {
            if(!exponents.ContainsKey(partName))
            {
                throw new Exception("An improvement event has been called for a part with no reliability data recorded");
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
    }
}
