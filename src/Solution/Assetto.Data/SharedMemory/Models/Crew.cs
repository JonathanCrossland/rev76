using System.Runtime.InteropServices;


// Structure for Participant Info (Driver Info)
[StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    public struct ParticipantInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] // Assuming max 64 characters for driver name
        public string Name;

        public bool IsConnected;
        public int CarModelIndex; // For car model, this could be another field
        // Add other fields as needed, such as Team name, car type, etc.
    }

    // Structure for Crew Info (Contains all participants)
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    public struct CrewInfo
    {
        public int NumParticipants; // Number of connected participants (drivers)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] // Max 64 participants in the session
        public ParticipantInfo[] Participants;
    }

    // Structure for SPageFileCrewChief (Full ACC Crew Data)
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    public struct SPageFileCrewChief
    {
        public int MaxClients; // Max number of clients in the session
        public CrewInfo Crew; // Contains all participant data

        // Add any other relevant fields here
    }

