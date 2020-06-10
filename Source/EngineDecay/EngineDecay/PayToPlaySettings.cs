using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineDecay
{
    public class PayToPlaySettings : GameParameters.CustomParameterNode
    {
        #region params

        [GameParameters.CustomParameterUI("Enable", newGameOnly = false, toolTip = "Burn time, ignitions and maintenance")]
        public bool enable = true;

        [GameParameters.CustomParameterUI("Reliability progress", newGameOnly = false, toolTip = "Parts have full reliability only after enough experience recovered")]
        public bool reliabilityProgress = true;

        [GameParameters.CustomFloatParameterUI("Starting reliability exponent", newGameOnly = false, displayFormat = "F1", minValue = 2, maxValue = 8, toolTip = "See the .docx in the mod dir")]
        public float startingReliability = 2;

        [GameParameters.CustomFloatParameterUI("Failure on ignition chance (%)", newGameOnly = false, displayFormat = "F1", minValue = 0.01f, maxValue = 1, toolTip = "Chance to cause a failure at max reliability")]
        public float failureOnIgnitionPercent = 0.05f;

        [GameParameters.CustomFloatParameterUI("Ignition failure chance (%)", newGameOnly = false, displayFormat = "F1", minValue = 0.01f, maxValue = 1, toolTip = "Ignition attempt fails (no consequences for the engine) at max reliability")]
        public float ignitionFailurePercent = 0.05f;

        [GameParameters.CustomFloatParameterUI("Part destruction on failure (%)", newGameOnly = false, displayFormat = "F1", minValue = 1, maxValue = 10, toolTip = "Does NOT affect wasted ignitions")]
        public float destructionOnFailurePercent = 5;

        #endregion

        #region logics

        public static bool Enable
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().enable;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().enable = value;
            }
        }

        public static bool ReliabilityProgress
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().reliabilityProgress;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().reliabilityProgress = value;
            }
        }

        public static float StartingReliability
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().startingReliability;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().startingReliability = value;
            }
        }

        #endregion

        #region CustomParameterNode

        public override GameParameters.GameMode GameMode
        {
            get
            {
                return GameParameters.GameMode.ANY;
            }
        }

        public override string Section
        {
            get
            {
                return "Pay to Play";
            }
        }

        public override string DisplaySection
        {
            get
            {
                return Section;
            }
        }

        public override string Title
        {
            get
            {
                return "Get ready for the consequences!";
            }
        }

        public override int SectionOrder
        {
            get
            {
                return 1;
            }
        }

        public override bool HasPresets
        {
            get
            {
                return false;
            }
        }

        #endregion
    }
}
