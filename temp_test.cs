using System;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Tests.Builders;

var provider = new ProviderBuilder().Build();
Console.WriteLine(\$\"Initial Status: {provider.Status}\");

provider.CompleteBasicInfo();
Console.WriteLine(\$\"After CompleteBasicInfo: {provider.Status}\");

provider.Suspend(\"Test reason\", \"admin@test.com\");
Console.WriteLine(\$\"After Suspend: {provider.Status}\");
