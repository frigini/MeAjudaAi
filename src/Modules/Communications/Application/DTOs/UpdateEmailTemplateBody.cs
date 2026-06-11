namespace MeAjudaAi.Modules.Communications.Application.DTOs;

public sealed record UpdateEmailTemplateBody(string Subject, string HtmlBody, string TextBody);
