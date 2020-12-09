namespace SideLoader
{
    public enum CharSaveType
    {
        /// <summary>Not saved, created from scratch each time.</summary>
        Temporary,
        /// <summary>Saved to a specific scene, wiped on scene reset.</summary>
        Scene,
        /// <summary>Saved as a persistent follower such as a Summoned Ally, will change scenes with players until destroyed by you.</summary>
        Follower
    }
}
