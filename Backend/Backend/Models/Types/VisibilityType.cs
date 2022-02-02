namespace Backend.Models.Assets
{
    /// <summary>
    /// Outsider/Insider Access assets
    /// </summary>
    public enum VisibilityType
    {
        /// <summary>
        /// Visible without user account
        /// </summary>
        Public = 0,

        /// <summary>
        /// Need special access to view
        /// </summary>
        Private = 1
    }
}
