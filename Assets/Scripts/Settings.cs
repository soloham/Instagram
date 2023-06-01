namespace Assets.Scripts
{
    using System.Collections.Generic;

    public class Settings
    {
        public int Version;

        public bool ForceLoadMessage;

        public bool AllowEditing;

        public bool RandomiseFeed;

        public bool VerboseSplashScreen;

        public float HomeScreenScrollYFactor;

        public float MinimumMessagePhotoScallingRatio;

        public float MessagePhotoWidthScallingRatioFactor;

        public float MessagePhotoHeightScallingRatioFactor;

        public List<HomeFeed> Feeds;
    }

    public class HomeFeed
    {
        public string FeedUID;

        public float Width;
        public float Height;
    }
}
