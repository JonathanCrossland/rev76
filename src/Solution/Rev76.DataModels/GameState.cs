using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;


namespace Rev76.DataModels
{
    public class GameState
    {

        /// <summary>
        /// In the garage, during a race, getting tires changed/fuel added etc.
        /// value of 1 is true
        /// </summary>
        public bool InPit { get; set; }

        /// <summary>
        /// From cross the pit lane in - to out
        /// value of 1 is true
        /// </summary>
        public bool InPitLane { get; set; }

        public bool IsRacing { get; set; }

        public string TrackStatus { get; set; }
        public bool IsSetupMenuVisible { get; internal set; }
        public SessionType SessionType { get; internal set; }
        public GameStatus Status { get; internal set; }
        public float SessionTimeLeft { get; internal set; }
    }
}
