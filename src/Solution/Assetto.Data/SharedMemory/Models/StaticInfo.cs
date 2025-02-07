
using System;
using System.Runtime.InteropServices;



[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
[Serializable]
public struct StaticInfo {
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string SMVersion;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string ACVersion;
    public int NumberOfSessions;
    public int NumCars;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string CarModel;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string Track;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string PlayerName;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string PlayerSurname;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string PlayerNick;
    public int SectorCount;
    public float MaxTorque;
    public float MaxPower;
    public int MaxRpm;
    public float MaxFuel;
    public TyreStat SuspensionMaxTravel;
    public TyreStat TyreRadius;
    public float MaxTurboBoost;
    public float Deprecated1;
    public float Deprecated2;
    public int PenaltiesEnabled;
    public float AidFuelRate;
    public float AidTireRate;
    public float AidMechanicalDamage;
    public int AidAllowTyreBlankets;
    public float AidStability;
    public int AidAutoClutch;
    public int AidAutoBlip;
    public int HasDRS;
    public int HasERS;
    public int HasKERS;
    public float KersMaxJoules;
    public int EngineBrakeSettingsCount;
    public int ErsPowerControllerCount;
    public float TrackSPlineLength;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
    public string TrackConfiguration;
    public float ErsMaxJ;
    public int IsTimedRace;
    public int HasExtraLap;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string CarSkin;
    public int ReversedGridPositions;
    public int PitWindowStart;
    public int PitWindowEnd;
    public int IsOnline;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string DryTyresName;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
    public string WetTyresName;
    
}


[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
[Serializable]
public struct acsVec3
{
    public float x;
    public float y;
    public float z;
}

[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
[Serializable]
public struct acsVehicleInfo
{
    public int carId;
    [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] driverName;
    [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] carModel;
    public float speedMS;
    public int bestLapMS;
    public int lapCount;
    public int currentLapInvalid;
    public int currentLapTimeMS;
    public int lastLapTimeMS;
    public acsVec3 worldPosition;
    public int isCarInPitline;
    public int isCarInPit;
    public int carLeaderboardPosition;
    public int carRealTimeLeaderboardPosition;
    public float spLineLength;
    public int isConnected;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] suspensionDamage;
    public float engineLifeLeft;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] tyreInflation;
}

//[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Ansi)]
//[Serializable]
//public struct SPageFileCrewChief
//{
//    public int numVehicles;
//    public int focusVehicle;
//    [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 512)]
//    public byte[] serverName;
//    [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
//    public acsVehicleInfo[] vehicle;
//    [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 512)]
//    public byte[] acInstallPath;
//    public int isInternalMemoryModuleLoaded;
//    [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
//    public byte[] pluginVersion;
//}