using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace Split.Infrastructure.PhoneNumberVerifier;

internal sealed class SmsSenderOptions
{
    [Required]
    public required Uri BaseAddress { get; set; }
}

[OptionsValidator]
internal sealed partial class SmsSenderOptionsValidator : IValidateOptions<SmsSenderOptions>;
