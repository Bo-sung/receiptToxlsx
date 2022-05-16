namespace Ragnarok
{
    public enum ToggleType
    {
        /// <summary>
        /// DisplayName     {Toggle}
        /// </summary>
        [System.Obsolete("Use RenameAttribute instead.")]
        Default,

        /// <summary>
        /// {Toggle} DisplayName
        /// </summary>
        Legacy,

        /// <summary>
        /// DisplayName     {yes} or {no}
        /// </summary>
        OnOff,

        /// <summary>
        /// DisplayName     {yes|no}
        /// </summary>
        Toolbar,

        /// <summary>
        /// {yes|no}
        /// </summary>
        NoNameToolbar,
    }
}