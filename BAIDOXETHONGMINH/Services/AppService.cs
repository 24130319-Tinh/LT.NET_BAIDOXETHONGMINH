namespace BAIDOXETHONGMINH.GUI;

public class AppService
{
    public static PricingService Pricing { get;}=new ();
    public static ParkingCardService Card { get; } = new();
    public static ParkingMapService Map { get;}=new ();
    public static CustomerService Customers { get;}=new ();
    public static MonthlyPassService Monthly { get;}=new (Pricing);
    


}