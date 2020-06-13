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
        public float prevEBTP = -1;

        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Extra Ignitions Percent", guiFormat = "D"),
            UI_FloatEdit(scene = UI_Scene.Editor, minValue = 0, maxValue = 100, incrementLarge = 20, incrementSmall = 5, incrementSlide = 1)]
        public float extraIgnitionsPercent = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        public float prevEIP = -1;

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
        bool nominal = true;

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
        float failAtBurnTime = -1;

        [KSPField(isPersistant = true, guiActive = false)]
        int maintenanceCost = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        int symmetryMaintenanceCost = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        int replaceCost = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        int symmetryReplaceCost = 0;

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

        List<ModuleEngines> decayingEngines;
        MultiModeEngine modeSwitcher;

        PartModule procSRBCylinder;
        PartModule procSRB;

        #endregion

        #region Maintenance

        [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "Maintenance")]
        void MaintenanceEvent()
        {
            foreach (Part i in part.symmetryCounterparts)
            {
                EngineDecay engineDecay = i.FindModuleImplementing<EngineDecay>();
                if (i != null)
                {
                    engineDecay.CounterpartMaintenance(maintenanceCost);
                    engineDecay.CounterpartReplace(maintenanceCost);
                }
                else
                {
                    UnityEngine.Debug.Log("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                }
            }

            CounterpartMaintenance(maintenanceCost);                            // the counterpart is the same part for this case, we call it to update symmetry maintenance button
            CounterpartReplace(maintenanceCost);

            Maintenance();

            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
        }

        void Maintenance()
        {
            usedBurnTime = 0;
            usageExperienceCoeff = 0;

            if (topBaseRatedTime != -1)                                                 // The r parameter is changed with the P2PAddon while reading usage experience
            {
                currentBaseRatedTime = ProbabilityLib.ATangentCumulativePercentArg(r, topBaseRatedTime);
                setBurnTime = currentBaseRatedTime * (1 + extraBurnTimePercent * (topMaxRatedTime / topBaseRatedTime - 1) / 100);
            }

            if (baseIgnitions != -1)
            {
                ignitionsLeft = setIgnitions;
            }

            UpdateIndicators();

            reliabilityStatus = "nominal";
            nominal = true;

            Enable();

            failAtBurnTime = -1;

            if (maintenanceCost != 0)
            {
                targetPartCost = maintenanceCost;
            }

            UpdateReplaceCost();

            maintenanceCost = 0;
            Events["MaintenanceEvent"].guiActiveEditor = false;
        }

        [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "Symmetry Maintenance")]
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
                    UnityEngine.Debug.Log("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                }
            }

            MaintenanceFromCounterpart();

            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
        }

        void MaintenanceFromCounterpart()
        {
            Maintenance();

            symmetryReplaceCost -= symmetryMaintenanceCost;
            UpdateSymmetryReplaceButton();

            symmetryMaintenanceCost = 0;
            Events["SymmetryMaintenance"].guiActiveEditor = false;
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

                if (maintenanceCost > knownPartCost * (1f + extraBurnTimePercent * maxCostRatedTimeCoeff / 100f))
                {
                    maintenanceCost = (int)(knownPartCost * (1f + extraBurnTimePercent * maxCostRatedTimeCoeff / 100f));
                }
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

            if (maintenanceCost > 0 || !nominal)
            {
                Events["MaintenanceEvent"].guiActiveEditor = true;
                Events["MaintenanceEvent"].guiName = string.Format("Maintenance: {0}", maintenanceCost);
            }

            return maintenanceCost;
        }

        #endregion

        #region Replacement

        [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "Replace")]
        void ReplaceEvent()
        {
            foreach (Part i in part.symmetryCounterparts)
            {
                EngineDecay engineDecay = i.FindModuleImplementing<EngineDecay>();
                if (i != null)
                {
                    engineDecay.CounterpartReplace(replaceCost);
                    engineDecay.CounterpartMaintenance(maintenanceCost);
                }
                else
                {
                    UnityEngine.Debug.Log("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                }
            }

            CounterpartReplace(replaceCost);                            // the counterpart is the same part for this case, we call it to update symmetry replace button
            CounterpartMaintenance(maintenanceCost);

            maintenanceCost = 0;
            Events["MaintenanceEvent"].guiActiveEditor = false;

            Replace();

            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
        }

        void Replace()
        {
            usedBurnTime = 0;
            usageExperienceCoeff = 0;

            if (!procPart)
            {
                r = ReliabilityProgress.fetch.GetExponent(part.name);
            }
            else
            {
                r = ReliabilityProgress.fetch.CheckProcSRBProgress(part.name, ref procSRBDiameter, ref procSRBThrust, ref procSRBBellName);

                if (r == -1)
                {
                    r = PayToPlaySettings.StartingReliability;
                    Events["SetAsANewProcSRBModel"].guiActiveEditor = true;
                }
                else
                {
                    Events["SetAsANewProcSRBModel"].guiActiveEditor = false;
                }
            }

            if (topBaseRatedTime != -1)
            {
                currentBaseRatedTime = ProbabilityLib.ATangentCumulativePercentArg(r, topBaseRatedTime);
                setBurnTime = currentBaseRatedTime * (1 + extraBurnTimePercent * (topMaxRatedTime / topBaseRatedTime - 1) / 100);
            }

            if (baseIgnitions != -1)
            {
                ignitionsLeft = setIgnitions;
            }

            UpdateIndicators();

            reliabilityStatus = "nominal";
            nominal = true;

            Enable();

            failAtBurnTime = -1;

            targetPartCost = fullPartCost;

            replaceCost = 0;
            Events["ReplaceEvent"].guiActiveEditor = false;
        }

        [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "Symmetry Replace")]
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
                    Debug.LogError("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                }
            }

            ReplaceFromCounterpart();

            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
        }

        void ReplaceFromCounterpart()
        {
            maintenanceCost = 0;
            symmetryMaintenanceCost = 0;
            Events["MaintenanceEvent"].guiActiveEditor = false;
            Events["SymmetryMaintenance"].guiActiveEditor = false;

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

        int UpdateReplaceCost()
        {
            replaceCost = (int)(fullPartCost - targetPartCost);

            if (replaceCost > 0 || !nominal)
            {
                Events["ReplaceEvent"].guiActiveEditor = true;
                Events["ReplaceEvent"].guiName = string.Format("Replace: {0}", replaceCost);
            }
            else
            {
                Events["ReplaceEvent"].guiActiveEditor = false;
            }

            return replaceCost;
        }

        void UpdateSymmetryReplaceButton()
        {
            if (symmetryReplaceCost > 0 || !nominal)
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

        #region proc SRB events

        [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "Set as a New Model")]
        void SetAsANewProcSRBModel()
        {
            ReliabilityProgress.fetch.CreateModel(part.name, procSRBDiameter, procSRBThrust, procSRBBellName);
            Events["SetAsANewProcSRBModel"].guiActiveEditor = false;
        }

        #endregion

        #region Indicators

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Switch Time Format")]
        void SwitchTimeFormat()
        {
            if (usingTimeFormat == 0 && PayToPlaySettings.UseNonstandardLongTimeFormat)
            {
                usingTimeFormat = 2;
            }
            else if (usingTimeFormat == 0 && !PayToPlaySettings.UseNonstandardLongTimeFormat)
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
            burnTimeIndicator = string.Format("{0} / {1}", Lib.Format(usedBurnTime, usingTimeFormat), Lib.Format(setBurnTime, usingTimeFormat));
            ignitionsIndicator = string.Format("{0} / {1}", ignitionsLeft, setIgnitions);

            holdIndicatorsTill = Time.time + 0.5f;
        }

        #endregion

        #region Game-called Methods

        public override void OnStart(StartState state)
        {
            if (PayToPlaySettings.Enable)
            {
                decayingEngines = part.FindModulesImplementing<ModuleEngines>();
                modeSwitcher = part.FindModuleImplementing<MultiModeEngine>();

                modesNumber = decayingEngines.Count();

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

                if (state == StartState.Editor)
                {
                    inEditor = true;

                    if (!procPart)
                    {
                        r = ReliabilityProgress.fetch.GetExponent(part.name);
                    }
                    else
                    {
                        procSRBCylinder = part.Modules["ProceduralShapeCylinder"];
                        procSRB = part.Modules["ProceduralSRB"];

                        if ((procSRBCylinder == null) || (procSRB == null))
                        {
                            print("An EngineDecay module marked as a one for ProceduralParts SRB could not find relevant modules. Switched to non-procedural logic");
                            Debug.LogError("An EngineDecay module marked as a one for ProceduralParts SRB could not find relevant modules. Switched to non-procedural logic");
                            procPart = false;
                        }
                        else
                        {
                            procSRBCylinder.Fields["diameter"].uiControlEditor.onFieldChanged += ProcUpdateDiameter;
                            procSRB.Fields["thrust"].uiControlEditor.onFieldChanged += ProcUpdateThrust;
                            procSRB.Fields["selectedBellName"].uiControlEditor.onFieldChanged += ProcUpdateBellName;

                            ProcUpdateDiameter(procSRBCylinder.Fields["diameter"], null);
                            ProcUpdateBellName(procSRB.Fields["selectedBellName"], null);
                            ProcUpdateThrust(procSRB.Fields["thrust"], null);
                        }
                    }
                }
                else
                {
                    inEditor = false;

                    ignoreIgnitionTill = Time.time + 0.5f;

                    if (!nominal)
                    {
                        Disable();
                    }
                    else if (failAtBurnTime == -1)
                    {
                        SetFailTime();
                    }

                    symmetryMaintenanceCost = -1;
                    symmetryReplaceCost = -1;
                }
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
        }

        public void Update()
        {
            if (PayToPlaySettings.Enable)
            {
                if (!inEditor && newBorn)
                {
                    throw new Exception("EngineDecay MODULE thinks it is not in editor but not initialized yet");
                }

                newBorn = false;

                if (inEditor)
                {
                    List<Part> counterparts = part.symmetryCounterparts;
                    if(counterparts.Count() != 0)
                    {
                        if(symmetryMaintenanceCost == -1)
                        {
                            symmetryMaintenanceCost = UpdateMaintenanceCost();
                            symmetryReplaceCost = UpdateReplaceCost();

                            foreach (Part i in counterparts)
                            {
                                EngineDecay engineDecay = i.FindModuleImplementing<EngineDecay>();
                                if (engineDecay != null)
                                {
                                    symmetryMaintenanceCost += engineDecay.UpdateMaintenanceCost();
                                    symmetryReplaceCost += engineDecay.UpdateReplaceCost();
                                }
                                else
                                {
                                    Debug.LogError("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                                }
                            }

                            foreach (Part i in counterparts)
                            {
                                EngineDecay engineDecay = i.FindModuleImplementing<EngineDecay>();
                                if (engineDecay != null)
                                {
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
                                }
                                else
                                {
                                    Debug.LogError("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                                }
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
                    }
                    else
                    {
                        UpdateMaintenanceCost();
                        UpdateReplaceCost();
                    }

                    if (prevEBTP != extraBurnTimePercent || prevEIP != extraIgnitionsPercent)
                    {
                        if (maintenanceCost > 0)
                        {
                            SymmetryMaintenance();
                        }

                        if (topBaseRatedTime != -1)
                        {
                            currentBaseRatedTime = ProbabilityLib.ATangentCumulativePercentArg(r, topBaseRatedTime);

                            setBurnTime = currentBaseRatedTime * (1 + extraBurnTimePercent * (topMaxRatedTime / topBaseRatedTime - 1) / 100);
                            usedBurnTime = 0;
                        }

                        if (baseIgnitions != -1)
                        {
                            setIgnitions = (int)(baseIgnitions + extraIgnitionsPercent * (maxIgnitions - baseIgnitions) / 100);
                            ignitionsLeft = setIgnitions;
                        }

                        ReplaceEvent();

                        //UpdateIndicators();

                        /*maintenanceCost = 0;
                        Events["MaintenanceEvent"].guiActiveEditor = false;

                        prevEBTP = extraBurnTimePercent;
                        prevEIP = extraIgnitionsPercent;

                        failAtBurnTime = -1;

                        knownPartCost = -1;*/
                    }
                }
            }
        }

        public void FixedUpdate()
        {
            if (PayToPlaySettings.Enable)
            {
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
                                usageExperienceCoeff = 0.1f;
                            }
                            targetPartCost = 0;
                        }

                        if (topBaseRatedTime != -1)
                        {
                            if (usedBurnTime > failAtBurnTime && nominal)
                            {
                                Failure();

                                usageExperienceCoeff = 0.3f;
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

                        if (Time.time >= ignoreIgnitionTill && baseIgnitions != -1 && nominal)
                        {
                            checkIgnition();
                        }
                    }

                    if (nominal && baseIgnitions != -1)
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
                    if (i != this)
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
                return targetPartCost - fullPartCost;
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
                    if (luck < PayToPlaySettings.FailureOnIgnitionPercent * (8.01 - r))        // 8.01 stands for 8 is max r, 8.01 - 8 = 1/100 thus settings are relevant for max-reliability parts
                    {
                        Failure();

                        usageExperienceCoeff = 0.3f;
                    }
                    else if(luck < (PayToPlaySettings.FailureOnIgnitionPercent + PayToPlaySettings.IgnitionFailurePercent) * (8.01 - r))
                    {
                        FlightLogger.fetch?.LogEvent(String.Format("Bad ignition of {0}, shutdown performed to prevent consequences", part.name));
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
                        if (luck < 0.0005f)
                        {
                            Failure();

                            usageExperienceCoeff = 0.3f;
                        }
                        else if (luck < 0.001f)
                        {
                            FlightLogger.fetch?.LogEvent(String.Format("Bad ignition of {0}, shutdown performed to prevent consequences", part.name));
                            CutoffOnFailure();
                        }
                    }
                }
            }
        }

        void Failure()
        {
            if(UnityEngine.Random.Range(0f, 1f) < PayToPlaySettings.DestructionOnFailurePercent/100 * (9 - r))
            {
                BadaBoom();
            }
            else
            {
                FlightLogger.fetch?.LogEvent(string.Format("{0} failed", part.name));
            }

            ticksTillDisabling = 5;

            CutoffOnFailure();

            reliabilityStatus = "failed";

            nominal = false;
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
                    nominal = false;
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

                            nominal = false;
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

        void SetFailTime()
        {
            failAtBurnTime = ProbabilityLib.ATangentRandom(r, topBaseRatedTime);
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
        #endregion

        #region ProcSRB Internal Methods

        public void ProcUpdateDiameter(BaseField diameter, object obj)
        {
            procSRBDiameter = (float)diameter.GetValue(procSRBCylinder);
            UpdateModelState(obj != null);
        }

        public void ProcUpdateThrust(BaseField thrust, object obj)
        {
            procSRBThrust = (float)thrust.GetValue(procSRB);
            UpdateModelState(obj != null);
        }

        public void ProcUpdateBellName(BaseField bellName, object obj)
        {
            procSRBBellName = (string)bellName.GetValue(procSRB);
            UpdateModelState(obj != null);
        }

        void UpdateModelState(bool hardReset)
        {
            r = ReliabilityProgress.fetch.CheckProcSRBProgress(part.name, ref procSRBDiameter, ref procSRBThrust, ref procSRBBellName);

            if (r == -1)
            {
                r = PayToPlaySettings.StartingReliability;
                Events["SetAsANewProcSRBModel"].guiActiveEditor = true;
            }
            else
            {
                Events["SetAsANewProcSRBModel"].guiActiveEditor = false;
            }

            if (hardReset)
            {
                if (maintenanceCost > 0)
                {
                    SymmetryMaintenance();
                }

                if (topBaseRatedTime != -1)
                {
                    currentBaseRatedTime = ProbabilityLib.ATangentCumulativePercentArg(r, topBaseRatedTime);

                    setBurnTime = currentBaseRatedTime * (1 + extraBurnTimePercent * (topMaxRatedTime / topBaseRatedTime - 1) / 100);
                    usedBurnTime = 0;
                }

                if (baseIgnitions != -1)
                {
                    setIgnitions = (int)(baseIgnitions + extraIgnitionsPercent * (maxIgnitions - baseIgnitions) / 100);
                    ignitionsLeft = setIgnitions;
                }

                ReplaceEvent();

                //UpdateIndicators();

                /*maintenanceCost = 0;
                Events["MaintenanceEvent"].guiActiveEditor = false;

                failAtBurnTime = -1;

                knownPartCost = -1;*/
            }
        }

        #endregion
    }
}