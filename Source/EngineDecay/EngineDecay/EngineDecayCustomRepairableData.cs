using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KerbalRepairsInterface;

namespace EngineDecay
{
    class EngineDecayCustomRepairableData : IKRISerializedCustomData
    {
        public enum RepairType
        {
            FULL_MAINTENANCE = 0,
            IGNITION_RESTORE = 1,
            FAILURE_FIX = 2
        }

        public RepairType repairType;

        public EngineDecayCustomRepairableData(RepairType repairType)
        {
            this.repairType = repairType;
        }

        #region IKRISerializedCustomData
        public string Serialize()
        {
            return repairType.ToString();
        }

        public void Deserialize(string serialized)
        {
            repairType = (RepairType)Enum.Parse(typeof(RepairType), serialized);
        }
        #endregion
    }
}
