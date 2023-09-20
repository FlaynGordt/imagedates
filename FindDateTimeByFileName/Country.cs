using System.Diagnostics.Metrics;

namespace FindStuff
{
    public static class Current
    {
        public static Country Country = Country.Ecuador;
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
