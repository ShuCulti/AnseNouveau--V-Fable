namespace AnseNouveau.Business
{
    // Sint Maarten is UTC-4 with no daylight saving; sale times are stored in UTC,
    // so a local business date maps to a shifted UTC range.
    public static class BusinessDay
    {
        public const int LocalUtcOffsetHours = -4;

        public static (DateTime FromUtc, DateTime ToUtc) ToUtcRange(DateTime businessDate)
        {
            DateTime fromUtc = businessDate.Date.AddHours(-LocalUtcOffsetHours);
            return (fromUtc, fromUtc.AddDays(1));
        }

        public static (DateTime FromUtc, DateTime ToUtc) ToUtcRange(DateTime fromDate, DateTime toDate)
        {
            DateTime fromUtc = fromDate.Date.AddHours(-LocalUtcOffsetHours);
            DateTime toUtc = toDate.Date.AddDays(1).AddHours(-LocalUtcOffsetHours);
            return (fromUtc, toUtc);
        }
    }
}
