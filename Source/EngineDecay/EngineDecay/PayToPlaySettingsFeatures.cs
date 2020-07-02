using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineDecay
{
    public class PayToPlaySettingsFeatures : GameParameters.CustomParameterNode
    {
        #region params

        [GameParameters.CustomParameterUI("Enable", newGameOnly = false, toolTip = "Burn time, ignitions and maintenance")]
        public bool enable = true;

        [GameParameters.CustomParameterUI("Reliability progress", newGameOnly = false, toolTip = "Parts have full reliability only after enough experience recovered")]
        public bool reliabilityProgress = true;

        [GameParameters.CustomFloatParameterUI("Non-standard long time display", newGameOnly = false, toolTip = "Stanrdad format is xx:xx:xx (hh:mm:ss), turn this option on for xh:m:s")]
        public bool useNonstandardLongTimeFormat = false;

        [GameParameters.CustomFloatParameterUI("Enable random warnings before failures", newGameOnly = false, toolTip = "There is a chance that you will get a warning before failure. This chance and failure porximity depend on reliability progress")]
        public bool randomFailureWarningEnable = false;

        #endregion

        #region logics

        public static bool Enable
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsFeatures>().enable;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsFeatures>().enable = value;
            }
        }

        public static bool ReliabilityProgress
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsFeatures>().reliabilityProgress;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsFeatures>().reliabilityProgress = value;
            }
        }

        public static bool UseNonstandardLongTimeFormat
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsFeatures>().useNonstandardLongTimeFormat;
            }
            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsFeatures>().useNonstandardLongTimeFormat = value;
            }
        }

        public static bool RandomFailureWarningEnable
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsFeatures>().randomFailureWarningEnable;
            }
            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsFeatures>().randomFailureWarningEnable = value;
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
