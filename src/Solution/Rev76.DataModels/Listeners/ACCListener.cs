using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Assetto.Data.Broadcasting.Structs;

namespace Rev76.DataModels.Listeners
{
    public class ACCListener : IDisposable
    {
       
        private AccSharedMemory _AccMemory;

        public ACCListener()
        {
            _AccMemory = new AccSharedMemory();
            _AccMemory.StaticInfoUpdateInterval = 1000;
            _AccMemory.PhysicsUpdateInterval = 50;
            _AccMemory.GraphicsUpdateInterval = 200;
       
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {

            _AccMemory.StaticInfoUpdated += (sender, e) =>
            {
                GameData.Instance.Track.Name = e.Data.Track;

                AddPlayerDriverInfo(e);
            };

            _AccMemory.PhysicsUpdated += (sender, e) =>
            {
                if (GameData.Instance.GameState.Status == GameStatus.OFF) return;

                GameData.Instance.Weather.AirTemperature = e.Data.AirTemp.ToString();
                GameData.Instance.Weather.RoadTemperature = e.Data.RoadTemp.ToString();
                GameData.Instance.Weather.AirDensity = e.Data.AirDensity.ToString();
              
                Car meCar = GameData.Instance.MeCar; 

                if (meCar != null)
                {
                    meCar.FuelTank.Fuel = e.Data.Fuel;

                    AddTyreInfo(e, meCar);
                }

            };


            _AccMemory.GraphicsUpdated += (sender, e) =>
            {
                GameData.Instance.PlayerCarIndex = e.Data.PlayerCarID;

                GameData.Instance.GameState.Status = e.Data.Status;

                if (GameData.Instance.GameState.Status == GameStatus.OFF) return;


                GameData.Instance.Session.SessionType = e.Data.Session;

                GameData.Instance.Weather.WindDirection = e.Data.WindDirection.ToString();
                GameData.Instance.Weather.WindSpeed = e.Data.WindSpeed.ToString();
                GameData.Instance.Weather.RainIntensity = e.Data.RainIntensity;
                GameData.Instance.Weather.RainIn10Minutes = e.Data.RainIntensityIn10min;
                GameData.Instance.Weather.RainIn30Minutes = e.Data.RainIntensityIn30min;
                GameData.Instance.Weather.Clock = GameData.GetFormattedClockTime(e.Data.Clock);

                GameData.Instance.Session.InPit = e.Data.IsInPit == 1;
                GameData.Instance.Session.InPitLane = e.Data.IsInPitLane == 1;
                GameData.Instance.Session.TrackStatus = e.Data.TrackStatus;
                GameData.Instance.Session.SessionType = e.Data.Session;
                GameData.Instance.Session.SessionTimeLeft = e.Data.SessionTimeLeft;

                GameData.Instance.GameState.IsSetupMenuVisible = e.Data.IsSetupMenuVisible == 1;
                GameData.Instance.GameState.Status = e.Data.Status;

                AddTyreInfo(e);

                Car meCar = GameData.Instance.MeCar;

                if (meCar != null)
                {
                    meCar.Flag = e.Data.Flag;
                    meCar.FuelTank.FuelXLap = e.Data.FuelXLap;
                    meCar.Laps = e.Data.NumberOfLaps;
                    meCar.GapAhead = e.Data.GapAhead; //always zero
                    meCar.GapBehind = e.Data.GapAhead;//always zero
                    meCar.Position = e.Data.Position;

                }

                GameData.Instance.Session.FlagSector1 = e.Data.GlobalYellow1;
                GameData.Instance.Session.FlagSector2 = e.Data.GlobalYellow2;
                GameData.Instance.Session.FlagSector3 = e.Data.GlobalYellow3;


                GameData.Instance.Session.GlobalGreen = e.Data.GlobalGreen;
                GameData.Instance.Session.GlobalYellow = e.Data.GlobalYellow;
                GameData.Instance.Session.GlobalRed = e.Data.GlobalRed;
                GameData.Instance.Session.GlobalWhite = e.Data.GlobalWhite;
                GameData.Instance.Session.GlobalChequered = e.Data.GlobalChequered;

                GameData.Instance.Session.Flag = e.Data.Flag;
            };
        }

        private static void AddTyreInfo(UpdatedEventArgs<Graphics> e)
        {
            GameData.Instance.Tyres.TyreCompound = e.Data.TyreCompound;
            GameData.Instance.Tyres.MfdTyrePressureLF = e.Data.MfdTyrePressureLF;
            GameData.Instance.Tyres.MfdTyrePressureRF = e.Data.MfdTyrePressureRF;
            GameData.Instance.Tyres.MfdTyrePressureLR = e.Data.MfdTyrePressureLR;
            GameData.Instance.Tyres.MfdTyrePressureRR = e.Data.MfdTyrePressureRR;
            GameData.Instance.Tyres.RainTyres = e.Data.RainTyres;
            GameData.Instance.Tyres.MfdTyreSet = e.Data.MfdTyreSet;
            GameData.Instance.Tyres.CurrentTyreSet = e.Data.CurrentTyreSet;
            GameData.Instance.Tyres.StrategyTyreSet = e.Data.StrategyTyreSet;
            GameData.Instance.Tyres.TC = e.Data.TC;
            GameData.Instance.Tyres.ABS = e.Data.ABS;
        }

        private void AddTyreInfo(UpdatedEventArgs<Physics> e, Car meCar)
        {
            GameData.Instance.Tyres.SteerAngle = e.Data.SteerAngle;
            GameData.Instance.Tyres.WheelsPressure = e.Data.WheelsPressure;
            GameData.Instance.Tyres.TyreWear = e.Data.TyreWear;
            GameData.Instance.Tyres.TyreDirtyLevel = e.Data.TyreDirtyLevel;
            GameData.Instance.Tyres.TyreCoreTemperature = e.Data.TyreCoreTemperature;
            GameData.Instance.Tyres.BrakeTemp = e.Data.BrakeTemp;
            GameData.Instance.Tyres.FrontBrakeCompound = e.Data.FrontBrakeCompound + 1;
            GameData.Instance.Tyres.RearBrakeCompound = e.Data.RearBrakeCompound + 1;
            GameData.Instance.Tyres.Brake = e.Data.Brake;
            GameData.Instance.Tyres.BrakeBias = BrakeBiasAdjustment(meCar, e.Data.BrakeBias);
            GameData.Instance.Tyres.WheelSlip = e.Data.WheelSlip;
            GameData.Instance.Tyres.TyreTemp = e.Data.TyreTemp;
            GameData.Instance.Tyres.BrakePressure = e.Data.BrakePressure;
            GameData.Instance.Tyres.PadLife = e.Data.PadLife;
            GameData.Instance.Tyres.DiscLife = e.Data.DiscLife;
            GameData.Instance.Tyres.AbsVibrations = e.Data.AbsVibrations;
            GameData.Instance.Tyres.TCInAction = e.Data.TC;


            //GameData.Instance.Tyres.WheelSlip = e.Data.WheelSlip;
            //GameData.Instance.Tyres.WheelLoad = e.Data.WheelLoad;
            //GameData.Instance.Tyres.CamberRad = e.Data.CamberRad;
            //GameData.Instance.Tyres.SuspensionTravel = e.Data.SuspensionTravel;
            //GameData.Instance.Tyres.WheelAngularSpeed = e.Data.WheelAngularSpeed;
            //GameData.Instance.Tyres.TyreTempI = e.Data.TyreTempI;
            //GameData.Instance.Tyres.TyreTempM = e.Data.TyreTempM;
            //GameData.Instance.Tyres.TyreTempO = e.Data.TyreTempO;
            //GameData.Instance.Tyres.SuspensionDamage = e.Data.SuspensionDamage;

            //GameData.Instance.Tyres.Mz = e.Data.Mz;
            //GameData.Instance.Tyres.Fx = e.Data.Fx;
            //GameData.Instance.Tyres.Fy = e.Data.Fy;
            //GameData.Instance.Tyres.SlipRatio = e.Data.SlipRatio;
            //GameData.Instance.Tyres.SlipAngle = e.Data.SlipAngle;
        }

        private static void AddPlayerDriverInfo(UpdatedEventArgs<StaticInfo> e)
        {
            Car meCar = GameData.Instance.MeCar;

            if (meCar != null)
            {
                meCar.CarModel = e.Data.CarModel;

                if (meCar.Drivers.Count == 0)
                {
                    DriverInfo driverInfo = new DriverInfo();
                    driverInfo.FirstName = e.Data.PlayerName;
                    driverInfo.LastName = e.Data.PlayerSurname;
                    driverInfo.ShortName = e.Data.PlayerNick;
                    meCar.Drivers.Add(driverInfo);

                }
            }
        }

        private float BrakeBiasAdjustment(Car car,float brakeBias)
        {
            if (string.IsNullOrEmpty(car.CarModel))
            {
                return 0;
            }
            if (brakeBias == 0)
            {
                return 0.5f;
            }
            switch (car.CarModel.ToLower())
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
            Trace.WriteLine("Mem: ACCListener ...");
            await _AccMemory.ConnectAsync(token);
        }

        public void Dispose()
        {
            _AccMemory?.Dispose();
        }
    }
}
