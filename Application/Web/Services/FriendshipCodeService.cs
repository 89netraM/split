using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using QRCoder;
using Split.Domain.Primitives;
using Split.Infrastructure.EncryptedRequests;
using static QRCoder.QRCodeGenerator;

namespace Split.Application.Web.Services;

public class FriendshipCodeService(RequestEncryptor requestEncryptor, QRCodeGenerator qrCodeGenerator)
{
    public string GetFriendshipQrCodeSvg(Uri friendshipPage, UserId userId)
    {
        var request = requestEncryptor.EncodeFriendRequest(userId);
        var payload = QueryHelpers.AddQueryString(
            friendshipPage.ToString(),
            [new KeyValuePair<string, string?>("addFriend", request)]
        );
        var qrCodeData = qrCodeGenerator.CreateQrCode(payload, ECCLevel.Q);
        var qrCode = new SvgQRCode(qrCodeData);
        return qrCode.GetGraphic(1, "#000000", "#FFFFFF", sizingMode: SvgQRCode.SizingMode.ViewBoxAttribute);
    }

    public UserId? DecodeFriendshipCode(string code) => requestEncryptor.DecodeFriendRequest(code);
}

public static class FriendshipCodeServiceServiceCollectionExtensions
{
    public static IServiceCollection AddFriendshipCodeService(this IServiceCollection services) =>
        services
            .AddRequestEncryptor()
            .AddSingleton<RequestEncryptor>()
            .AddSingleton<QRCodeGenerator>()
            .AddSingleton<FriendshipCodeService>();
}
