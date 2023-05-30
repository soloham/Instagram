namespace Assets.Scripts
{
    using System.Collections.Generic;

    public class Settings
    {
        public int Version;

        public bool ForceLoadMessage;

        public bool AllowEditing;

        public bool RandomiseFeed;

        public List<HomeFeed> Feeds;
    }

    public class HomeFeed
    {
        public string FeedUID;

        public float Width;
        public float Height;
    }
}
