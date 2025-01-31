namespace Assetto.Data.Broadcasting.Structs
{
    public struct DriverInfo
    {
        public string FirstName { get;  set; }
        public string LastName { get;  set; }
        public string ShortName { get;  set; }
        public DriverCategory Category { get;  set; }
        public NationalityEnum Nationality { get;  set; }
    }
}
