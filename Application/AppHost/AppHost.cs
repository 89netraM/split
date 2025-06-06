using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Web>("frontend").WithExternalHttpEndpoints().WithHttpHealthCheck("/health");

builder.Build().Run();
