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

        [GameParameters.CustomFloatParameterUI("Failure on ignition chance (%) at max reliability progress", newGameOnly = false, displayFormat = "F1", minValue = 0.01f, maxValue = 1, toolTip = "Chance to cause a failure at max reliability")]
        public float failureOnIgnitionPercent = 0.05f;

        [GameParameters.CustomFloatParameterUI("Ignition failure chance (%) at max reliability progress", newGameOnly = false, displayFormat = "F1", minValue = 0.01f, maxValue = 1, toolTip = "Ignition attempt fails (no consequences for the engine) at max reliability")]
        public float ignitionFailurePercent = 0.05f;

        [GameParameters.CustomFloatParameterUI("Part destruction on failure (%) at max reliability progress", newGameOnly = false, displayFormat = "F1", minValue = 1, maxValue = 10, toolTip = "Does NOT affect wasted ignitions")]
        public float destructionOnFailurePercent = 5;

        [GameParameters.CustomFloatParameterUI("Procedural SRB diameter model margin (%)", newGameOnly = false, displayFormat = "F1", minValue = 0.1f, maxValue = 10)]
        public float procSRBDiameterModelMarginPercent = 4;

        [GameParameters.CustomFloatParameterUI("Procedural SRB thrust model margin (%)", newGameOnly = false, displayFormat = "F1", minValue = 0.1f, maxValue = 20)]
        public float procSRBThrustModelMarginPercent = 10;

        [GameParameters.CustomFloatParameterUI("Non-standard long time display", newGameOnly = false, toolTip = "Stanrdad format is xx:xx:xx (hh:mm:ss), turn this option on for xh:m:s")]
        public bool useNonstandardLongTimeFormat = false;

        [GameParameters.CustomFloatParameterUI("Degradation on use multiplier", newGameOnly = false, displayFormat = "F1", minValue = 0.1f, maxValue = 5, toolTip = "The more it is the faster engines wear out and should be replaced with new ones")]
        public float usageExperienceToDegradationMul = 1;

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

        public static float ProcSRBDiameterModelMarginPercent
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().procSRBDiameterModelMarginPercent;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().procSRBDiameterModelMarginPercent = value;
            }
        }

        public static float ProcSRBThrustModelMarginPercent
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().procSRBThrustModelMarginPercent;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().procSRBThrustModelMarginPercent = value;
            }
        }

        public static bool UseNonstandardLongTimeFormat
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().useNonstandardLongTimeFormat;
            }
            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().useNonstandardLongTimeFormat = value;
            }
        }

        public static float FailureOnIgnitionPercent
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().failureOnIgnitionPercent;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().failureOnIgnitionPercent = value;
            }
        }

        public static float IgnitionFailurePercent
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().ignitionFailurePercent;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().ignitionFailurePercent = value;
            }
        }

        public static float DestructionOnFailurePercent
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().destructionOnFailurePercent;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().destructionOnFailurePercent = value;
            }
        }

        public static float UsageExperienceToDegradationMul
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().usageExperienceToDegradationMul;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().usageExperienceToDegradationMul = value;
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
