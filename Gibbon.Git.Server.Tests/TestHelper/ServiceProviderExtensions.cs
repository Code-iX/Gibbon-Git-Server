using System;

using Microsoft.Extensions.DependencyInjection;

namespace Gibbon.Git.Server.Tests.TestHelper;

public static class ServiceProviderExtensions
{
    public static T AddSubstitute<T>(this ServiceCollection services, Action<T>? configure = null) where T : class
    {
        var substitute = NSubstitute.Substitute.For<T>();
        services.AddSingleton(substitute);
        configure?.Invoke(substitute);
        return substitute;
    }
}
