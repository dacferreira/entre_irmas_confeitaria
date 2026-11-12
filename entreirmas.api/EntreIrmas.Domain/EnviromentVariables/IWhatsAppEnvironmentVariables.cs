namespace EntreIrmas.Domain.EnviromentVariables;

public interface IWhatsAppEnvironmentVariables
{
    /// <summary>Número com DDI e DDD, só dígitos (ex.: 5521999999999).</summary>
    string WHATSAPP_NUMERO { get; }
}
