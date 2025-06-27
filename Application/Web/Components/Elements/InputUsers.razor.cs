using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace Split.Application.Web.Components.Elements;

public partial class InputUsers : ComponentBase
{
    [GeneratedRegex(@"^\+\d+$")]
    private partial Regex PhoneNumberValidation { get; }
}
