using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient("resilient", client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("dotnet-8-demo");
        })
        .AddStandardResilienceHandler(options =>
        {
            options.TotalRequestTimeout = new HttpTimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(20)
            };
            options.Retry.MaxRetryAttempts = 3; // 👈 hasta 3 retries
            options.Retry.Delay = TimeSpan.FromSeconds(1); // pausa de 1s entre intentos
            options.CircuitBreaker.MinimumThroughput = 2;  // 👈 requiere 2 intentos para evaluar
            options.CircuitBreaker.FailureRatio = 0.5;     // si el 50% falla → abre circuito
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(10); // queda abierto 10s
        });

        services.AddScoped<HttpService>();
        services.AddScoped<App>();
    })
    .Build();

// Ejecutar app
await host.Services.GetRequiredService<App>().RunAsync();