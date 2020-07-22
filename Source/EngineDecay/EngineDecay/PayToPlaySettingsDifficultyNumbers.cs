using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineDecay
{
    public class PayToPlaySettingsDifficultyNumbers : GameParameters.CustomParameterNode
    {
        #region params

        [GameParameters.CustomFloatParameterUI("Starting reliability exponent", newGameOnly = false, displayFormat = "F1", minValue = 2, maxValue = 8, toolTip = "See the .docx or .pdf in the mod dir")]
        public float startingReliability = 2;

        [GameParameters.CustomFloatParameterUI("Random starting reliability exponent bonus limit", newGameOnly = false, displayFormat = "F1", minValue = 0, maxValue = 6)]
        public float randomStartingReliabilityBonusLimit = 2;

        [GameParameters.CustomFloatParameterUI("Failure on ignition chance (%) at max reliability progress", newGameOnly = false, displayFormat = "F2", minValue = 0.01f, maxValue = 1, toolTip = "Chance to cause a failure at max reliability")]
        public float failureOnIgnitionPercent = 0.05f;

        [GameParameters.CustomFloatParameterUI("Ignition failure chance (%) at max reliability progress", newGameOnly = false, displayFormat = "F2", minValue = 0.01f, maxValue = 1, toolTip = "Ignition attempt fails (no consequences for the engine) at max reliability")]
        public float ignitionFailurePercent = 0.05f;

        [GameParameters.CustomFloatParameterUI("Part destruction on failure (%) at max reliability progress", newGameOnly = false, displayFormat = "F1", minValue = 1, maxValue = 10, toolTip = "Does NOT affect wasted ignitions")]
        public float destructionOnFailurePercent = 5;

        [GameParameters.CustomFloatParameterUI("Degradation on use multiplier", newGameOnly = false, displayFormat = "F1", minValue = 0.1f, maxValue = 5, toolTip = "The more it is the faster engines wear out and should be replaced with new ones")]
        public float usageExperienceToDegradationMul = 1;

        [GameParameters.CustomFloatParameterUI("Failure warning chance percent at max reliability", newGameOnly = false, displayFormat = "F1", minValue = 0, maxValue = 100, toolTip = "Failure warning chance multiplier, equals to warn chance precent at max reliability")]
        public float topFailureWarningChancePercent = 90;

        [GameParameters.CustomFloatParameterUI("Failure warning deviation ratio percent at max reliability", newGameOnly = false, displayFormat = "F1", minValue = 1, maxValue = 50, toolTip = "Failure warning deviation multiplier, equals to deviation ratio percent at max reliability. Warnings are disabled for engines with this percent > 50.")]
        public float topFailureWarningDeviationRatioPercent = 5;

        #endregion

        #region logics
        public static float StartingReliability
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsDifficultyNumbers>().startingReliability;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsDifficultyNumbers>().startingReliability = value;
            }
        }

        public static float RandomStartingReliabilityBonusLimit
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsDifficultyNumbers>().randomStartingReliabilityBonusLimit;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsDifficultyNumbers>().randomStartingReliabilityBonusLimit = value;
            }
        }

        public static float FailureOnIgnitionPercent
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsDifficultyNumbers>().failureOnIgnitionPercent;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsDifficultyNumbers>().failureOnIgnitionPercent = value;
            }
        }

        public static float IgnitionFailurePercent
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsDifficultyNumbers>().ignitionFailurePercent;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsDifficultyNumbers>().ignitionFailurePercent = value;
            }
        }

        public static float DestructionOnFailurePercent
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsDifficultyNumbers>().destructionOnFailurePercent;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsDifficultyNumbers>().destructionOnFailurePercent = value;
            }
        }

        public static float UsageExperienceToDegradationMul
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsDifficultyNumbers>().usageExperienceToDegradationMul;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsDifficultyNumbers>().usageExperienceToDegradationMul = value;
            }
        }

        public static float TopFailureWarningChancePercent
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsDifficultyNumbers>().topFailureWarningChancePercent;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsDifficultyNumbers>().topFailureWarningChancePercent = value;
            }
        }

        public static float TopFailureWarningDeviationRatioPercent
        {
            get
            {
                return HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsDifficultyNumbers>().topFailureWarningDeviationRatioPercent;
            }

            set
            {
                HighLogic.CurrentGame.Parameters.CustomParams<PayToPlaySettingsDifficultyNumbers>().topFailureWarningDeviationRatioPercent = value;
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
                return "Numbers";
            }
        }

        public override int SectionOrder
        {
            get
            {
                return 2;
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

