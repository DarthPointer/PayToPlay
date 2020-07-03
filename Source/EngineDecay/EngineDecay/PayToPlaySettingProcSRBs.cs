using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineDecay
{
    public class PayToPlaySettingProcSRBs : GameParameters.CustomParameterNode
    {
        #region params

        [GameParameters.CustomFloatParameterUI("Procedural SRB diameter model margin (%)", newGameOnly = false, displayFormat = "F1", minValue = 0.1f, maxValue = 10)]
        public float procSRBDiameterModelMarginPercent = 4;

        [GameParameters.CustomFloatParameterUI("Procedural SRB thrust model margin (%)", newGameOnly = false, displayFormat = "F1", minValue = 0.1f, maxValue = 20)]
        public float procSRBThrustModelMarginPercent = 10;

        #endregion

        public static float ProcSRBDiameterModelMarginPercent
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingProcSRBs>().procSRBDiameterModelMarginPercent;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingProcSRBs>().procSRBDiameterModelMarginPercent = value;
            }
        }

        public static float ProcSRBThrustModelMarginPercent
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingProcSRBs>().procSRBThrustModelMarginPercent;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingProcSRBs>().procSRBThrustModelMarginPercent = value;
            }
        }

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
                return "Proc SRB handling";
            }
        }

        public override int SectionOrder
        {
            get
            {
                return 3;
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
