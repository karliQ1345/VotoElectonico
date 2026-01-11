namespace VotoElectonico.Options
{
    public class TwoFactorOptions
    {
        public int CodeTtlMinutes { get; set; } = 5;
        public int MaxAttempts { get; set; } = 5;
        public string Pepper { get; set; } = default!;
    }
}
