using System;
using System.Text.Json.Serialization;
using Split.Domain.Primitives;

namespace Split.Infrastructure.EncryptedRequests;

internal record FriendRequest(UserId UserId, DateTimeOffset IssuedAt);

[JsonSerializable(typeof(FriendRequest))]
internal partial class FriendRequestSerializer : JsonSerializerContext;
