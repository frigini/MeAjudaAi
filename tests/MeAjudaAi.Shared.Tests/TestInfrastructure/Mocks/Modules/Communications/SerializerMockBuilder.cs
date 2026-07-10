using MeAjudaAi.Modules.Communications.Application.DTOs;
using MeAjudaAi.Shared.Serialization;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Modules.Communications;

public class SerializerMockBuilder
{
    public Mock<ISerializer> Mock { get; } = new Mock<ISerializer>();

    public SerializerMockBuilder SetupDefault()
    {
        Mock.Setup(x => x.Serialize(It.IsAny<It.IsAnyType>())).Returns("dummy_payload");

        Mock.Setup(x => x.Deserialize<EmailOutboxPayload>(It.IsAny<string>()))
            .Returns(EmailOutboxPayload.Create(to: "test@test.com", subject: "Hi", htmlBody: "Hello"));
        Mock.Setup(x => x.Deserialize<SmsOutboxPayload>(It.IsAny<string>()))
            .Returns(new SmsOutboxPayload("+5511999999999", "Hello"));
        Mock.Setup(x => x.Deserialize<PushOutboxPayload>(It.IsAny<string>()))
            .Returns(new PushOutboxPayload("token123", "Hi", "Hello", null));
        Mock.Setup(x => x.Deserialize<EmailOutboxPayload>("html_payload"))
            .Returns(EmailOutboxPayload.Create(to: "t@t.com", subject: "S", templateKey: "welcome_template", templateData: new Dictionary<string, string> { { "FirstName", "John" } }));
        Mock.Setup(x => x.Deserialize<EmailOutboxPayload>("body_payload"))
            .Returns(EmailOutboxPayload.Create(to: "t@t.com", subject: "S", htmlBody: "<b>B</b>"));
        Mock.Setup(x => x.Deserialize<EmailOutboxPayload>("raw_body_payload"))
            .Returns(EmailOutboxPayload.Create(to: "t@t.com", subject: "S", htmlBody: "<b>Raw Body</b>"));
        Mock.Setup(x => x.Deserialize<EmailOutboxPayload>("html_body_payload"))
            .Returns(EmailOutboxPayload.Create(to: "t@t.com", subject: "S", htmlBody: "<h1>H</h1>"));

        return this;
    }

    public ISerializer Build() => Mock.Object;
}
