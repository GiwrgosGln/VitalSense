namespace VitalSense.Shared.Services;

public interface IEncryptionService
{
    string Protect(string plainText);
    string Unprotect(string protectedText);
}