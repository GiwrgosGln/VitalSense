using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace VitalSense.Shared.Services;

public class EncryptionService : IEncryptionService
{
    private readonly IDataProtector _protector;

    public EncryptionService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("VitalSense.QuestionnaireAnswerProtection");
    }

    public string Protect(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        return _protector.Protect(plainText);
    }

    public string Unprotect(string protectedText)
    {
        if (string.IsNullOrEmpty(protectedText))
            return protectedText;

        return _protector.Unprotect(protectedText);
    }
}