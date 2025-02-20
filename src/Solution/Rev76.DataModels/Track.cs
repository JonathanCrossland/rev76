using System.Collections.Generic;

namespace Rev76.DataModels
{

    public class Track
    {
        public List<Car> Cars { get; set; } = new List<Car>();
        public float TrackLength { get;  set; }
        public string Name { get;  set; }
        public int NumberOfCars { get; internal set; }
    }

}
