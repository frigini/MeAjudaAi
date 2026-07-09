namespace MeAjudaAi.Integration.Tests.Infrastructure;

/// <summary>
/// DTOs compartilhados para testes de integração
/// </summary>
/// 
public sealed record LoginResponseDto(LoginDataDto Data);

public sealed record LoginDataDto(string Token);
