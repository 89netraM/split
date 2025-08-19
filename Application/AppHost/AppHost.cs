using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithPgAdmin().WithDataVolume("postgres-data");
var database = postgres.AddDatabase("database");

var backend = builder
    .AddProject<Projects.Api>("backend", options => options.ExcludeLaunchProfile = true)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithHttpEndpoint(env: "ASPNETCORE_HTTP_PORTS", port: 5223, isProxied: false)
    .WithReference(database)
    .WaitFor(database)
    .WithHttpHealthCheck("/health")
    .PublishAsDockerFile(options =>
        options
            .WithDockerfile(contextPath: "../..", dockerfilePath: "./Application/Api/.containerfile")
            .WithImageTag("split:api")
    );

var frontend = builder
    .AddNpmApp("frontend", workingDirectory: "../Frontend", scriptName: "dev", args: ["--host"])
    .WithReference(backend)
    .WithHttpEndpoint(env: "PORT", port: 5173, isProxied: false)
    .PublishAsDockerFile(options =>
        options
            .WithDockerfile(contextPath: "../Frontend", dockerfilePath: "./.containerfile")
            .WithImageTag("split:frontend")
    );

builder
    .AddYarp("gateway")
    .WithExternalHttpEndpoints()
    .WithHostPort(8934)
    .WithContainerRuntimeArgs(
        $"--add-host=host.containers.internal:{GetLocalIp()}",
        $"--add-host=host.docker.internal:{GetLocalIp()}"
    )
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute(frontend);
        yarp.AddRoute("/api/{**catch-all}", backend);
    });

builder.AddDockerComposeEnvironment("docker-compose");

builder.Build().Run();

static IPAddress GetLocalIp() =>
    Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily is AddressFamily.InterNetwork)
    ?? throw new Exception("Could not determine local IP address.");
