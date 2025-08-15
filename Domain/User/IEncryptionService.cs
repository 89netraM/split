namespace Split.Domain.User;

public interface IEncryptionService
{
    string Encrypt(string secret);
    string Decrypt(string blob);
}
