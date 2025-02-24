using System.Collections.Concurrent;

namespace Rev76.DataModels
{

    public class Track
    {
        public ConcurrentDictionary<int,Car> Cars { get; set; } = new ConcurrentDictionary<int,Car>();
        public float TrackLength { get;  set; }
        public string Name { get;  set; }
        public int NumberOfCars { get; set; }
    }

}
