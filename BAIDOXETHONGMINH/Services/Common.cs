namespace BAIDOXETHONGMINH.Services;

public class ServiceException : Exception
{
    public ServiceException(string message) : base(message) { }
    
}

public static class PassStatus
{
    public const string Active = "Active";
    public const string Expired = "Expired";
    public const string Cancelled = "Cancelled";
}
public static class CardStatus
{
    public const string Available = "Available";
    public const string InUse = "InUse";
    public const string Lost =  "Lost";
    public const string Locked = "Locked";
}

public static class SlotStatus
{
    public const string Available = "Available";
    public const string Occupied = "Occupied";
    public const string Maintenance = "Maintenance";
}

