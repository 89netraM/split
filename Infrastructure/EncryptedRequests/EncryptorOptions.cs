using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace Split.Infrastructure.EncryptedRequests;

public class EncryptorOptions
{
    [Required]
    public required string Key { get; set; }

    [Required]
    public required string Iv { get; set; }

    public TimeSpan FriendRequestTimeout { get; set; } = TimeSpan.FromMinutes(5);
}

[OptionsValidator]
public partial class EncryptorOptionsValidator : IValidateOptions<EncryptorOptions>;
