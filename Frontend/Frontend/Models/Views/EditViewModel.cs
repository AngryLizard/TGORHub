namespace Frontend.Models
{
    public class EditViewModel
    {
        public long ProjectId { get; set; } = 0;
        public IEnumerable<int> Target { get; set; } = new List<int>();
    }
}