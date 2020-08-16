using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EngineDecay
{
    public class EngineDecay : PartModule, IPartMassModifier, IPartCostModifier
    {
        #region fields

        [KSPField(isPersistant = true, guiActive = false)]
        public float topBaseRatedTime = 10;

        [KSPField(isPersistant = true, guiActive = false)]
        public float topMaxRatedTime = 100;

        [KSPField(isPersistant = true, guiActive = false)]
        float r = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        bool reliabilityIsVisible = true;

        [KSPField(isPersistant = true, guiActive = false)]
        float currentBaseRatedTime;

        [KSPField(isPersistant = true, guiActive = false)]
        public float maintenanceAtRatedTimeCoeff = 0.3f;

        [KSPField(isPersistant = true, guiActive = false)]
        public float maxMassRatedTimeCoeff = 0.2f;

        [KSPField(isPersistant = true, guiActive = false)]
        public float maxCostRatedTimeCoeff = 4;

        [KSPField(isPersistant = true, guiActive = false)]
        public int baseIgnitions = 1;

        [KSPField(isPersistant = true, guiActive = false)]
        public int maxIgnitions = 100;

        [KSPField(isPersistant = true, guiActive = false)]
        public float maxMassIgnitionsCoeff = 0.1f;

        [KSPField(isPersistant = true, guiActive = false)]
        public float maxCostIgnitionsCoeff = 0.5f;

        [KSPField(isPersistant = true, guiActive = false)]
        public float maxIgnitionRestoreCostCoeff = 0.1f;

        [KSPField(isPersistant = true, guiActive = false)]
        bool usingMultiModeLogic = false;

        [KSPField(isPersistant = true, guiActive = false)]
        int modesNumber = 1;

        [KSPField(isPersistant = true, guiActive = false)]
        public string decayRates = "";

        List<float> decayRatesList = new List<float>();

        [KSPField(isPersistant = true, guiActive = false)]
        public string ignitionsUsage = "";

        List<bool> ignitionsUsageList = new List<bool>();

        [KSPField(isPersistant = true, guiActive = false)]
        public string ignitionsOnSwitch = "";

        List<bool> ignitionsOnSwitchList = new List<bool>();

        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Extra Burn Time Percent", guiFormat = "D"),
            UI_FloatEdit(scene = UI_Scene.Editor, minValue = 0, maxValue = 100, incrementLarge = 20, incrementSmall = 5, incrementSlide = 1)]
        public float extraBurnTimePercent = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        public float prevEBTP = 0;

        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Extra Ignitions Percent", guiFormat = "D"),
            UI_FloatEdit(scene = UI_Scene.Editor, minValue = 0, maxValue = 100, incrementLarge = 20, incrementSmall = 5, incrementSlide = 1)]
        public float extraIgnitionsPercent = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        public float prevEIP = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        float setBurnTime = 1;

        [KSPField(isPersistant = true, guiActive = false)]
        float usedBurnTime;

        [KSPField(isPersistant = true, guiActive = false)]
        float usageExperienceCoeff = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        int setIgnitions;

        [KSPField(isPersistant = true, guiActive = false)]
        int ignitionsLeft;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Burn Time Used", guiFormat = "F2")]
        string burnTimeIndicator = "";

        [KSPField(isPersistant = true, guiActive = false)]
        int usingTimeFormat = 0;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Ignitions Left", guiFormat = "F2")]
        string ignitionsIndicator = "";

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Reliability Status", guiFormat = "F2")]
        string reliabilityStatus = "nominal";

        [KSPField(isPersistant = true, guiActive = false)]
        int issueCode = 0;

        int modeRunningPrevTick = -1;
        bool wasRailWarpingPrevTick = false;

        [KSPField(isPersistant = true, guiActive = false)]
        public float knownPartCost = -1;

        [KSPField(isPersistant = true, guiActive = false)]
        public float fullPartCost = -1;

        [KSPField(isPersistant = true, guiActive = false)]
        public float targetPartCost = -1;

        [KSPField(isPersistant = true, guiActive = false)]
        public bool subtractResourcesCost = false;

        [KSPField(isPersistant = true, guiActive = false)]
        public bool useSRBCost = false;

        [KSPField(isPersistant = true, guiActive = false)]                                      //use mass and cost logic for Procedural Parts mod
        public bool procPart = false;

        [KSPField(isPersistant = true, guiActive = false)]
        bool newBorn = true;

        [KSPField(isPersistant = true, guiActive = false)]
        bool prevLoadWasInEditor = false;

        [KSPField(isPersistant = true, guiActive = false)]
        bool isKCTBuilt = false;

        [KSPField(isPersistant = true, guiActive = false)]
        float failAtBurnTime = -1;

        [KSPField(isPersistant = true, guiActive = false)]
        float warnAtBurnTime = -1;

        [KSPField(isPersistant = true, guiActive = false)]
        bool autoShutdownOnWarning = false;

        [KSPField(isPersistant = true, guiActive = false)]
        int maintenanceCost = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        int symmetryMaintenanceCost = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        int replaceCost = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        int symmetryReplaceCost = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        int ignitionRestoreCost = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        int symmetryIgnitionRestoreCost = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        public float procSRBDiameter = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        public float procSRBThrust = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        public string procSRBBellName = "";

        bool inEditor = true;
        float ignoreIgnitionTill = 0;
        int ticksTillDisabling = -1;
        float holdIndicatorsTill = 0;
        int partSymmetryCounterpartsCount = -1;

        List<ModuleEngines> decayingEngines;
        MultiModeEngine modeSwitcher;

        PartModule procSRBCylinder;
        PartModule procSRB;

        #endregion

        #region Maintenance

        [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "Maintenance", groupName = "PayToPlayReliability", groupDisplayName = "PayToPlay Reliability")]
        void MaintenanceEvent()
        {
            foreach (Part i in part.symmetryCounterparts)
            {
                EngineDecay engineDecay = i.FindModuleImplementing<EngineDecay>();
                if (i != null)
                {
                    if (baseIgnitions != -1 && !useSRBCost)
                    {
                        engineDecay.CounterpartIgnitionRestore(ignitionRestoreCost);
                    }
                    engineDecay.CounterpartMaintenance(maintenanceCost);
                    engineDecay.CounterpartReplace(maintenanceCost);
                }
                else
                {
                   Lib.LogWarning("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                }
            }

            if (baseIgnitions != -1 && !useSRBCost)
            {
                CounterpartIgnitionRestore(ignitionRestoreCost);
            }
            CounterpartMaintenance(maintenanceCost);                            // the counterpart is the same part for this case, we call it to update symmetry maintenance button
            CounterpartReplace(maintenanceCost);

            Maintenance();

            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);           // the cost has changed!
        }

        void Maintenance()
        {
            usedBurnTime = 0;
            usageExperienceCoeff = 0;

            if (topBaseRatedTime != -1)                                                 // The r parameter is changed by OnStart when getting back to editor
            {
                currentBaseRatedTime = ProbabilityLib.ATangentCumulativePercentArg(r, topBaseRatedTime);
                setBurnTime = currentBaseRatedTime * (1 + extraBurnTimePercent * (topMaxRatedTime / topBaseRatedTime - 1) / 100);
            }

            if (baseIgnitions != -1)
            {
                ignitionsLeft = setIgnitions;

                ignitionRestoreCost = 0;
                Events["IgnitionRestoreEvent"].guiActiveEditor = false;
            }

            UpdateIndicators();

            if (r < 5)
            {
                reliabilityStatus = PayToPlayAddon.RandomStatus("HeavilyReused");
            }
            else
            {
                reliabilityStatus = "nominal";
            }
            issueCode = 0;

            Enable();

            if (maintenanceCost != 0)
            {
                targetPartCost += maintenanceCost;
            }

            UpdateReplaceCost();

            maintenanceCost = 0;
            Events["MaintenanceEvent"].guiActiveEditor = false;
        }

        [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "Symmetry Maintenance", groupName = "PayToPlayReliability", groupDisplayName = "PayToPlay Reliability")]
        void SymmetryMaintenance()
        {
            foreach (Part i in part.symmetryCounterparts)
            {
                EngineDecay engineDecay = i.FindModuleImplementing<EngineDecay>();
                if(i != null)
                {
                    engineDecay.MaintenanceFromCounterpart();
                }
                else
                {
                    Lib.LogWarning("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                }
            }

            MaintenanceFromCounterpart();

            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);           // the cost has changed!
        }

        void MaintenanceFromCounterpart()
        {
            Maintenance();

            symmetryReplaceCost -= symmetryMaintenanceCost;
            UpdateSymmetryReplaceButton();

            symmetryMaintenanceCost = 0;
            Events["SymmetryMaintenance"].guiActiveEditor = false;

            if (baseIgnitions != -1 && !useSRBCost)
            {
                symmetryIgnitionRestoreCost = 0;
                Events["SymmetryIgnitionRestore"].guiActive = false;
            }
        }

        void CounterpartMaintenance(int cost)
        {
            if (symmetryMaintenanceCost != 0)
            {
                symmetryMaintenanceCost -= cost;

                if (symmetryMaintenanceCost != 0)
                {
                    Events["SymmetryMaintenance"].guiName = string.Format("Symmetry Maintenance: {0}", symmetryMaintenanceCost);
                }
                else
                {
                    Events["SymmetryMaintenance"].guiActiveEditor = false;
                }
            }
        }

        int UpdateMaintenanceCost()
        {
            if (!useSRBCost)
            {
                maintenanceCost = (int)(knownPartCost * (1f + extraBurnTimePercent * maxCostRatedTimeCoeff / 100f) * maintenanceAtRatedTimeCoeff * usedBurnTime / setBurnTime);
            }
            else
            {
                if (ignitionsLeft == setIgnitions)
                {
                    maintenanceCost = 0;
                }
                else
                {
                    maintenanceCost = (int)(knownPartCost * maintenanceAtRatedTimeCoeff);
                }
            }

            if (baseIgnitions != -1 && !useSRBCost)
            {
                maintenanceCost += UpdateIgnitionRestoreCost();
            }

            if (maintenanceCost > 0 || issueCode != 0)
            {
                Events["MaintenanceEvent"].guiActiveEditor = true;
                Events["MaintenanceEvent"].guiName = string.Format("Maintenance: {0}", maintenanceCost);
            }
            else
            {
                Events["MaintenanceEvent"].guiActiveEditor = false;
            }

            //maintenanceCost = maintenanceCost < fullPartCost ? maintenanceCost : (int)fullPartCost;

            return maintenanceCost;
        }

        #endregion

        #region Replacement

        [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "Replace", groupName = "PayToPlayReliability", groupDisplayName = "PayToPlay Reliability")]
        void ReplaceEvent()
        {
            foreach (Part i in part.symmetryCounterparts)
            {
                EngineDecay engineDecay = i.FindModuleImplementing<EngineDecay>();
                if (i != null)
                {
                    engineDecay.CounterpartReplace(replaceCost);
                    engineDecay.CounterpartMaintenance(maintenanceCost);
                    if (baseIgnitions != -1 && !useSRBCost)
                    {
                        engineDecay.CounterpartIgnitionRestore(ignitionRestoreCost);
                    }
                }
                else
                {
                    Lib.LogWarning("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                }
            }

            CounterpartReplace(replaceCost);                            // the counterpart is the same part for this case, we call it to update symmetry replace button
            CounterpartMaintenance(maintenanceCost);
            if (baseIgnitions != -1 && !useSRBCost)
            {
                CounterpartIgnitionRestore(ignitionRestoreCost);
            }

            maintenanceCost = 0;
            Events["MaintenanceEvent"].guiActiveEditor = false;

            if (baseIgnitions != -1 && !useSRBCost)
            {
                ignitionRestoreCost = 0;
                Events["IgnitionRestoreEvent"].guiActiveEditor = false;
            }

            Replace();

            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);           // the cost has changed!
        }

        void Replace()
        {
            usedBurnTime = 0;
            usageExperienceCoeff = 0;

            UpdateReliabilityProgress();

            UpdateIndicators();

            issueCode = 0;

            Enable();

            targetPartCost = fullPartCost;

            replaceCost = 0;
            Events["ReplaceEvent"].guiActiveEditor = false;
        }

        [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "Symmetry Replace", groupName = "PayToPlayReliability", groupDisplayName = "PayToPlay Reliability")]
        void SymmetryReplace()
        {
            foreach (Part i in part.symmetryCounterparts)
            {
                EngineDecay engineDecay = i.FindModuleImplementing<EngineDecay>();
                if (i != null)
                {
                    engineDecay.ReplaceFromCounterpart();
                }
                else
                {
                    Debug.LogWarning("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                }
            }

            ReplaceFromCounterpart();

            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);           // the cost has changed!
        }

        void ReplaceFromCounterpart()
        {
            maintenanceCost = 0;
            symmetryMaintenanceCost = 0;
            Events["MaintenanceEvent"].guiActiveEditor = false;
            Events["SymmetryMaintenance"].guiActiveEditor = false;

            if (baseIgnitions != -1 && !useSRBCost)
            {
                ignitionRestoreCost = 0;
                symmetryIgnitionRestoreCost = 0;
                Events["IgnitionRestoreEvent"].guiActiveEditor = false;
                Events["SymmetryIgnitionRestore"].guiActiveEditor = false;
            }

            Replace();

            symmetryReplaceCost = 0;
            Events["SymmetryReplace"].guiActiveEditor = false;
        }

        void CounterpartReplace(int cost)
        {
            if (symmetryReplaceCost != 0)
            {
                symmetryReplaceCost -= cost;

                if (symmetryReplaceCost != 0)
                {
                    Events["SymmetryReplace"].guiName = string.Format("Symmetry Replace: {0}", symmetryReplaceCost);
                }
                else
                {
                    Events["SymmetryReplace"].guiActiveEditor = false;
                }
            }
        }

        bool IsNewAndUpToDate()
        {
            if (procPart)
            {
                if (r != ReliabilityProgress.fetch.CheckProcSRBProgress(part.name, ref procSRBDiameter, ref procSRBThrust, ref procSRBBellName).r)
                {
                    return false;
                }
            }
            else
            {
                if (r != ReliabilityProgress.fetch.GetReliabilityData(part.name).r)
                {
                    return false;
                }
            }

            if (topBaseRatedTime == -1)
            {
                return ignitionsLeft == setIgnitions;
            }
            else
            {
                return usedBurnTime == 0;
            }
        }

        int UpdateReplaceCost()
        {
            replaceCost = (int)(fullPartCost - targetPartCost);

            if (!IsNewAndUpToDate())
            {
                Events["ReplaceEvent"].guiActiveEditor = true;
                Events["ReplaceEvent"].guiName = string.Format("Replace: {0}", replaceCost);
            }

            return replaceCost;
        }

        void UpdateSymmetryReplaceButton()
        {
            if (symmetryReplaceCost > 0 || issueCode != 0)
            {
                Events["SymmetryReplace"].guiActiveEditor = true;
                Events["SymmetryReplace"].guiName = string.Format("Replace: {0}", symmetryReplaceCost);
            }
            else
            {
                Events["SymmetryReplace"].guiActiveEditor = false;
            }
        }

        #endregion

        #region Ignition Restore

        [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "Restore Ignitions", groupName = "PayToPlayReliability", groupDisplayName = "PayToPlay Reliability")]
        void IgnitionRestoreEvent()
        {
            foreach (Part i in part.symmetryCounterparts)
            {
                EngineDecay engineDecay = i.FindModuleImplementing<EngineDecay>();
                if (i != null)
                {
                    engineDecay.CounterpartIgnitionRestore(ignitionRestoreCost);
                }
                else
                {
                    Lib.LogWarning("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                }
            }

            Lib.Log($"Separately Ign-Restored part keeps Symm Ign Restore Cost {symmetryIgnitionRestoreCost}, Ign Restore Cost is {ignitionRestoreCost}");
            CounterpartIgnitionRestore(ignitionRestoreCost);
            Lib.Log($"Resulting Symm Ign Restore Cost is {symmetryIgnitionRestoreCost}");

            IgnitionRestore();

            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);           // the cost has changed!
        }

        void IgnitionRestore()
        {
            if (ignitionRestoreCost != 0)
            {
                ignitionsLeft = setIgnitions;

                UpdateIndicators();

                if (issueCode == 2)
                {
                    issueCode = 0;
                    reliabilityStatus = "nominal";
                    Enable();
                }

                targetPartCost = ignitionRestoreCost;
                Lib.Log($"Restored ignitions, target part cost set to {targetPartCost}");

                Lib.Log("Hiding Ignition Restore");
                Events["IgnitionRestoreEvent"].guiActiveEditor = false;
                ignitionRestoreCost = 0;

                UpdateMaintenanceCost();
                UpdateReplaceCost();
            }
        }

        [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "Symm Ign Restore", groupName = "PayToPlayReliability", groupDisplayName = "PayToPlay Reliability")]
        void SymmetryIgnitionRestore()
        {
            foreach (Part i in part.symmetryCounterparts)
            {
                EngineDecay engineDecay = i.FindModuleImplementing<EngineDecay>();
                if (i != null)
                {
                    engineDecay.IgnitionRestoreFromCounterpart();
                }
                else
                {
                    Debug.LogWarning("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                }
            }

            
            IgnitionRestoreFromCounterpart();

            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);           // the cost has changed!
        }

        void IgnitionRestoreFromCounterpart()
        {
            CounterpartMaintenance(symmetryIgnitionRestoreCost);
            CounterpartReplace(symmetryIgnitionRestoreCost);

            IgnitionRestore();

            symmetryIgnitionRestoreCost = 0;
            Lib.Log("Hiding Symm Ign Restore");
            Events["SymmetryIgnitionRestore"].guiActiveEditor = false;
        }

        void CounterpartIgnitionRestore(int cost)
        {
            CounterpartMaintenance(cost);
            CounterpartReplace(cost);

            if (symmetryIgnitionRestoreCost != 0)
            {
                symmetryIgnitionRestoreCost -= cost;

                if (symmetryIgnitionRestoreCost != 0)
                {
                    Events["SymmetryIgnitionRestore"].guiName = string.Format("Symm Ign Restore: {0}", symmetryIgnitionRestoreCost);
                }
                else
                {
                    Events["SymmetryIgnitionRestore"].guiActiveEditor = false;
                }
            }
        }

        int UpdateIgnitionRestoreCost()
        {
            ignitionRestoreCost = (int)(knownPartCost * (1 + extraIgnitionsPercent/100) * (1 - ignitionsLeft/setIgnitions) * maxIgnitionRestoreCostCoeff);

            if (ignitionRestoreCost > 0 || issueCode == 2)
            {
                Events["IgnitionRestoreEvent"].guiActiveEditor = true;
                Events["IgnitionRestoreEvent"].guiName = string.Format("Restore Ignitions: {0}", ignitionRestoreCost);
            }
            else
            {
                Events["IgnitionRestoreEvent"].guiActiveEditor = false;
            }

            return ignitionRestoreCost;
        }

        void UpdateSymmetryIgnitionRestoreButton()
        {
            if (symmetryIgnitionRestoreCost > 0 || issueCode == 2)
            {
                Events["SymmetryIgnitionRestore"].guiActiveEditor = true;
                Events["SymmetryIgnitionRestore"].guiName = string.Format("Symm Ign Restore: {0}", symmetryIgnitionRestoreCost);
            }
            else
            {
                Events["SymmetryIgnitionRestore"].guiActiveEditor = false;
            }
        }

        #endregion

        #region proc SRB events

        [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "Set as a New Model", groupName = "PayToPlayReliability", groupDisplayName = "PayToPlay Reliability")]
        void SetAsANewProcSRBModel()
        {
            ReliabilityProgress.fetch.CreateModel(part.name, r, procSRBDiameter, procSRBThrust, procSRBBellName);
            Events["SetAsANewProcSRBModel"].guiActiveEditor = false;

            foreach (Part i in part.symmetryCounterparts)
            {
                if (i.FindModuleImplementing<EngineDecay>() != null)
                {
                    i.FindModuleImplementing<EngineDecay>().Events["SetAsANewProcSRBModel"].guiActiveEditor = false;
                    i.FindModuleImplementing<EngineDecay>().r = r;
                }
                else
                {
                    Lib.LogWarning("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                }
            }
        }

        #endregion

        #region Indicators

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Switch Time Format", groupName = "PayToPlayReliability", groupDisplayName = "PayToPlay Reliability")]
        void SwitchTimeFormat()
        {
            if (usingTimeFormat == 0 && PayToPlaySettingsFeatures.UseNonstandardLongTimeFormat)
            {
                usingTimeFormat = 2;
            }
            else if (usingTimeFormat == 0 && !PayToPlaySettingsFeatures.UseNonstandardLongTimeFormat)
            {
                usingTimeFormat = 1;
            }
            else //if (usingTimeFormat == 2 || usingTimeFormat == 1)
            {
                usingTimeFormat = 0;
            }

            UpdateIndicators();
        }

        void UpdateIndicators()
        {
            if (reliabilityIsVisible || !PayToPlaySettingsFeatures.HideStartingReliability)
            {
                burnTimeIndicator = string.Format("{0} / {1}", Lib.Format(usedBurnTime, usingTimeFormat), Lib.Format(setBurnTime, usingTimeFormat));
            }
            else
            {
                burnTimeIndicator = "Need to recover usage experience";
            }

            ignitionsIndicator = string.Format("{0} / {1}", ignitionsLeft, setIgnitions);

            holdIndicatorsTill = Time.time + 0.5f;

            if (topBaseRatedTime != -1 && PayToPlaySettingsDifficultyNumbers.TopFailureWarningDeviationRatioPercent * (float)Math.Pow(9 - r, 2) <= 50 && PayToPlaySettingsFeatures.RandomFailureWarningEnable)
            {
                Events["ToggleAutoShutdownOnWarning"].guiActiveEditor = true;
                Events["ToggleAutoShutdownOnWarning"].guiActive = true;
                Events["ToggleAutoShutdownOnWarning"].guiName = "Autoshutdown on Warning: " + autoShutdownOnWarning;
            }
            else
            {
                Events["ToggleAutoShutdownOnWarning"].guiActiveEditor = false;
                Events["ToggleAutoShutdownOnWarning"].guiActive = false;
            }
        }

        #endregion

        #region Autoshutdown Button

        [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "Autoshutdown on Warning", groupName = "PayToPlayReliability", groupDisplayName = "PayToPlay Reliability")]
        void ToggleAutoShutdownOnWarning()
        {
            autoShutdownOnWarning = !autoShutdownOnWarning;
            Events["ToggleAutoShutdownOnWarning"].guiName = "Autoshutdown on Warning: " + autoShutdownOnWarning;
        }

        #endregion

        #region Game-called Methods

        public override void OnStart(StartState state)
        {
            if (PayToPlaySettingsFeatures.Enable)
            {
                decayingEngines = part.FindModulesImplementing<ModuleEngines>();
                modeSwitcher = part.FindModuleImplementing<MultiModeEngine>();

                modesNumber = decayingEngines.Count();

                Lib.Log($"EngineDecay Module has found {modesNumber} engine modules");

                if (decayRates.Length != 0)
                {
                    foreach (string i in decayRates.Split(';'))
                    {
                        decayRatesList.Add(float.Parse(i));
                    }
                }

                if (ignitionsUsage.Length != 0)
                {
                    foreach (string i in ignitionsUsage.Split(';'))
                    {
                        ignitionsUsageList.Add(int.Parse(i) != 0);
                    }
                }

                if (ignitionsOnSwitch.Length != 0)
                {
                    int c = 0;
                    foreach (string i in ignitionsOnSwitch.Split(';'))
                    {
                        if (c % (modesNumber + 1) == 0)
                        {
                            ignitionsOnSwitchList.Add(false);
                            c++;
                        }
                        ignitionsOnSwitchList.Add(int.Parse(i) != 0);
                        c++;
                    }
                    ignitionsOnSwitchList.Add(false);
                }

                //if engine multimode data is consistent, we will access features for mulit-mode engines
                usingMultiModeLogic = decayRatesList.Count() == modesNumber && ignitionsUsageList.Count() == modesNumber && ignitionsOnSwitchList.Count() == modesNumber * modesNumber;

                if (modesNumber == 0)
                {
                    Lib.LogWarning($"EngineDecay could not find engine modules at part {part.name}");           // Need full stop flag
                    return;
                }
                else if (modesNumber != 1 && !usingMultiModeLogic)
                {
                    Lib.LogWarning($"EngineDecay found multiple engine modules at {part.name} but is not properly configured for {modesNumber} modes, fallback to singlemode logic");
                }
                else
                {
                    Lib.Log($"EngineDecay found {modesNumber} modes at {part.name}");
                }

                if (topBaseRatedTime == -1)
                {
                    Fields["extraBurnTimePercent"].guiActiveEditor = false;
                    Fields["extraBurnTimePercent"].guiActive = false;

                    Fields["burnTimeIndicator"].guiActiveEditor = false;
                    Fields["burnTimeIndicator"].guiActive = false;

                    Events["SwitchTimeFormat"].guiActiveEditor = false;
                    Events["SwitchTimeFormat"].guiActive = false;
                }

                if (baseIgnitions == -1 || baseIgnitions == maxIgnitions)
                {
                    Fields["extraIgnitionsPercent"].guiActiveEditor = false;
                    Fields["extraIgnitionsPercent"].guiActive = false;

                    Fields["ignitionsIndicator"].guiActiveEditor = false;
                    Fields["ignitionsIndicator"].guiActive = false;
                }

                if (!PayToPlaySettingsFeatures.RandomFailureWarningEnable)
                {
                    autoShutdownOnWarning = false;
                }

                if (state == StartState.Editor)
                {
                    inEditor = true;

                    r -= usageExperienceCoeff * PayToPlaySettingsDifficultyNumbers.UsageExperienceToDegradationMul;

                    if (procPart)
                    {
                        procSRBCylinder = part.Modules["ProceduralShapeCylinder"];
                        procSRB = part.Modules["ProceduralSRB"];

                        if ((procSRBCylinder == null) || (procSRB == null))
                        {
                            Lib.LogWarning("An EngineDecay module marked as a one for ProceduralParts SRB could not find relevant modules. Switched to non-procedural logic");
                            procPart = false;
                        }
                        else
                        {
                            Lib.Log($"EngineDecay started initialization for procedural SRB at part {part.name}");

                            procSRBCylinder.Fields["diameter"].uiControlEditor.onFieldChanged += ProcUpdateDiameter;
                            procSRB.Fields["thrust"].uiControlEditor.onFieldChanged += ProcUpdateThrust;
                            procSRB.Fields["selectedBellName"].uiControlEditor.onFieldChanged += ProcUpdateBellName;

                            ProcUpdateDiameter(procSRBCylinder.Fields["diameter"], null);
                            ProcUpdateBellName(procSRB.Fields["selectedBellName"], null);
                            ProcUpdateThrust(procSRB.Fields["thrust"], null);

                            Lib.Log($"EngineDecay initialization success for procedural SRB at part {part.name}");
                        }
                    }

                    UpdateReplaceCost();

                    if (!isKCTBuilt || replaceCost == 0 || newBorn)
                    {
                        UpdateReliabilityProgress();

                        Lib.Log($"EngineDecay has automatically updated to last reliability generation at part {part.name}");
                    }

                    failAtBurnTime = -1;

                    if (part.PartActionWindow != null)
                    {
                        part.PartActionWindow.displayDirty = true;
                    }
                    //GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
                    prevLoadWasInEditor = true;
                }
                else
                {
                    inEditor = false;

                    ignoreIgnitionTill = Time.time + 0.5f;

                    if (issueCode != 0)
                    {
                        Disable();
                    }
                    else
                    {
                        if (replaceCost == 0 && (prevLoadWasInEditor || isKCTBuilt))        // if pLWIE is false and iKCTB is true, then we have been recovered and rolled out without visiting editor
                        {
                            UpdateReliabilityProgress();

                            Lib.Log($"EngineDecay has automatically updated to last reliability generation at part {part.name}");
                        }
                        if (failAtBurnTime == -1 && topBaseRatedTime != -1)
                        {
                            SetReliabilityData();
                        }
                    }

                    symmetryIgnitionRestoreCost = -1;
                    symmetryMaintenanceCost = -1;
                    symmetryReplaceCost = -1;

                    prevLoadWasInEditor = false;
                }

                Lib.Log($"EngineDecay at {part.name} is finishing OnStart, about to update indicators");

                UpdateIndicators();
            }
            else
            {
                Fields["extraBurnTimePercent"].guiActiveEditor = false;
                Fields["extraBurnTimePercent"].guiActive = false;

                Fields["burnTimeIndicator"].guiActiveEditor = false;
                Fields["burnTimeIndicator"].guiActive = false;


                Fields["extraIgnitionsPercent"].guiActiveEditor = false;
                Fields["extraIgnitionsPercent"].guiActive = false;

                Fields["ignitionsIndicator"].guiActiveEditor = false;
                Fields["ignitionsIndicator"].guiActive = false;


                Fields["reliabilityStatus"].guiActiveEditor = false;
                Fields["reliabilityStatus"].guiActive = false;


                Events["MaintenanceEvent"].guiActiveEditor = false;
                Events["MaintenanceEvent"].guiActive = false;
            }

            isKCTBuilt = false;                                     // If we don't do this, this flag will be passed to a saved ship
        }

        public void Update()
        {
            if (PayToPlaySettingsFeatures.Enable)
            {
            }
        }

        void ReviewSymmetryCosts(List<Part> counterparts)
        {
            if (baseIgnitions != -1 && !useSRBCost)
            {
                symmetryIgnitionRestoreCost = UpdateIgnitionRestoreCost();
            }
            symmetryMaintenanceCost = UpdateMaintenanceCost();
            symmetryReplaceCost = UpdateReplaceCost();

            foreach (Part i in counterparts)
            {
                EngineDecay engineDecay = i.FindModuleImplementing<EngineDecay>();
                if (engineDecay != null)
                {
                    if (baseIgnitions != -1 && !useSRBCost)
                    {
                        symmetryIgnitionRestoreCost += engineDecay.UpdateIgnitionRestoreCost();
                    }
                    symmetryMaintenanceCost += engineDecay.UpdateMaintenanceCost();
                    symmetryReplaceCost += engineDecay.UpdateReplaceCost();
                }
                else
                {
                    Debug.LogWarning("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                }
            }

            foreach (Part i in counterparts)
            {
                EngineDecay engineDecay = i.FindModuleImplementing<EngineDecay>();
                if (engineDecay != null)
                {
                    if (baseIgnitions != -1 && !useSRBCost)
                    {
                        engineDecay.symmetryIgnitionRestoreCost = symmetryIgnitionRestoreCost;
                    }
                    engineDecay.symmetryMaintenanceCost = symmetryMaintenanceCost;
                    engineDecay.symmetryReplaceCost = symmetryReplaceCost;

                    if (symmetryMaintenanceCost > 0)
                    {
                        engineDecay.Events["SymmetryMaintenance"].guiName = string.Format("Symmetry Maintenance: {0}", symmetryMaintenanceCost);
                        engineDecay.Events["SymmetryMaintenance"].guiActiveEditor = true;
                    }
                    if (symmetryReplaceCost > 0)
                    {
                        engineDecay.Events["SymmetryReplace"].guiName = string.Format("Symmetry Replace: {0}", symmetryReplaceCost);
                        engineDecay.Events["SymmetryReplace"].guiActiveEditor = true;
                    }
                    if (symmetryIgnitionRestoreCost > 0)
                    {
                        engineDecay.Events["SymmetryIgnitionRestore"].guiName = string.Format("Symm Ign Restore: {0}", symmetryIgnitionRestoreCost);
                        engineDecay.Events["SymmetryIgnitionRestore"].guiActiveEditor = true;
                    }
                }
                else
                {
                    Debug.LogWarning("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                }
            }

            if (symmetryMaintenanceCost > 0)
            {
                Events["SymmetryMaintenance"].guiName = string.Format("Symmetry Maintenance: {0}", symmetryMaintenanceCost);
                Events["SymmetryMaintenance"].guiActiveEditor = true;
            }
            if (symmetryReplaceCost > 0)
            {
                Events["SymmetryReplace"].guiName = string.Format("Symmetry Replace: {0}", symmetryReplaceCost);
                Events["SymmetryReplace"].guiActiveEditor = true;
            }
            if (symmetryIgnitionRestoreCost > 0)
            {
                Events["SymmetryIgnitionRestore"].guiName = string.Format("Symm Ign Restore: {0}", symmetryIgnitionRestoreCost);
                Events["SymmetryIgnitionRestore"].guiActiveEditor = true;
            }

            partSymmetryCounterpartsCount = counterparts.Count();

            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
        }

        public void FixedUpdate()
        {
            if (PayToPlaySettingsFeatures.Enable)
            {
                if (!inEditor && newBorn)
                {
                    throw new Exception("EngineDecay MODULE thinks it is not in editor but not initialized yet");
                }

                newBorn = false;

                if (inEditor)
                {
                    List<Part> counterparts = part.symmetryCounterparts;
                    if (counterparts.Count() != 0)
                    {
                        if (symmetryMaintenanceCost == -1 || partSymmetryCounterpartsCount != counterparts.Count())
                        {
                            Lib.Log("Rviewing symmetry costs as symmetry counterparts count has changed or costs not set yet");
                            ReviewSymmetryCosts(counterparts);
                        }
                    }
                    else
                    {
                        if (partSymmetryCounterpartsCount > 0)            // If a part was started in a symmetry group
                        {
                            symmetryIgnitionRestoreCost = -1;
                            symmetryMaintenanceCost = -1;
                            symmetryReplaceCost = -1;

                            Lib.Log("Detected symmetry disconnection, disabling symmetry-related buttons");
                            Events["SymmetryIgnitionRestore"].guiActiveEditor = false;
                            Events["SymmetryMaintenance"].guiActiveEditor = false;
                            Events["SymmetryReplace"].guiActiveEditor = false;

                            part.PartActionWindow.displayDirty = true;

                            partSymmetryCounterpartsCount = 0;
                        }
                        if (partSymmetryCounterpartsCount == -1)        // -1 is initial value, then it is set to 0/positve
                        {
                            //UpdateIgnitionRestoreCost();          called in UpdateMaintenanceCost
                            UpdateMaintenanceCost();
                            UpdateReplaceCost();

                            partSymmetryCounterpartsCount = 0;
                        }
                    }

                    if (prevEBTP != extraBurnTimePercent || prevEIP != extraIgnitionsPercent)
                    {
                        prevEBTP = extraBurnTimePercent;
                        prevEIP = extraIgnitionsPercent;

                        knownPartCost = -1;

                        ReplaceEvent();
                    }
                }

                if (!inEditor)
                {
                    bool railWarping = IsRailWarping();

                    int runningMode = RunningMode();

                    if (!railWarping)
                    {
                        if (wasRailWarpingPrevTick)
                        {
                            ignoreIgnitionTill = Time.time + 0.5f;
                        }

                        if (runningMode != -1)
                        {
                            if (topBaseRatedTime != -1)
                            {
                                if (!usingMultiModeLogic)
                                {
                                    usedBurnTime += TimeWarp.fixedDeltaTime;
                                }
                                else
                                {
                                    usedBurnTime += TimeWarp.fixedDeltaTime * decayRatesList[runningMode];
                                }

                                if (usedBurnTime <= setBurnTime)
                                {
                                    usageExperienceCoeff = 0.1f * usedBurnTime / setBurnTime;
                                }
                            }
                            else
                            {
                                usageExperienceCoeff = 0.3f;
                            }
                            targetPartCost = 0;
                        }

                        if (topBaseRatedTime != -1)
                        {
                            if (usedBurnTime > failAtBurnTime && issueCode == 0)
                            {
                                Failure();

                                usageExperienceCoeff = 0.3f;
                            }

                            if (PayToPlaySettingsFeatures.RandomFailureWarningEnable)
                            {
                                if (usedBurnTime > warnAtBurnTime)
                                {
                                    if (reliabilityStatus == "nominal")
                                    {
                                        reliabilityStatus = PayToPlayAddon.RandomStatus("PoorEngineCondition");
                                        part.SetHighlightType(Part.HighlightType.AlwaysOn);
                                        part.SetHighlightColor(new Color(1, 1, 0));
                                        part.SetHighlight(true, false);

                                        ScreenMessages.PostScreenMessage("Bad engine telemetry, get ready for a failure!");

                                        if (autoShutdownOnWarning)
                                        {
                                            CutoffOnFailure();
                                        }
                                    }
                                }
                            }
                            else if (usedBurnTime > setBurnTime)
                            {
                                if (reliabilityStatus == "nominal")
                                {
                                    reliabilityStatus = PayToPlayAddon.RandomStatus("PoorEngineCondition");
                                }
                            }

                            if (ticksTillDisabling > 0)
                            {
                                CutoffOnFailure();
                                ticksTillDisabling--;
                            }

                            if (ticksTillDisabling == 0)
                            {
                                Disable();
                                ticksTillDisabling = -1;
                            }
                        }

                        if (Time.time >= ignoreIgnitionTill && baseIgnitions != -1 && issueCode == 0)
                        {
                            checkIgnition();
                        }
                    }

                    if (issueCode == 0 && baseIgnitions != -1)
                    {
                        LastIgnitionCheck();
                    }

                    if (Time.time > holdIndicatorsTill)
                    {
                        UpdateIndicators();
                    }

                    wasRailWarpingPrevTick = railWarping;
                    modeRunningPrevTick = runningMode;
                }
            }
        }

        #region mass and cost modifiers implementation (game-called too)

        float IPartMassModifier.GetModuleMass(float defaultMass, ModifierStagingSituation sit)
        {
            return defaultMass * (extraBurnTimePercent * maxMassRatedTimeCoeff + extraIgnitionsPercent * maxMassIgnitionsCoeff) / 100;
        }

        ModifierChangeWhen IPartMassModifier.GetModuleMassChangeWhen()
        {
            return ModifierChangeWhen.CONSTANTLY;
        }

        float IPartCostModifier.GetModuleCost(float defaultCost, ModifierStagingSituation sit)
        {
            if(procPart)
            {
                defaultCost = 0;

                foreach (IPartCostModifier i in part.FindModulesImplementing<IPartCostModifier>())
                {
#pragma warning disable CS0252 // Yes, I compare references
                    if (i != this)
#pragma warning restore CS0252
                    {
                        defaultCost += i.GetModuleCost(0, sit);                                     //We assume there are no other cost modifiers besides procedural-related and EngineDecay
                    }
                }
            }

            if (defaultCost != 0)
            {
                if (knownPartCost == -1)
                {
                    if (subtractResourcesCost)
                    {
                        foreach (PartResource i in part.Resources.ToList())
                        {
                            defaultCost -= (float)i.maxAmount * PartResourceLibrary.Instance.GetDefinition(i.resourceName).unitCost;
                        }
                    }

                    knownPartCost = defaultCost;
                    fullPartCost = knownPartCost * (1 + maxCostRatedTimeCoeff * extraBurnTimePercent / 100 + maxCostIgnitionsCoeff * extraIgnitionsPercent / 100);
                    targetPartCost = fullPartCost;
                    replaceCost = 0;
                }
            }

            if (newBorn)
            {
                return 0;
            }
            else
            {
                return targetPartCost - knownPartCost;
            }
        }

        ModifierChangeWhen IPartCostModifier.GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.CONSTANTLY;
        }

        #endregion

        #endregion

        #region internal methods

        int RunningMode()
        {
            int mode = -1, c = 0;
            foreach (ModuleEngines i in decayingEngines)
            {
                if(i.currentThrottle > 0 && i.EngineIgnited)
                {
                    mode = c;
                }
                c++;
            }

            return mode;
        }

        bool IsRailWarping()
        {
            return TimeWarp.WarpMode == TimeWarp.Modes.HIGH && TimeWarp.CurrentRate != 1;
        }

        void checkIgnition()
        {
            if (!usingMultiModeLogic)
            {
                if (RunningMode() != -1 && modeRunningPrevTick == -1)
                {
                    ignitionsLeft--;

                    float luck = UnityEngine.Random.Range(0f, 1f);
                    Lib.Log($"Failure on ignition chance is {PayToPlaySettingsDifficultyNumbers.FailureOnIgnitionPercent / 100 * Math.Pow(9 - r, 3.2)}");
                    Lib.Log($"Noncritical gnition failure chance is {PayToPlaySettingsDifficultyNumbers.IgnitionFailurePercent / 100 * Math.Pow(9 - r, 3.2)}");
                    if (luck < PayToPlaySettingsDifficultyNumbers.FailureOnIgnitionPercent / 100 * Math.Pow(9 - r, 3.2))
                    {
                        Failure();

                        if (topBaseRatedTime != -1)
                        {
                            usageExperienceCoeff = 0.3f;
                        }
                        else
                        {
                            usageExperienceCoeff = 0.5f;
                        }
                    }
                    else if(luck < (PayToPlaySettingsDifficultyNumbers.FailureOnIgnitionPercent + PayToPlaySettingsDifficultyNumbers.IgnitionFailurePercent) / 100 * Math.Pow(9 - r, 3.2))
                    {
                        FlightLogger.fetch?.LogEvent(string.Format("Bad ignition of {0}, shutdown performed to prevent consequences", part.name));
                        CutoffOnFailure();
                    }
                }
            }
            else
            {
                int runningMode = RunningMode();
                if(runningMode != -1)
                {
                    if(SwitchNeedsIgnition(modeRunningPrevTick, runningMode))
                    {
                        ignitionsLeft--;

                        float luck = UnityEngine.Random.Range(0f, 1f);
                        if (luck < PayToPlaySettingsDifficultyNumbers.FailureOnIgnitionPercent / 100 * Math.Pow(9 - r, 3.2))
                        {
                            Failure();

                            usageExperienceCoeff = 0.3f;
                        }
                        else if (luck < (PayToPlaySettingsDifficultyNumbers.FailureOnIgnitionPercent + PayToPlaySettingsDifficultyNumbers.IgnitionFailurePercent) / 100 * Math.Pow(9 - r, 3.2))
                        {
                            FlightLogger.fetch?.LogEvent(string.Format("Bad ignition of {0}, shutdown performed to prevent consequences", part.name));
                            CutoffOnFailure();
                        }
                    }
                }
            }
        }

        void Failure()
        {
            Lib.Log($"Failing {part.name}, destruction chance is {PayToPlaySettingsDifficultyNumbers.DestructionOnFailurePercent / 100 * (9 - r)}");

            if(UnityEngine.Random.Range(0f, 1f) < PayToPlaySettingsDifficultyNumbers.DestructionOnFailurePercent/100 * (9 - r))
            {
                BadaBoom();
            }
            else
            {
                FlightLogger.fetch?.LogEvent(string.Format("{0} failed", part.name));
            }

            ticksTillDisabling = 5;

            CutoffOnFailure();

            if (PayToPlaySettingsFeatures.JokesInsteadOfFailedStatus)
            {
                reliabilityStatus = PayToPlayAddon.RandomStatus("Failed");
            }
            else
            {
                reliabilityStatus = "failed";
            }

            issueCode = 1;
        }

        void CutoffOnFailure()
        {
            foreach (ModuleEngines i in decayingEngines)
            {
                i.Flameout("failure");
                i.Shutdown();
                i.EngineIgnited = false;
                i.currentThrottle = 0;
                i.stagingEnabled = false;
            }
        }

        void Disable()
        {
            foreach (ModuleEngines i in decayingEngines)
            {
                i.Shutdown();
                i.currentThrottle = 0;
                i.isEnabled = false;
                i.stagingEnabled = false;
            }

            if (modeSwitcher != null)
            {
                modeSwitcher.isEnabled = false;
            }
        }

        void Enable()
        {
            foreach (ModuleEngines i in decayingEngines)
            {
                i.isEnabled = true;
                i.currentThrottle = 0;
                i.stagingEnabled = true;
            }

            if (modeSwitcher != null)
            {
                modeSwitcher.isEnabled = true;
            }
        }

        void BadaBoom()
        {
            FlightLogger.fetch?.LogEvent(string.Format("A critical maulfunction has destroyed {0}", part.name));        //placeholder using internal part name
            part.explode();
        }

        void LastIgnitionCheck()
        {
            if (!usingMultiModeLogic)
            {
                if (modeRunningPrevTick != -1 && RunningMode() == -1 && ignitionsLeft == 0)
                {
                    Disable();

                    reliabilityStatus = "out of ignitions";
                    issueCode = 2;
                }
            }
            else
            {
                if(ignitionsLeft == 0)
                {
                    int runningMode = RunningMode();

                    if(runningMode == 0)
                    {
                        modeSwitcher.isEnabled = !SwitchNeedsIgnition(0, 1);            //we are allowed to switch mode if it does not need an ignition
                    }

                    if(runningMode == 1)
                    {
                        modeSwitcher.isEnabled = !SwitchNeedsIgnition(1, 0);
                    }

                    if(runningMode == -1)
                    {
                        if (ignitionsUsageList[0] && ignitionsUsageList[1])
                        {
                            Disable();

                            reliabilityStatus = "out of ignitions";

                            issueCode = 2;
                        }
                        else if (ignitionsUsageList[0])
                        {
                            modeSwitcher.SetSecondary(true);
                            modeSwitcher.isEnabled = false;
                        }
                        else if (ignitionsUsageList[1])
                        {
                            modeSwitcher.SetPrimary(true);
                            modeSwitcher.isEnabled = false;
                        }
                    }

                    int c = 0;
                    foreach(ModuleEngines i in decayingEngines)
                    {
                        if(SwitchNeedsIgnition(runningMode, c))
                        {
                            i.isEnabled = false;
                        }
                    }
                }
            }
        }

        bool SwitchNeedsIgnition(int fromMode, int toMode)
        {
            if (fromMode == -1)
            {
                return ignitionsUsageList[toMode];
            }
            else
            {
                return ignitionsOnSwitchList[fromMode * modesNumber + toMode];
            }
        }

        void UpdateReliabilityProgress()
        {
            if (!procPart)
            {
                r = ReliabilityProgress.fetch.GetReliabilityData(part.name).r;
                reliabilityIsVisible = ReliabilityProgress.fetch.GetReliabilityData(part.name).reliabilityIsVisible;
            }
            else
            {
                r = ReliabilityProgress.fetch.CheckProcSRBProgress(part.name, ref procSRBDiameter, ref procSRBThrust, ref procSRBBellName).r;
                reliabilityIsVisible = ReliabilityProgress.fetch.CheckProcSRBProgress(part.name, ref procSRBDiameter, ref procSRBThrust, ref procSRBBellName).reliabilityIsVisible;

                if (r == -1)
                {
                    if (PayToPlaySettingsFeatures.ReliabilityProgress)
                    {
                        r = PayToPlaySettingsDifficultyNumbers.StartingReliability;
                        if (PayToPlaySettingsFeatures.RandomStartingReliability)
                        {
                            r += UnityEngine.Random.Range(0f, 1f) * PayToPlaySettingsDifficultyNumbers.RandomStartingReliabilityBonusLimit;
                        }

                        r = Math.Min(r, 8);
                    }
                    else
                    {
                        r = 8;
                    }

                    reliabilityIsVisible = !PayToPlaySettingsFeatures.HideStartingReliability;

                    Events["SetAsANewProcSRBModel"].guiActiveEditor = true;
                }
                else
                {
                    Events["SetAsANewProcSRBModel"].guiActiveEditor = false;
                }
            }

            if (r < 5)
            {
                reliabilityStatus = PayToPlayAddon.RandomStatus("LowReliabilityModel");
            }
            else
            {
                reliabilityStatus = "nominal";
            }

            if (topBaseRatedTime != -1)
            {
                currentBaseRatedTime = ProbabilityLib.ATangentCumulativePercentArg(r, topBaseRatedTime);
                setBurnTime = currentBaseRatedTime * (1 + extraBurnTimePercent * (topMaxRatedTime / topBaseRatedTime - 1) / 100);

                usedBurnTime = 0;
            }

            if (baseIgnitions != -1)
            {
                setIgnitions = (int)(baseIgnitions + (maxIgnitions - baseIgnitions) * (extraIgnitionsPercent / 100));
                ignitionsLeft = setIgnitions;
            }
        }

        void SetReliabilityData()
        {
            failAtBurnTime = ProbabilityLib.ATangentRandom(r, topBaseRatedTime) * (1 + extraBurnTimePercent * (topMaxRatedTime / topBaseRatedTime - 1) / 100);

            if (PayToPlaySettingsFeatures.RandomFailureWarningEnable)
            {
                float failureWarningDevationRatioPercent = PayToPlaySettingsDifficultyNumbers.TopFailureWarningDeviationRatioPercent * (float)Math.Pow(9 - r, 2);
                if (failureWarningDevationRatioPercent <= 50)
                {
                    if (UnityEngine.Random.value < PayToPlaySettingsDifficultyNumbers.TopFailureWarningChancePercent / (float)Math.Pow(9 - r, 2))
                    {
                        warnAtBurnTime = failAtBurnTime * (1 - UnityEngine.Random.value * failureWarningDevationRatioPercent / 100);
                    }
                    else
                    {
                        warnAtBurnTime = float.PositiveInfinity;                // bad luck
                    }
                }
                else
                {
                    warnAtBurnTime = float.PositiveInfinity;                    // warnings are not available for this engine
                }
            }
        }
        #endregion

        #region ProcSRB Internal Methods

        public void ProcUpdateDiameter(BaseField diameter, object obj)
        {
            procSRBDiameter = (float)diameter.GetValue(procSRBCylinder);

            if (obj != null)
            {
                UpdateModelState();

                foreach (Part i in part.symmetryCounterparts)
                {
                    if (i.FindModuleImplementing<EngineDecay>() != null)
                    {
                        i.FindModuleImplementing<EngineDecay>().procSRBDiameter = procSRBDiameter;
                        i.FindModuleImplementing<EngineDecay>().UpdateModelState();
                    }
                    else
                    {
                        Lib.LogWarning("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                    }
                }
            }
        }

        public void ProcUpdateThrust(BaseField thrust, object obj)
        {
            procSRBThrust = (float)thrust.GetValue(procSRB);

            if (obj != null)
            {
                UpdateModelState();

                foreach (Part i in part.symmetryCounterparts)
                {
                    if (i.FindModuleImplementing<EngineDecay>() != null)
                    {
                        i.FindModuleImplementing<EngineDecay>().procSRBThrust = procSRBThrust;
                        i.FindModuleImplementing<EngineDecay>().UpdateModelState();
                    }
                    else
                    {
                        Lib.LogWarning("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                    }
                }
            }
        }

        public void ProcUpdateBellName(BaseField bellName, object obj)
        {
            procSRBBellName = (string)bellName.GetValue(procSRB);

            if (obj != null)
            {
                UpdateModelState();

                foreach (Part i in part.symmetryCounterparts)
                {
                    if (i.FindModuleImplementing<EngineDecay>() != null)
                    {
                        i.FindModuleImplementing<EngineDecay>().procSRBBellName = procSRBBellName;
                        i.FindModuleImplementing<EngineDecay>().UpdateModelState();
                    }
                    else
                    {
                        Lib.LogWarning("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                    }
                }
            }
        }

        void UpdateModelState()
        {
            knownPartCost = -1;
            ReplaceEvent();
        }

        #endregion
    }
}