namespace RedisPubSubModels
{
    public class LineItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public double PricePerItem { get; set; }
        public double LineItemTotal => Quantity * PricePerItem;

    }
}
