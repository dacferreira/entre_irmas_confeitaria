namespace EntreIrmas.Test.Config;

/// <summary>Relógio parado, no fuso de Brasília, para as regras que dependem de "hoje".</summary>
internal sealed class TempoFixo : TimeProvider
{
    private readonly DateTimeOffset _agora;

    public TempoFixo(DateTimeOffset agora) => _agora = agora;

    public override DateTimeOffset GetUtcNow() => _agora.ToUniversalTime();

    public override TimeZoneInfo LocalTimeZone =>
        TimeZoneInfo.CreateCustomTimeZone("BRT", TimeSpan.FromHours(-3), "BRT", "BRT");
}
