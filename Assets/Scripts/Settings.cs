namespace Assets.Scripts
{
    using System.Collections.Generic;

    public class Settings
    {
        public int Version { get; set; }

        public bool ForceLoadMessage { get; set; }

        public bool RandomiseFeed { get; set; }

        public List<HomeFeed> Feeds { get; set; }
    }

    public class HomeFeed
    {
        public string FeedUID { get; set; }

        public float Width { get; set; }
        public float Height { get; set; }
    }
}
