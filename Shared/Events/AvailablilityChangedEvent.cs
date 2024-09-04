namespace Shared.Events
{
    public class AvailablilityChangedEvent
    {
        public string ProductId { get; set; }
        public bool IsAvailable { get; set; }
    }
}
