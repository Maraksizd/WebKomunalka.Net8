namespace WebKomunalka.Net8.Models;

public class Payment
{
    public int Id { get; set; } = 0;
    
    public string Date { get; set; } = String.Empty;
    
    public double AmountUsage { get; set; } = 0;
    
    public double TotalPrice { get; set; } = 0;

    public int ServiceId { get; set; } = 0;

    public Service? Service { get; set; } = new Service();
}