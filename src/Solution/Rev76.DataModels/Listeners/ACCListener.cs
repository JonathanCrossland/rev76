using System;

using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Assetto.Data.Broadcasting.Structs;



namespace Rev76.DataModels.Listeners
{
    public class ACCListener : IDisposable
    {
        //private readonly object _DriverLock = new object()
        //     private Driver _Driver;

        private AccSharedMemory _AccMemory;
       
        public ACCListener()
        {
            _AccMemory = new AccSharedMemory();
            _AccMemory.StaticInfoUpdateInterval = 1000;
            _AccMemory.PhysicsUpdateInterval = 50;
            _AccMemory.GraphicsUpdateInterval = 500;
            
            SubscribeToEvents();

        }

        private void SubscribeToEvents()
        {
         
            _AccMemory.StaticInfoUpdated += (sender, e) =>
            {
                
                    GameData.Car.CarModel = e.Data.CarModel;

                    if (GameData.Car.Drivers.Count == 0)
                    {
                        DriverInfo driverInfo = new DriverInfo();
                        driverInfo.FirstName = e.Data.PlayerName;
                        driverInfo.LastName = e.Data.PlayerSurname;
                        driverInfo.ShortName = e.Data.PlayerNick;
                        GameData.Car.Drivers.Add(driverInfo);

                    }

                

            };

            // Subscribe the event for PhysicsUpdated and write some example data to stdout
            _AccMemory.PhysicsUpdated += (sender, e) =>
            {
               
                    GameData.Weather.AirTemperature = e.Data.AirTemp.ToString();
                    GameData.Weather.RoadTemperature = e.Data.RoadTemp.ToString();
                    GameData.Weather.AirDensity = e.Data.AirDensity.ToString();
                    GameData.Car.FuelTank.Fuel = e.Data.Fuel;
                    GameData.Tyres.SteerAngle = e.Data.SteerAngle;
                    //tyres
                    //GameData.Tyres.WheelSlip = e.Data.WheelSlip;
                    //GameData.Tyres.WheelLoad = e.Data.WheelLoad;
                    GameData.Tyres.WheelsPressure = e.Data.WheelsPressure;
                    //GameData.Tyres.WheelAngularSpeed = e.Data.WheelAngularSpeed;
                    GameData.Tyres.TyreWear = e.Data.TyreWear;
                    GameData.Tyres.TyreDirtyLevel = e.Data.TyreDirtyLevel;
                    GameData.Tyres.TyreCoreTemperature = e.Data.TyreCoreTemperature;
                    //GameData.Tyres.CamberRad = e.Data.CamberRad;
                    //GameData.Tyres.SuspensionTravel = e.Data.SuspensionTravel;
                    GameData.Tyres.BrakeTemp = e.Data.BrakeTemp;
                    GameData.Tyres.FrontBrakeCompound = e.Data.FrontBrakeCompound + 1;
                    GameData.Tyres.RearBrakeCompound = e.Data.RearBrakeCompound + 1;
                    GameData.Tyres.Brake = e.Data.Brake;

                    GameData.Tyres.BrakeBias = BrakeBiasAdjustment(e.Data.BrakeBias);

                    GameData.Tyres.WheelSlip = e.Data.WheelSlip;

                    //GameData.Tyres.TyreTempI = e.Data.TyreTempI;
                    //GameData.Tyres.TyreTempM = e.Data.TyreTempM;
                    //GameData.Tyres.TyreTempO = e.Data.TyreTempO;
                    //GameData.Tyres.SuspensionDamage = e.Data.SuspensionDamage;
                    GameData.Tyres.TyreTemp = e.Data.TyreTemp;
                    GameData.Tyres.BrakePressure = e.Data.BrakePressure;
                    GameData.Tyres.PadLife = e.Data.PadLife;
                    GameData.Tyres.DiscLife = e.Data.DiscLife;
                    GameData.Tyres.AbsVibrations = e.Data.AbsVibrations;
                    GameData.Tyres.TCInAction = e.Data.TC;
                    //GameData.Tyres.Mz = e.Data.Mz;
                    //GameData.Tyres.Fx = e.Data.Fx;
                    //GameData.Tyres.Fy = e.Data.Fy;
                    //GameData.Tyres.SlipRatio = e.Data.SlipRatio;
                    //GameData.Tyres.SlipAngle = e.Data.SlipAngle;

                
            };

            // Subscribe the event for GraphicsUpdated and write some example data to stdout
            _AccMemory.GraphicsUpdated += (sender, e) =>
            {

                if (GameData.GameState.SessionType != e.Data.Session 
                    || GameData.GameState.SessionTimeLeft < e.Data.SessionTimeLeft
                   
                    )
                {
                    GameData.Reset();
                }
                
                    GameData.Car.CarIndex = e.Data.PlayerCarID;
                                

                    GameData.Weather.WindDirection = e.Data.WindDirection.ToString();
                    GameData.Weather.WindSpeed = e.Data.WindSpeed.ToString();
                    GameData.Weather.RainIntensity = e.Data.RainIntensity;
                    GameData.Weather.RainIn10Minutes = e.Data.RainIntensityIn10min;
                    GameData.Weather.RainIn30Minutes = e.Data.RainIntensityIn30min;

                    GameData.Weather.Clock = GameData.GetFormattedClockTime(e.Data.Clock);

                    GameData.GameState.InPit = e.Data.IsInPit == 1;
                    GameData.GameState.InPitLane = e.Data.IsInPitLane == 1;
                    GameData.GameState.IsSetupMenuVisible = e.Data.IsSetupMenuVisible == 1;
                    GameData.GameState.TrackStatus = e.Data.TrackStatus;
                    GameData.GameState.SessionType = e.Data.Session;
                    GameData.GameState.Status = e.Data.Status;

                    //tyres
                    GameData.Tyres.TyreCompound = e.Data.TyreCompound;
                    GameData.Tyres.MfdTyrePressureLF = e.Data.MfdTyrePressureLF;
                    GameData.Tyres.MfdTyrePressureRF = e.Data.MfdTyrePressureRF;
                    GameData.Tyres.MfdTyrePressureLR = e.Data.MfdTyrePressureLR;
                    GameData.Tyres.MfdTyrePressureRR = e.Data.MfdTyrePressureRR;
                    GameData.Tyres.RainTyres = e.Data.RainTyres;
                    GameData.Tyres.MfdTyreSet = e.Data.MfdTyreSet;
                    GameData.Tyres.CurrentTyreSet = e.Data.CurrentTyreSet;
                    GameData.Tyres.StrategyTyreSet = e.Data.StrategyTyreSet;

                    GameData.Car.FuelTank.FuelXLap = e.Data.FuelXLap;
                    GameData.Car.Laps = e.Data.NumberOfLaps;
                    GameData.GameState.SessionTimeLeft = e.Data.SessionTimeLeft;


                    GameData.Tyres.TC = e.Data.TC;
                    GameData.Tyres.ABS = e.Data.ABS;

                    GameData.Car.GapAhead = e.Data.GapAhead;
                    GameData.Car.GapBehind = e.Data.GapAhead;
                
                //e.Data.LastTime
            };

            
        }

        private float BrakeBiasAdjustment(float brakeBias)
        {
            if (string.IsNullOrEmpty(GameData.Car.CarModel))
            {
                return 0;
            }
            if (brakeBias == 0)
            {
                return 0.5f;
            }
            switch (GameData.Car.CarModel.ToLower())
            {
                case "aston_martin_vantage_v12_gt3":
                case "bentley_continental_gt3":
                case "emil_frey_jaguar_g3":
                case "aston_martin_v8_vantage_gt3":
                    brakeBias -= 0.7f;
                    break;

                case "audi_r8_lms":
                case "honda_nsx_gt3":
                case "lamborghini_gallardo_g3_reiter":
                case "lamborghini_huracan_gt3":
                case "lamborghini_huracan_st":
                case "lexus_rcf_gt3":
                case "mercedes_amg_gt3":
                case "audi_r8_lms_evo":
                case "honda_nsx_gt3_evo":
                case "lamborghini_huracan_gt3_evo":
                case "bmw_m4_gt3":
                case "audi_r8_lms_evo_ii":
                case "lamborghini_huracan_st_evo2":
                case "mercedes_amg_gt3_evo":
                    brakeBias -= 0.14f;
                    break;

                case "bmw_m6_gt3":
                case "nissan_gtr_nismo_gt3":
                case "bmw_m4_gt4":
                    brakeBias -= 0.15f;
                    break;

                case "ferrari_488_gt3":
                case "mclaren_650s_gt3":
                case "mclaren_720s_gt3":
                case "bmw_m2_cup":
                    brakeBias -= 0.17f;
                    break;

                case "porsche_991_gt3_r":
                case "porsche_911_ii_gt3_r":
                case "porsche_718_cayman_gt4_mr":
                    brakeBias -= 0.21f;
                    break;

                case "porsche_991_ii_gt3_cup":
                case "porsche_992_gt3_cup":
                    brakeBias -= 0.5f;
                    break;

                case "aston_martin_vantage_amr_gt4":
                case "ktm_xbow_gt4":
                case "mercedes_amg_gt4":
                    brakeBias -= 0.20f;
                    break;

                case "chevrolet_camaro_gt4_r":
                case "ginetta_g55_gt4":
                    brakeBias -= 0.18f;
                    break;

                case "maserati_gran_turismo_mc_gt4":
                case "audi_r8_lms_gt4":
                case "alpine_a110_gt4":
                    brakeBias -= 0.15f;
                    break;

                case "mclaren_570s_gt4":
                    brakeBias -= 0.9f;
                    break;

                case "ferrari_488_challenge_evo":
                    brakeBias -= 0.13f;
                    break;

                default:
                    // No adjustment for cars not listed
                    break;
            }

            return brakeBias;
        }


        public async Task Listen(CancellationToken token)
        {

            Trace.WriteLine("ACCListener ...");
            await _AccMemory.ConnectAsync(token);
            
        }

        public void Dispose()
        {
            // Dispose the AccSharedMemory instance when no longer needed
            _AccMemory?.Dispose();
            //_Driver?.Dispose();
        }

       
    }
}
