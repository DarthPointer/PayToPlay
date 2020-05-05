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
        public string decayRates;

        List<float> decayRatesList = new List<float>();

        [KSPField(isPersistant = true, guiActive = false)]
        public string ignitionsUsage;

        List<bool> ignitionsUsageList = new List<bool>();

        [KSPField(isPersistant = true, guiActive = false)]
        public string ignitionsOnSwitch;

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
        float setBurnTime;

        [KSPField(isPersistant = true, guiActive = false)]
        float usedBurnTime;

        [KSPField(isPersistant = true, guiActive = false)]
        float usageExperienceCoeff = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        int setIgnitions;

        [KSPField(isPersistant = true, guiActive = false)]
        int ignitionsLeft;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Burn Time Used", guiFormat = "F2")]
        string burnTimeIndicator;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Ignitions Left", guiFormat = "F2")]
        string ignitionsIndicator;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Reliability Status", guiFormat = "F2")]
        string reliabilityStatus = "nominal";

        [KSPField(isPersistant = true, guiActive = false)]
        bool nominal = true;

        int modeRunningPrevTick = -1;
        bool wasRailWarpingPrevTick = false;

        [KSPField(isPersistant = true, guiActive = false)]
        float knownPartCost = -1;

        [KSPField(isPersistant = true, guiActive = false)]
        bool newBorn = true;

        [KSPField(isPersistant = true, guiActive = false)]
        float failAtBurnTime = -1;

        [KSPField(isPersistant = true, guiActive = false)]
        int maintenanceCost = 0;

        bool inEditor = true;
        float ignoreIgnitionTill = 0;
        int ticksTillDisabling = -1;
        float holdIndicatorsTill = 0;

        List<ModuleEngines> decayingEngines;
        MultiModeEngine modeSwitcher;

        #endregion

        #region actions and events

        [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "maintenance")]
        void Maintenance()
        {
            usedBurnTime = 0;

            ignitionsLeft = setIgnitions;

            UpdateIndicators();

            reliabilityStatus = "nominal";
            nominal = true;

            maintenanceCost = 0;
            Events["Maintenance"].guiActiveEditor = false;

            Enable();

            failAtBurnTime = -1;

            usageExperienceCoeff = 0;

            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
        }

        #endregion

        #region game-called methods

        public override void OnStart(StartState state)
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

            if (baseIgnitions == -1)
            {
                Fields["extraIgnitionsPercent"].guiActiveEditor = false;
                Fields["extraIgnitionsPercent"].guiActive = false;

                Fields["ignitionsIndicator"].guiActiveEditor = false;
                Fields["ignitionsIndicator"].guiActive = false;
            }

            if (state == StartState.Editor)
            {
                inEditor = true;

                if (r == 0)
                {
                    r = ReliabilityProgress.fetch.GetExponent(part.name);
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
            }
        }

        public void Update()
        {
            if (!inEditor && newBorn)
            {
                throw new Exception("EngineDecay MODULE thinks it is not in editor but not initialized yet");
            }

            if (inEditor)
            {
                newBorn = false;

                maintenanceCost = (int)(knownPartCost * (1f + extraBurnTimePercent * maxCostRatedTimeCoeff / 100f) * maintenanceAtRatedTimeCoeff * usedBurnTime / setBurnTime);

                if (maintenanceCost > knownPartCost * (1f + extraBurnTimePercent * maxCostRatedTimeCoeff / 100f))
                {
                    maintenanceCost = (int)(knownPartCost * (1f + extraBurnTimePercent * maxCostRatedTimeCoeff / 100f));
                }

                if (maintenanceCost > 0 || !nominal)
                {
                    Events["Maintenance"].guiActiveEditor = true;
                    Events["Maintenance"].guiName = String.Format("maintenance: {0}", maintenanceCost);
                }

                if (prevEBTP != extraBurnTimePercent || prevEIP != extraIgnitionsPercent)
                {
                    if (maintenanceCost > 0)
                    {
                        Maintenance();
                    }

                    currentBaseRatedTime = ProbabilityLib.ATangentCumulativePercentArg(r, topBaseRatedTime);

                    setBurnTime = currentBaseRatedTime * (1 + extraBurnTimePercent * (topMaxRatedTime / topBaseRatedTime - 1) / 100);
                    usedBurnTime = 0;

                    setIgnitions = (int)(baseIgnitions + extraIgnitionsPercent * (maxIgnitions - baseIgnitions) / 100);
                    ignitionsLeft = setIgnitions;

                    UpdateIndicators();

                    maintenanceCost = 0;
                    Events["Maintenance"].guiActiveEditor = false;

                    prevEBTP = extraBurnTimePercent;
                    prevEIP = extraIgnitionsPercent;

                    failAtBurnTime = -1;
                }
            }
        }

        public void FixedUpdate()
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
                        if (!usingMultiModeLogic)
                        {
                            usedBurnTime += TimeWarp.fixedDeltaTime;
                        }
                        else
                        {
                            usedBurnTime += TimeWarp.fixedDeltaTime * decayRatesList[runningMode];
                        }

                        if(usedBurnTime <= setBurnTime)
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

                    if (Time.time >= ignoreIgnitionTill && baseIgnitions != -1 && nominal)
                    {
                        checkIgnition();
                    }
                }

                if (nominal)
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
            if (knownPartCost == -1)            //It is assumed not to change. It makes procedural engines have issues.
            {
                knownPartCost = defaultCost;
            }

            if (newBorn)
            {
                return 0;
            }
            else
            {
                if(setBurnTime/usedBurnTime > maintenanceAtRatedTimeCoeff)
                {
                    return (extraBurnTimePercent * maxCostRatedTimeCoeff * defaultCost / 100) + (extraIgnitionsPercent * maxCostIgnitionsCoeff * defaultCost / 100) -
                    (defaultCost + extraBurnTimePercent * maxCostRatedTimeCoeff * defaultCost / 100) * maintenanceAtRatedTimeCoeff * usedBurnTime / setBurnTime;
                }
                else
                {
                    return (extraBurnTimePercent * maxCostRatedTimeCoeff * defaultCost / 100) + (extraIgnitionsPercent * maxCostIgnitionsCoeff * defaultCost / 100) -
                    (defaultCost + extraBurnTimePercent * maxCostRatedTimeCoeff * defaultCost / 100);
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
                    }
                }
            }
        }

        void Failure()
        {
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
            burnTimeIndicator = String.Format("{0}h:{1}:{2} / {3}h:{4}:{5}", ((int)usedBurnTime) / 3600, (((int)usedBurnTime) % 3600) / 60, ((int)usedBurnTime) % 60, ((int)setBurnTime) / 3600, (((int)setBurnTime) % 3600) / 60, ((int)setBurnTime) % 60);
            ignitionsIndicator = String.Format("{0} / {1}", ignitionsLeft, setIgnitions);

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
    }
}