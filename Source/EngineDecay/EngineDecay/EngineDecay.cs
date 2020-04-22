using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KSP.IO;

namespace EngineDecay
{
    public class EngineDecay : PartModule, IPartMassModifier, IPartCostModifier
    {
        #region fields

        [KSPField(isPersistant = true, guiActive = false)]
        public float baseRatedTime = 10;

        [KSPField(isPersistant = true, guiActive = false)]
        public float maxRatedTime = 100;

        [KSPField(isPersistant = true, guiActive = false)]
        public float resourceCostRatio = 0.5f;

        [KSPField(isPersistant = true, guiActive = false)]
        public float maxMassRatedTimeCoeff = 0.2f;

        [KSPField(isPersistant = true, guiActive = false)]
        public float maxCostRatedTimeCoeff = 4;

        [KSPField(isPersistant = true, guiActive = false)]
        public float resourceExcessCoeff = 0.5f;

        [KSPField(isPersistant = true, guiActive = false)]
        public int baseIgnitions = 1;

        [KSPField(isPersistant = true, guiActive = false)]
        public int maxIgnitons = 100;

        [KSPField(isPersistant = true, guiActive = false)]
        public float maxMassIgnitionsCoeff = 0.1f;

        [KSPField(isPersistant = true, guiActive = false)]
        public float maxCostIgnitionsCoeff = 0.5f;


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


        bool wasRunningPrevTick = false;
        bool wasRailWarpingPrevTick = false;

        [KSPField(isPersistant = true, guiActive = false)]
        float knownPartCost = -1;

        [KSPField(isPersistant = true, guiActive = false)]
        bool newBorn = true;

        [KSPField(isPersistant = true, guiActive = false)]
        float faliureTime = 0;

        [KSPField(isPersistant = true, guiActive = false)]
        bool checkMaintenance = false;

        [KSPField(isPersistant = true, guiActive = false)]
        int maintenanceCost = 0;

        bool notInEditor = false;
        float ignoreIgnitionTill = 0;
        int ticksTillDisabling = -1;
        float holdIndicatorsTill = 0;

        private List<ModuleEngines> decaying_engines;

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

            part.ModulesOnUpdate();
        }

        #endregion

        #region game-called methods

        public override void OnStart(StartState state)
        {
            decaying_engines = part.FindModulesImplementing<ModuleEngines>();

            if (state == StartState.Editor)
            {
                notInEditor = false;
            }
            else
            {
                notInEditor = true;

                ignoreIgnitionTill = Time.time + 0.5f;
                
                if(reliabilityStatus == "failed")
                {
                    Failure();
                }

                checkMaintenance = true;
            }
        }

        public void Update()
        {
            if (notInEditor && newBorn)
            {
                throw new Exception("EngineDecay MODULE thinks it is not in editor but not initialized yet");
            }

            if (!notInEditor)
            {
                newBorn = false;

                if (prevEBTP != extraBurnTimePercent || prevEIP != extraIgnitionsPercent)
                {
                    setBurnTime = baseRatedTime + extraBurnTimePercent * (maxRatedTime - baseRatedTime) / 100;
                    usedBurnTime = 0;

                    setIgnitions = (int)(baseIgnitions + extraIgnitionsPercent * (maxIgnitons - baseIgnitions) / 100);
                    ignitionsLeft = setIgnitions;

                    UpdateIndicators();

                    maintenanceCost = 0;
                    Events["Maintenance"].guiActiveEditor = false;

                    prevEBTP = extraBurnTimePercent;
                    prevEIP = extraIgnitionsPercent;
                }

                if(checkMaintenance)
                {
                    maintenanceCost = (int)(knownPartCost * (1 + extraBurnTimePercent * maxCostRatedTimeCoeff / 100) * resourceCostRatio * usedBurnTime / (setBurnTime * resourceExcessCoeff));
                    if(maintenanceCost > 0)
                    {
                        Events["Maintenance"].guiActiveEditor = true;
                        Events["Maintenance"].guiName = String.Format("maintenance: {0}", maintenanceCost);
                    }

                    checkMaintenance = false;
                }
            }
        }

        public void FixedUpdate()
        {
            if (notInEditor)
            {
                bool railWarping = IsRailWarping();

                if(!railWarping)
                {
                    if (wasRailWarpingPrevTick)
                    {
                        ignoreIgnitionTill = Time.time + 0.5f;
                    }

                    if (IsRunning())
                    {
                        usedBurnTime += (float)TimeWarp.fixedDeltaTime;
                    }

                    if ((usedBurnTime >= setBurnTime * (resourceExcessCoeff + 1)) && nominal)
                    {
                        Failure();
                    }

                    if(ticksTillDisabling > 0)
                    {
                        CutoffOnFailure();
                        ticksTillDisabling--;
                    }

                    if(ticksTillDisabling == 0)
                    {
                        Disable();
                        ticksTillDisabling = -1;
                    }

                    if (Time.time >= ignoreIgnitionTill)
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
                wasRunningPrevTick = IsRunning();
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

            if(newBorn)
            {
                return 0;
            }
            else
            {
                return (extraBurnTimePercent * maxCostRatedTimeCoeff * defaultCost / 100) + (extraIgnitionsPercent * maxCostIgnitionsCoeff * defaultCost / 100) -
                    (defaultCost + extraBurnTimePercent * maxCostRatedTimeCoeff * defaultCost / 100) * resourceCostRatio * usedBurnTime / (setBurnTime * resourceExcessCoeff);
            }
        }

        ModifierChangeWhen IPartCostModifier.GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.CONSTANTLY;
        }

        #endregion

        #endregion

        #region internal methods

        bool IsRunning()
        {
            bool running = false;
            foreach (var i in decaying_engines)
            {
                running = running || (i.currentThrottle > 0 && i.EngineIgnited);
            }

            return running;
        }

        bool IsRailWarping()
        {
            return TimeWarp.WarpMode == TimeWarp.Modes.HIGH && TimeWarp.CurrentRate != 1;
        }

        void checkIgnition()
        {
            bool running = IsRunning();
            if (running && !wasRunningPrevTick)
            {
                ignitionsLeft -= 1;
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
            foreach (ModuleEngines i in decaying_engines)
            {
                i.Shutdown();
                i.currentThrottle = 0;
            }
        }

        void Disable()
        {
            foreach (ModuleEngines i in decaying_engines)
            {
                i.Shutdown();
                i.currentThrottle = 0;
                i.enabled = false;
                i.isEnabled = false;
            }
        }

        void LastIgnitionCheck()
        {
            if(wasRunningPrevTick && !IsRunning() && ignitionsLeft == 0)
            {
                Disable();

                reliabilityStatus = "out of ignitions";
                nominal = false;
            }
        }

        void UpdateIndicators()
        {
            burnTimeIndicator = String.Format("{0}h:{1}:{2} / {3}h:{4}:{5}", ((int)usedBurnTime) / 3600, (((int)usedBurnTime) % 3600) / 60, ((int)usedBurnTime) % 60, ((int)setBurnTime) / 3600, (((int)setBurnTime) % 3600) / 60, ((int)setBurnTime) % 60);
            ignitionsIndicator = String.Format("{0} / {1}", ignitionsLeft, setIgnitions);

            holdIndicatorsTill = Time.time + 0.5f;
        }
        #endregion
    }
}
