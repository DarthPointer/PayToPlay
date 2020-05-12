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

        [GameParameters.CustomParameterUI("Should failures happen?", newGameOnly = true)]
        public bool on = true;

        [GameParameters.CustomParameterUI("Reliability progress", newGameOnly = true, toolTip = "Parts have full reliability only after enough experience recovered")]
        public bool reliabilityProgress = true;

        [GameParameters.CustomFloatParameterUI("Starting reliability exponent", newGameOnly = true, displayFormat = "F1", minValue = 2, maxValue = 8, toolTip = "See the .docx in the mod dir")]
        public float startingReliability = 2;

        #endregion

        #region logics

        public static bool On
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().on;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettings>().on = value;
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
