namespace Backend.Models.Requests
{
    /// <summary>
    /// Request used for payload creation
    /// </summary>
    public class CreatePayloadRequest : CreateEntityRequest
    {   
        /// <summary>
        /// Data for this payload
        /// </summary>
        public PayloadData Data { get; set; } = new PayloadData();
    }
}
