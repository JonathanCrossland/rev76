namespace Rev76.DataModels
{
    public class Tyres
    {
        public enum Position
        {
            FrontLeft,
            FrontRight,
            RearLeft,
            RearRight
        }

        public string TyreCompound { get; set; }
        public float MfdTyrePressureLF { get; set; }
        public float MfdTyrePressureRF { get; set; }
        public float MfdTyrePressureLR { get; set; }
        public float MfdTyrePressureRR { get; set; }
        public int RainTyres { get; set; }
        public int MfdTyreSet { get; set; }
        public int CurrentTyreSet { get; set; }
        public int StrategyTyreSet { get; set; }

        // Additional tyre-related properties
        //public TyreStat WheelSlip { get; set; }
        //public TyreStat WheelLoad { get; set; }
        public TyreStat WheelsPressure { get; set; }
        //public TyreStat WheelAngularSpeed { get; set; }
        public TyreStat TyreWear { get; set; }
        public TyreStat TyreDirtyLevel { get; set; }
        public TyreStat TyreCoreTemperature { get; set; }
        //public TyreStat CamberRad { get; set; }
        //public TyreStat SuspensionTravel { get; set; }
        public TyreStat BrakeTemp { get; set; }
        //public TyreStat TyreTempI { get; set; }
        //public TyreStat TyreTempM { get; set; }
        //public TyreStat TyreTempO { get; set; }
        //public TyreStat SuspensionDamage { get; set; }
        public TyreStat TyreTemp { get; set; }
        public TyreStat BrakePressure { get; set; }
        
        public TyreStat DiscLife { get; set; }
        public float SteerAngle { get; set; }
        public int FrontBrakeCompound { get; set; }
        public int RearBrakeCompound { get; set; }
        public float Brake { get; set; }
       
        private float _brakeBias;
        private bool _isBrakeBiasDefaultSet = false; // Tracks if the default has been set

        private float _BrakeBiasDefault = -1;
        public float BrakeBiasDefault
        {
            get { return _BrakeBiasDefault; }

        }
        public float BrakeBias
        {
            get => _brakeBias;
            set
            {
               
                    // Only set BrakeBiasDefault if it has not been set and the value is non-zero
                    if (_BrakeBiasDefault < 0 && !_isBrakeBiasDefaultSet && value > 0)
                    {
                        _BrakeBiasDefault = value;
                        _isBrakeBiasDefaultSet = true;


                    }

                    _brakeBias = value;
               
            }
        }

        private TyreStat _padLife;
        private TyreStat _padLifeDefault;
        private bool _isPadLifeDefaultSet = false; // Tracks if the default has been set

        public TyreStat PadLife
        {
            get => _padLife;
            set 
            {
                // Set the default only once
                if (!_isPadLifeDefaultSet && value.RearLeft > 0 && value.RearRight > 0)
                {
                    _padLifeDefault = value;
                    _isPadLifeDefaultSet = true;
                }

                _padLife = value;
            }
        }


        public float AbsVibrations { get; set; }
        public float TCInAction { get; set; }
    
        public int TC { get; set; }
        public int ABS { get; set; }
        public TyreStat WheelSlip { get; set; }

        public float GetPadLifePercentage(Position position)
        {
            float defaultValue = 0;
            float currentValue = 0;

            switch (position)
            {
                case Position.FrontLeft:
                    defaultValue = _padLifeDefault.FrontLeft;
                    currentValue = PadLife.FrontLeft;
                    break;
                case Position.FrontRight:
                    defaultValue = _padLifeDefault.FrontRight;
                    currentValue = PadLife.FrontRight;
                    break;
                case Position.RearLeft:
                    defaultValue = _padLifeDefault.RearLeft;
                    currentValue = PadLife.RearLeft;
                    break;
                case Position.RearRight:
                    defaultValue = _padLifeDefault.RearRight;
                    currentValue = PadLife.RearRight;
                    break;
            }

            if (defaultValue == 0) return 0; // Avoid division by zero
            return (currentValue / defaultValue) * 100;
        }



        //public TyreStat Mz { get; set; }
        //public TyreStat Fx { get; set; }
        //public TyreStat Fy { get; set; }
        //public TyreStat SlipRatio { get; set; }
        //public TyreStat SlipAngle { get; set; }

        public string GetBrakeString()
        {
            return $"BRAKES {FrontBrakeCompound} / {RearBrakeCompound}";
        }

        public string GetTyreString()
        {
            var tyre = RainTyres == 1 ? $"WET" : $"DRY {CurrentTyreSet}";
            return tyre;
        }

       

    }

}
