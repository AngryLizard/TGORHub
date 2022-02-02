namespace Backend.Models.Requests
{
    /// <summary>
    /// Registration request
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Desired display name
        /// </summary>
        public string Username { get; set; } = "";

        /// <summary>
        /// Email used by the user (unique per database)
        /// </summary>
        public string Email { get; set; } = "";

        /// <summary>
        /// Password to be used (usually already hashed by frontend)
        /// </summary>
        public string Password { get; set; } = "";
    }
}
