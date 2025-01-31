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

        public string TyreCompound { get; internal set; }
        public float MfdTyrePressureLF { get; internal set; }
        public float MfdTyrePressureRF { get; internal set; }
        public float MfdTyrePressureLR { get; internal set; }
        public float MfdTyrePressureRR { get; internal set; }
        public int RainTyres { get; internal set; }
        public int MfdTyreSet { get; internal set; }
        public int CurrentTyreSet { get; internal set; }
        public int StrategyTyreSet { get; internal set; }

        // Additional tyre-related properties
        //public TyreStat WheelSlip { get; internal set; }
        //public TyreStat WheelLoad { get; internal set; }
        public TyreStat WheelsPressure { get; internal set; }
        //public TyreStat WheelAngularSpeed { get; internal set; }
        public TyreStat TyreWear { get; internal set; }
        public TyreStat TyreDirtyLevel { get; internal set; }
        public TyreStat TyreCoreTemperature { get; internal set; }
        //public TyreStat CamberRad { get; internal set; }
        //public TyreStat SuspensionTravel { get; internal set; }
        public TyreStat BrakeTemp { get; internal set; }
        //public TyreStat TyreTempI { get; internal set; }
        //public TyreStat TyreTempM { get; internal set; }
        //public TyreStat TyreTempO { get; internal set; }
        //public TyreStat SuspensionDamage { get; internal set; }
        public TyreStat TyreTemp { get; internal set; }
        public TyreStat BrakePressure { get; internal set; }
        
        public TyreStat DiscLife { get; internal set; }
        public float SteerAngle { get; internal set; }
        public int FrontBrakeCompound { get; internal set; }
        public int RearBrakeCompound { get; internal set; }
        public float Brake { get; internal set; }
       
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
            internal set
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
            internal set 
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


        public float AbsVibrations { get; internal set; }
        public float TCInAction { get; internal set; }
    
        public int TC { get; internal set; }
        public int ABS { get; internal set; }
        public TyreStat WheelSlip { get; internal set; }

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



        //public TyreStat Mz { get; internal set; }
        //public TyreStat Fx { get; internal set; }
        //public TyreStat Fy { get; internal set; }
        //public TyreStat SlipRatio { get; internal set; }
        //public TyreStat SlipAngle { get; internal set; }

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
