using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EngineDecay
{
    class Lib
    {
        public static string Format(float seconds, int formatCode)
        {
            if (formatCode == 0)
            {
                return (int)seconds + "s";
            }

            else if (formatCode == 1)
            {
                return string.Format("{0:D2}:{1:D2}:{2:D2}", (int)seconds / 3600, ((int)seconds / 60) % 60, (int)seconds % 60);
            }

            else if (formatCode == 2)
            {
                return string.Format("{0}h:{1}:{2}", (int)seconds / 3600, ((int)seconds / 60) % 60, (int)seconds % 60);
            }

            else
            {
                throw new Exception("EngineDecay.Lib.Format: bad time format code");
            }
        }

        public static void Log(string message)
        {
            if (PayToPlaySettingsFeatures.ExtraDebugLogging)
            {
                Debug.Log("[P2P] " + message);
            }
        }

        public static void LogWarning(string message)
        {
            Debug.LogWarning("[P2P] " + message);
        }

        public static void LogError(string message)
        {
            Debug.LogError("[P2P] " + message);
        }
    }
}
