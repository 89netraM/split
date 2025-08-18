using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithPgAdmin().WithDataVolume("postgres-data");
var database = postgres.AddDatabase("database");

var backend = builder
    .AddProject<Projects.Api>("backend")
    .WithReference(database)
    .WaitFor(database)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .PublishAsDockerFile(options =>
        options
            .WithDockerfile(contextPath: "../..", dockerfilePath: "./Application/Api/.containerfile")
            .WithImageTag("split:api")
    );

builder
    .AddNpmApp("frontend", workingDirectory: "../Frontend", scriptName: "dev")
    .WithReference(backend)
    .WithHttpEndpoint(port: 5173, env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile(options =>
        options
            .WithDockerfile(contextPath: "../Frontend", dockerfilePath: "./.containerfile")
            .WithImageTag("split:frontend")
    );

builder.AddDockerComposeEnvironment("docker-compose");

builder.Build().Run();
