using System.Collections.Generic;
using System.Runtime;

namespace Assetto.Data.Broadcasting.Structs
{
    public class LapInfo
    {
        public int? LaptimeMS { get; set; }
        public List<int?> Splits { get; } = new List<int?>();
        public ushort CarIndex { get; set; }
        public ushort DriverIndex { get; set; }
        public bool IsInvalid { get; set; }
        public bool IsValidForBest { get; set; }
        public LapType Type { get; set; }

        public int LapNumber { get; set; }

        public override string ToString()
        {
            return $"{LaptimeMS, 5}|{string.Join("|", Splits)}";
        }
    }
}
