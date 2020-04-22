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


        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Extra Burn Time Percent", guiFormat = "F2"),
            UI_FloatEdit(scene = UI_Scene.Editor, minValue = 0, maxValue = 100, incrementLarge = 20, incrementSmall = 5, incrementSlide = 1)]
        float extraBurnTimePercent = 0;

        //float prevEBTP = -1;

        //[KSPField(isPersistant = true, guiActive = false)]
        //float chosenBTime;

        //[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Chosen Burn Time", guiFormat = "F2")]
        //string chosenBurnTime;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Extra Ignitions Percent", guiFormat = "F2"),
            UI_FloatEdit(scene = UI_Scene.Editor, minValue = 0, maxValue = 100, incrementLarge = 20, incrementSmall = 5, incrementSlide = 1)]
        public float extraIgnitionsPercent = 0;
        
        //float prevEIP = -1;

        //[KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Ignitions", guiFormat = "F2")]
        //int Ign;

        [KSPField(isPersistant = true, guiActive = false)]
        float setBurnTime;

        [KSPField(isPersistant = true, guiActive = false)]
        float usedBurnTime;

        /*[KSPField(isPersistant = true, guiActive = false)]
        float burnCostPerSecond;*/

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

        bool nominal = true;


        bool wasRunningPrevTick = false;
        bool wasRailWarpingPrevTick = false;

        /*[KSPField(isPersistant = true, guiActive = false)]
        public float baseOnCost = -1;*/

        //float knownPartCost = -1;

        //[KSPField(isPersistant = true, guiActive = false)]
        //float resourceConsumption;

        [KSPField(isPersistant = true, guiActive = false)]
        bool newBorn = true;

        [KSPField(isPersistant = true, guiActive = false)]
        float faliureTime = 0;

        bool notInEditor = false;
        float ignoreIgnitionTill = 0;
        int ticksTillDisabling = -1;
        float holdIndicatorsTill = 0;

        #endregion

        private List<ModuleEngines> decaying_engines;

        public override void OnStart(StartState state)
        {
            //knownPartCost = baseOnCost;
            decaying_engines = part.FindModulesImplementing<ModuleEngines>();

            //chosenBTime = baseRatedTime + extraBurnTimePercent * (maxRatedTime - baseRatedTime) / 100;
            //float maxAmount = knownPartCost * (1 + extraBurnTimePercent * maxCostRatedTimeCoeff / 100) * resourceCostRatio;
            //resourceConsumption = maxAmount / ((1f + resourceExcessCoeff) * chosenBTime);

            if (state == StartState.Editor)
            {
                notInEditor = false;
            }
            else
            {
                notInEditor = true;

                ignoreIgnitionTill = Time.time + 3;
                
                if(reliabilityStatus == "failed")
                {
                    Failure();
                }
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

                setBurnTime = baseRatedTime + extraBurnTimePercent * (maxRatedTime - baseRatedTime) / 100;
                usedBurnTime = 0;

                setIgnitions = (int)(baseIgnitions + extraIgnitionsPercent * (maxIgnitons - baseIgnitions) / 100);
                ignitionsLeft = setIgnitions;

                UpdateIndicators();

                //chosenBTime = baseRatedTime + extraBurnTimePercent * (maxRatedTime - baseRatedTime) / 100;
                //int t = (int)chosenBTime;
                //chosenBurnTime = String.Format("{0}h:{1}:{2}", t / 3600, (t % 3600) / 60, t % 60);

                //Ign = (int)(baseIgnitions + extraIgnitionsPercent * (maxIgnitons - baseIgnitions) / 100);

                /*if (extraBurnTimePercent != prevEBTP)
                {
                    

                    float maxAmount = knownPartCost * (1 + extraBurnTimePercent * maxCostRatedTimeCoeff / 100) * resourceCostRatio;
                    resourceConsumption = maxAmount / ((1f + resourceExcessCoeff) * chosenBTime);

                    var reslib = PartResourceLibrary.Instance.resourceDefinitions;                                              //this is copypasta from kerbalism, need some revision
                                                                                                                                // if the resource is not known, log a warning and do nothing
                    if (!reslib.Contains("_EngineResource"))
                    {
                        print("wtf man, I dunno what your _EngineResource is");
                        return;
                    }
                    var resourceDefinition = reslib["_EngineResource"];

                    PartResource engineResource = part.Resources[resourceDefinition.name];

                    if (engineResource == null)
                    {
                        engineResource = new PartResource(part);
                        engineResource.SetInfo(resourceDefinition);
                        engineResource.maxAmount = maxAmount;
                        engineResource.amount = maxAmount;
                        engineResource.flowState = true;
                        engineResource.isTweakable = resourceDefinition.isTweakable;
                        engineResource.isVisible = resourceDefinition.isVisible;
                        engineResource.hideFlow = false;
                        part.Resources.dict.Add(resourceDefinition.name.GetHashCode(), engineResource);

                        PartResource simulationResource = new PartResource(engineResource);
                        simulationResource.simulationResource = true;
                        part.SimulationResources?.dict.Add(resourceDefinition.name.GetHashCode(), simulationResource);

                        // flow mode is a property that call some code using SimulationResource in its setter.
                        // consequently it must be set after simulationResource is registered to avoid the following log error spam :
                        // [PartSet]: Failed to add Resource XXXXX to Simulation PartSet:XX as corresponding Part XXXX SimulationResource was not found.
                        engineResource.flowMode = PartResource.FlowMode.Both;

                        GameEvents.onPartResourceListChange.Fire(part);
                    }
                    else if (!notInEditor)
                    {
                        engineResource.maxAmount = maxAmount;

                        PartResource simulationResource = part.SimulationResources?[resourceDefinition.name];
                        if (simulationResource != null) simulationResource.maxAmount = maxAmount;

                        if (engineResource.amount >= maxAmount)
                        {
                            engineResource.amount = maxAmount;
                        }
                    }

                    prevEBTP = extraBurnTimePercent;
                }*/

                /*if (extraIgnitionsPercent != prevEIP)
                {
                    float maxAmount = Ign;

                    var reslib = PartResourceLibrary.Instance.resourceDefinitions;                                              //this is copypasta from kerbalism, need some revision
                                                                                                                                // if the resource is not known, log a warning and do nothing
                    if (!reslib.Contains("_Ignitions"))
                    {
                        print("wtf man, I dunno what your _Ignitions is");
                        return;
                    }
                    var resourceDefinition = reslib["_Ignitions"];

                    PartResource ignitions = part.Resources[resourceDefinition.name];

                    if (ignitions == null)
                    {
                        ignitions = new PartResource(part);
                        ignitions.SetInfo(resourceDefinition);
                        ignitions.maxAmount = maxAmount;
                        ignitions.amount = maxAmount;
                        ignitions.flowState = true;
                        ignitions.isTweakable = resourceDefinition.isTweakable;
                        ignitions.isVisible = resourceDefinition.isVisible;
                        ignitions.hideFlow = false;
                        part.Resources.dict.Add(resourceDefinition.name.GetHashCode(), ignitions);

                        PartResource simulationResource = new PartResource(ignitions);
                        simulationResource.simulationResource = true;
                        part.SimulationResources?.dict.Add(resourceDefinition.name.GetHashCode(), simulationResource);

                        // flow mode is a property that call some code using SimulationResource in its setter.
                        // consequently it must be set after simulationResource is registered to avoid the following log error spam :
                        // [PartSet]: Failed to add Resource XXXXX to Simulation PartSet:XX as corresponding Part XXXX SimulationResource was not found.
                        ignitions.flowMode = PartResource.FlowMode.Both;

                        GameEvents.onPartResourceListChange.Fire(part);
                    }
                    else if (!notInEditor)
                    {
                        ignitions.maxAmount = maxAmount;

                        PartResource simulationResource = part.SimulationResources?[resourceDefinition.name];
                        if (simulationResource != null) simulationResource.maxAmount = maxAmount;

                        if (ignitions.amount >= maxAmount)
                        {
                            ignitions.amount = maxAmount;
                        }
                    }

                    prevEIP = extraIgnitionsPercent;
                }*/
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
                        //part.RequestResource("_EngineResource", TimeWarp.fixedDeltaTime * resourceConsumption);
                        usedBurnTime += (float)TimeWarp.fixedDeltaTime;
                    }

                    if (usedBurnTime == setBurnTime * resourceExcessCoeff && nominal)
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

        #region mass and cost modifiers implementation

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
            /*if (knownPartCost == -1)
            {
                knownPartCost = defaultCost;
            }*/

            float result = defaultCost * (extraBurnTimePercent * maxCostRatedTimeCoeff + extraIgnitionsPercent * maxCostIgnitionsCoeff) / 100;
            if (!newBorn)
            {
                result += (float)part.Resources["_EngineResource"].amount - (float)part.Resources["_EngineResource"].maxAmount;
            }


            return result;
        }

        ModifierChangeWhen IPartCostModifier.GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.CONSTANTLY;
        }

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
                //part.RequestResource("_Ignitions", 1f);
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
