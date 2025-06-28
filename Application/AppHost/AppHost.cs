using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithPgAdmin().WithDataVolume("postgres-data");
var database = postgres.AddDatabase("database");

builder
    .AddProject<Projects.Web>("frontend")
    .WithReference(database)
    .WaitFor(database)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .PublishAsDockerFile(options =>
        options
            .WithDockerfile(contextPath: "../..", dockerfilePath: "./Application/Web/.containerfile")
            .WithImageTag("split")
    );

builder
    .AddDockerComposeEnvironment("docker-compose")
    .ConfigureComposeFile(options => options.AddNetwork(new() { Name = "proxy", External = true }));

builder.Build().Run();
