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
        float extraBurnTimePercent = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        float prevEBTP = -1;

        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Extra Ignitions Percent", guiFormat = "D"),
            UI_FloatEdit(scene = UI_Scene.Editor, minValue = 0, maxValue = 100, incrementLarge = 20, incrementSmall = 5, incrementSlide = 1)]
        float extraIgnitionsPercent = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        float prevEIP = -1;

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

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Ignitions Left", guiFormat = "F2")]
        string ignitionsIndicator = "";

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Reliability Status", guiFormat = "F2")]
        string reliabilityStatus = "nominal";

        [KSPField(isPersistant = true, guiActive = false)]
        bool nominal = true;

        int modeRunningPrevTick = -1;
        bool wasRailWarpingPrevTick = false;

        [KSPField(isPersistant = true, guiActive = false)]
        float knownPartCost = -1;

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
        float procSRBDiameter;

        [KSPField(isPersistant = true, guiActive = false)]
        float procSRBThrust;

        [KSPField(isPersistant = true, guiActive = false)]
        string procSRBNozzleName;

        bool inEditor = true;
        float ignoreIgnitionTill = 0;
        int ticksTillDisabling = -1;
        float holdIndicatorsTill = 0;

        List<ModuleEngines> decayingEngines;
        MultiModeEngine modeSwitcher;

        PartModule procSRBTank;
        PartModule procSRB;

        #endregion

        #region maintenance

        [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "Maintenance")]
        void MaintenanceEvent()
        {
            foreach (Part i in part.symmetryCounterparts)
            {
                EngineDecay engineDecay = i.FindModuleImplementing<EngineDecay>();
                if (i != null)
                {
                    engineDecay.CounterpartMaintenance(maintenanceCost);
                }
                else
                {
                    UnityEngine.Debug.Log("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                }
            }

            CounterpartMaintenance(maintenanceCost);                            // the counterpart is the same part for this case, we call it to update symmetry maintenance button

            Maintenance();

            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
        }

        void Maintenance()
        {
            usedBurnTime = 0;
            usageExperienceCoeff = 0;
            r = ReliabilityProgress.fetch.GetExponent(part.name);
            currentBaseRatedTime = ProbabilityLib.ATangentCumulativePercentArg(r, topBaseRatedTime);
            setBurnTime = currentBaseRatedTime * (1 + extraBurnTimePercent * (topMaxRatedTime / topBaseRatedTime - 1) / 100);

            ignitionsLeft = setIgnitions;

            UpdateIndicators();

            reliabilityStatus = "nominal";
            nominal = true;

            Enable();

            failAtBurnTime = -1;

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
        }

        void MaintenanceFromCounterpart()
        {
            Maintenance();

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
                Events["MaintenanceEvent"].guiName = String.Format("Maintenance: {0}", maintenanceCost);
            }

            return maintenanceCost;
        }

        #endregion

        #region game-called methods

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

                    if (r == 0 && topBaseRatedTime != -1)
                    {
                        r = ReliabilityProgress.fetch.GetExponent(part.name);
                    }

                    if (procPart)
                    {
                        part.Modules["ProceduralShapeCylinder"].Fields["diabmeter"].uiControlEditor.onFieldChanged += ProcUpdateDiameter;
                        part.Modules["ProceduralSRB"].Fields["thrust"].uiControlEditor.onFieldChanged += ProcUpdateThrust;
                        part.Modules["ProceduralSRB"].Fields["selectedBellName"].uiControlEditor.onFieldChanged += ProcUpdateNozzleName;

                        if ((procSRBTank == null) || (procSRB == null))
                        {
                            Debug.LogError("An EngineDecay module marked as a one for ProceduralParts SRB could not find relevant modules. Switched to non-procedural logic");
                            procPart = false;
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

                if (inEditor)
                {
                    if (newBorn)
                    {
                        newBorn = false;

                        /*ProcUpdateDiameter(procSRBTank.Fields["diameter"], null);
                        ProcUpdateNozzleName(procSRB.Fields["thrust"], null);
                        ProcUpdateThrust(procSRB.Fields["selectedBellName"], null);*/
                    }

                    UpdateMaintenanceCost();

                    List<Part> counterparts = part.symmetryCounterparts;
                    if(counterparts.Count() != 0)
                    {
                        if(symmetryMaintenanceCost == -1)
                        {
                            foreach (Part i in counterparts)
                            {
                                EngineDecay engineDecay = i.FindModuleImplementing<EngineDecay>();
                                if (engineDecay != null)
                                {
                                    symmetryMaintenanceCost += engineDecay.UpdateMaintenanceCost();
                                }
                                else
                                {
                                    UnityEngine.Debug.Log("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                                }
                            }

                            foreach (Part i in counterparts)
                            {
                                EngineDecay engineDecay = i.FindModuleImplementing<EngineDecay>();
                                if (engineDecay != null)
                                {
                                    engineDecay.symmetryMaintenanceCost = symmetryMaintenanceCost;
                                    if(symmetryMaintenanceCost > 0)
                                    {
                                        engineDecay.Events["SymmetryMaintenance"].guiName = string.Format("Symmetry Maintenance: {0}", symmetryMaintenanceCost);
                                        engineDecay.Events["SymmetryMaintenance"].guiActiveEditor = true;
                                    }
                                }
                                else
                                {
                                    UnityEngine.Debug.Log("EngineDecay found a counterpart without EngineDecay, it is really WEIRD!");
                                }
                            }
                        }

                        if (symmetryMaintenanceCost > 0)
                        {
                            Events["SymmetryMaintenance"].guiName = string.Format("Symmetry Maintenance: {0}", symmetryMaintenanceCost);
                            Events["SymmetryMaintenance"].guiActiveEditor = true;
                        }
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

                        UpdateIndicators();

                        maintenanceCost = 0;
                        Events["MaintenanceEvent"].guiActiveEditor = false;

                        prevEBTP = extraBurnTimePercent;
                        prevEIP = extraIgnitionsPercent;

                        failAtBurnTime = -1;
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

                        if (topBaseRatedTime != -1)
                        {
                            if (runningMode != -1)
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
                if (knownPartCost == -1 || procPart)            //It is assumed not to change only for procedural parts. Probably this check will have to be removed
                {
                    if (subtractResourcesCost)
                    {
                        foreach (PartResource i in part.Resources.ToList())
                        {
                            defaultCost -= (float)i.maxAmount * PartResourceLibrary.Instance.GetDefinition(i.resourceName).unitCost;
                        }
                    }

                    knownPartCost = defaultCost;
                }
            }

            if (newBorn)
            {
                return 0;
            }
            else
            {
                if (!useSRBCost)
                {
                    if (setBurnTime / usedBurnTime > maintenanceAtRatedTimeCoeff)
                    {
                        return (extraBurnTimePercent * maxCostRatedTimeCoeff * knownPartCost / 100) + (extraIgnitionsPercent * maxCostIgnitionsCoeff * knownPartCost / 100) -
                        (knownPartCost + extraBurnTimePercent * maxCostRatedTimeCoeff * knownPartCost / 100) * maintenanceAtRatedTimeCoeff * usedBurnTime / setBurnTime;
                    }
                    else
                    {
                        return (extraBurnTimePercent * maxCostRatedTimeCoeff * knownPartCost / 100) + (extraIgnitionsPercent * maxCostIgnitionsCoeff * knownPartCost / 100) -
                        (knownPartCost + extraBurnTimePercent * maxCostRatedTimeCoeff * knownPartCost / 100);
                    }
                }
                else
                {
                    if (ignitionsLeft == setIgnitions)
                    {
                        return 0;
                    }
                    else
                    {
                        return -knownPartCost * maintenanceAtRatedTimeCoeff;
                    }
                }
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
                    if (luck < 0.0005f)
                    {
                        Failure();
                    }
                    else if(luck < 0.001f)
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
            if(UnityEngine.Random.Range(0f, 1f) < 0.05f)
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

        void UpdateIndicators()
        {
            burnTimeIndicator = string.Format("{0}h:{1}:{2} / {3}h:{4}:{5}", ((int)usedBurnTime) / 3600, (((int)usedBurnTime) % 3600) / 60, ((int)usedBurnTime) % 60, ((int)setBurnTime) / 3600, (((int)setBurnTime) % 3600) / 60, ((int)setBurnTime) % 60);
            ignitionsIndicator = string.Format("{0} / {1}", ignitionsLeft, setIgnitions);

            holdIndicatorsTill = Time.time + 0.5f;
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

        #region ProcPart Callbacks

        public void ProcUpdateDiameter(BaseField diameter, object obj)
        {
            print(string.Format("Field diameter is {0}", diameter.GetValue(obj)));
        }

        public void ProcUpdateThrust(BaseField thrust, object obj)
        {
            print(string.Format("Field diameter is {0}", thrust.GetValue(obj)));
        }

        public void ProcUpdateNozzleName(BaseField nozzleName, object obj)
        {
            print(string.Format("Field diameter is {0}", nozzleName.GetValue(obj)));
        }

        #endregion
    }
}