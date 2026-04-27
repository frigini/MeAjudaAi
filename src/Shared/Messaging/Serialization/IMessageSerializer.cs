namespace MeAjudaAi.Shared.Messaging.Serialization;

/// <summary>
/// Abstração para serialização de mensagens para permitir troca entre System.Text.Json e Newtonsoft.Json
/// </summary>
public interface IMessageSerializer
{
    string Serialize<T>(T obj);
    T? Deserialize<T>(string json);
}
