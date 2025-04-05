using System.Collections.Generic;

namespace Assetto.Data.Broadcasting.Structs
{
    public struct TrackData
    {
        public string TrackName { get; set; }
        public int TrackId { get; set; }
        public float TrackMeters { get; set; }
        public Dictionary<string, List<string>> CameraSets { get; set; }
        public IEnumerable<string> HUDPages { get; set; }
    }
}
