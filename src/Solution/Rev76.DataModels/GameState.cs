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

     
        public bool IsSetupMenuVisible { get; internal set; }
      
        public GameStatus Status { get; internal set; }

        public bool Broadcasting { get; internal set; }
    }
}
