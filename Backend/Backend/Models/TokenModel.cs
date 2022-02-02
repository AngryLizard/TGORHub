namespace Backend.Models
{
    /// <summary>
    /// Token returned after login
    /// </summary>
    public class TokenModel
    {
        /// <summary>
        /// JWT token generated after login
        /// </summary>
        public string Token { get; set; } = "";
    }
}
