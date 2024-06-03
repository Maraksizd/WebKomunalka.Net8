namespace WebKomunalka.Net8.Models;

public class HomeViewModel
{
    public double TotalCost { get; set; }
    public List<BigPayment> BigPayments { get; set; }

    public class BigPayment
    {
        public Payment Payment { get; set; }
        public double Percentage { get; set; }
    }
}