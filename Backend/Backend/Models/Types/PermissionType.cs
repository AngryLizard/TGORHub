namespace Backend.Models.Assets
{
    /// <summary>
    /// Access permissions to different parts of this API.
    /// Permissions can be combined.
    /// </summary>
    [Flags]
    public enum PermissionType
    {
        /// <summary>
        /// No access
        /// </summary>
        None =      0,

        /// <summary>
        /// create and edit projects (most users have this, could safeguard this with email verification)
        /// </summary>
        User =      1 << 0,

        /// <summary>
        /// view/edit/remove users
        /// </summary>
        Accounts =  1 << 1,

        /// <summary>
        /// edit website-wide relevant content
        /// </summary>
        Public =    1 << 2,

        /// <summary>
        /// edit assets and features
        /// </summary>
        Content =   1 << 3,

        /// <summary>
        /// administrate user generated content
        /// </summary>
        Admin =     1 << 4,

        /// <summary>
        /// separate since files may be either user-generated or content
        /// </summary>
        Files =     1 << 5,

        /// <summary>
        /// avoid deletion of things under all cost to maintain backwards compatibility
        /// </summary>
        Delete =    1 << 6,

        /// <summary>
        /// Access to everything
        /// </summary>
        All = -1
    }
}
