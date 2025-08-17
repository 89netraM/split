using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithPgAdmin().WithDataVolume("postgres-data");
var database = postgres.AddDatabase("database");

// builder
//     .AddProject<Projects.Web>("all-in-one-frontend")
//     .WithReference(database)
//     .WaitFor(database)
//     .WithExternalHttpEndpoints()
//     .WithHttpHealthCheck("/health")
//     .PublishAsDockerFile(options =>
//         options
//             .WithDockerfile(contextPath: "../..", dockerfilePath: "./Application/Web/.containerfile")
//             .WithImageTag("split")
//     );

var backend = builder
    .AddProject<Projects.Api>("backend")
    .WithReference(database)
    .WaitFor(database)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder
    .AddNpmApp("frontend", workingDirectory: "../Frontend", scriptName: "dev")
    .WithReference(backend)
    .WithHttpEndpoint(port: 5173, env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder
    .AddDockerComposeEnvironment("docker-compose")
    .ConfigureComposeFile(options => options.AddNetwork(new() { Name = "proxy", External = true }));

builder.Build().Run();
