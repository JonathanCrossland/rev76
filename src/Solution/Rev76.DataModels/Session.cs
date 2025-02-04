using Assetto.Data.Broadcasting;
using Assetto.Data.Broadcasting.Structs;

namespace Rev76.DataModels
{
    public class Session
    {
        public LapInfo BestSession { get; set; }
        public BroadcastingCarEventType EventType { get; internal set; }
        public FlagType Flag { get; internal set; }

        public bool Broadcasting { get; internal set; }
    }
}
