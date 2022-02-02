namespace Backend.Models.Requests
{
    /// <summary>
    /// Login request
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Login email to identify user
        /// </summary>
        public string Email { get; set; } = "";

        /// <summary>
        /// Password (usually already hashed by frontend)
        /// </summary>
        public string Password { get; set; } = "";
    }
}
