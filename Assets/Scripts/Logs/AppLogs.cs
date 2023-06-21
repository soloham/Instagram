namespace Assets.Scripts.Logs
{
    using System;
    using System.Collections.Generic;

    using UnityEngine;

    public class AppLog
    {
        public Guid SessionId;

        public DeviceInfo Device;
        public TimeInfo Time;
        public LocationInfo? Location;

        public static AppLog New(Guid SessionId, DeviceInfo deviceInfo, TimeInfo timeInfo, LocationInfo? locationInfo)
        {
            return new AppLog
            {
                SessionId = SessionId,
                Device = deviceInfo,
                Time = timeInfo,
                Location = locationInfo
            };
        }
    }

    public class AppLogs
    {
        public List<AppLog> Logs;
    }
}
