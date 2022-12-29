using System;

namespace AngryWasp.Helpers
{
    public class DateTimeHelper
    {
        /// <summary>
        /// Gets the current UTC time as a Unix timestamp
        /// </summary>
        /// <returns>The current UTC time as a Unix timestamp</returns>
        public static ulong TimestampNow =>
            (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

        /// <summary>
        /// Converts a Unix timestamp to a DateTime structure
        /// </summary>
        /// <param name="ts">The UTC timestamp to convert</param>
        /// <returns>A UTC DateTime object representing the timestamp</returns>
        public static DateTime UnixTimestampToDateTime(ulong utcTimestamp, DateTimeKind kind = DateTimeKind.Utc) =>
            new DateTime(1970, 1, 1, 0, 0, 0, 0, kind).AddSeconds(utcTimestamp);

        /// <summary>
        /// Converts a DateTime to a Timestamp
        /// </summary>
        /// <param name="dt">The DateTime to convert. Automatically converted to UTC</param>
        /// <returns>The UTC timestamp representation of dt</returns>
        public static ulong DateTimeToUnixTimestamp(DateTime dt) =>
            (ulong)(dt.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }
}