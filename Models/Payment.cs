namespace WebKomunalka.Net8.Models;

public class Payment
{
    public int Id { get; set; } = 0;// 1
    
    public string Date { get; set; } = String.Empty;// 2
    
    public double AmountUsage { get; set; } = 0;// 3
    
    public double TotalPrice { get; set; } = 0;// 4

    public int ServiceId { get; set; } = 0;// 5

    public Service? Service { get; set; } = new Service();
    
    public string UserId { get; set; } = String.Empty;// 6
}