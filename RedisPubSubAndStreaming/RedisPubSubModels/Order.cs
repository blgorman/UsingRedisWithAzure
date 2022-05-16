namespace RedisPubSubModels
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public List<LineItem> LineItems { get; set; }
        public double OrderTotal => LineItems.Sum(x => x.LineItemTotal);
    }
}
