using System.Security.Cryptography;
using System.Text;

namespace CleanERP.Shared.Utilities;

public static class PasswordHelper
{
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        var hashedInput = HashPassword(password);
        return hashedInput == hashedPassword;
    }

    public static string GenerateRandomPassword(int length = 12)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

public static class CodeGenerator
{
    public static string GenerateProductCode(string prefix = "PRD")
    {
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"{prefix}-{timestamp}-{random}";
    }

    public static string GenerateOrderNumber(string prefix = "ORD")
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd");
        var random = new Random().Next(10000, 99999);
        return $"{prefix}-{timestamp}-{random}";
    }

    public static string GenerateInvoiceNumber(string prefix = "INV")
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd");
        var random = new Random().Next(10000, 99999);
        return $"{prefix}-{timestamp}-{random}";
    }

    public static string GenerateBarcode()
    {
        var random = new Random();
        var barcode = new StringBuilder();
        
        for (int i = 0; i < 12; i++)
        {
            barcode.Append(random.Next(0, 10));
        }
        
        return barcode.ToString();
    }
}

public static class ValidationHelper
{
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        // Remove all non-digit characters
        var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());
        
        // Check if it has 10-15 digits (common phone number range)
        return digitsOnly.Length >= 10 && digitsOnly.Length <= 15;
    }

    public static bool IsValidSKU(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return false;

        // SKU should contain only alphanumeric characters, hyphens, and underscores
        return sku.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
    }
}

public static class DateTimeHelper
{
    public static DateTime GetStartOfDay(DateTime date)
    {
        return date.Date;
    }

    public static DateTime GetEndOfDay(DateTime date)
    {
        return date.Date.AddDays(1).AddTicks(-1);
    }

    public static DateTime GetStartOfWeek(DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    public static DateTime GetEndOfWeek(DateTime date, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        return GetStartOfWeek(date, startOfWeek).AddDays(7).AddTicks(-1);
    }

    public static DateTime GetStartOfMonth(DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    public static DateTime GetEndOfMonth(DateTime date)
    {
        return GetStartOfMonth(date).AddMonths(1).AddTicks(-1);
    }

    public static int GetBusinessDaysBetween(DateTime startDate, DateTime endDate)
    {
        if (startDate >= endDate)
            return 0;

        int businessDays = 0;
        var current = startDate;

        while (current < endDate)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                businessDays++;
            
            current = current.AddDays(1);
        }

        return businessDays;
    }
}
