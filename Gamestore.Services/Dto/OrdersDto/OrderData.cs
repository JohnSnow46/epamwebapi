namespace Gamestore.Services.Dto.OrdersDto;
public class OrderData
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public DateTime? OrderDate { get; set; }
    public string DateString { get; set; } = string.Empty;
}