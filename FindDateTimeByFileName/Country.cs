using System.Diagnostics.Metrics;

namespace FindStuff
{
    public static class Current
    {
        public static Country Country = Country.Peru;
    }

    public enum Country
    {
        Ecuador,
        Peru,
        Bolivia,
        Chile,
        Argentina,
        NewZealand,
        Australia,
        Indonesia
    }
}
