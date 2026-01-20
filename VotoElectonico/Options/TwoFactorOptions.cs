namespace VotoElectonico.Options;

public class TwoFactorOptions
{
    public int CodeLength { get; set; } = 6;
    public int ExpireMinutes { get; set; } = 5;
    public int MaxAttempts { get; set; } = 5;

    public string Subject { get; set; } = "Tu código de verificación";
}

