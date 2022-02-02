
using System.Text.Json.Serialization;

namespace Backend.Models
{
    /// <summary>
    /// User extension for credentials used by login.
    /// These credentials should preferably never be returned by any API request.
    /// </summary>
    public class CredentialsModel : UserModel
    {
        /// <summary>
        /// Email used to identify user. This email is unique in the user database.
        /// </summary>
        public string Email { get; set; } = "";

        /// <summary>
        /// Hashed password used to login
        /// </summary>
        public string Password { get; set; } = "";
    }
}
