using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;

namespace WarehouseSystem.Application.Validators
{
    public abstract class InputValidatorBase
    {
        protected readonly ValidationResult ValidationResult;

        protected InputValidatorBase()
        {
            ValidationResult = new ValidationResult();
        }

        // Pagrindiniai validacijos metodai
        protected bool ValidateRequired(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                ValidationResult.AddError($"Laukas '{fieldName}' yra privalomas");
                return false;
            }
            return true;
        }

        protected bool ValidateLength(string value, string fieldName, int maxLength, int minLength = 1)
        {
            if (string.IsNullOrEmpty(value)) return true; // Jei neprivalomas laukas

            if (value.Length < minLength || value.Length > maxLength)
            {
                ValidationResult.AddError(
                    $"Lauko '{fieldName}' ilgis turi būti tarp {minLength} ir {maxLength} simbolių");
                return false;
            }
            return true;
        }

        protected bool ValidatePattern(string value, string pattern, string fieldName, string errorMessage)
        {
            if (string.IsNullOrEmpty(value)) return true; // Jei neprivalomas laukas

            if (!Regex.IsMatch(value, pattern))
            {
                ValidationResult.AddError($"Laukas '{fieldName}' {errorMessage}");
                return false;
            }
            return true;
        }

        protected bool ValidateNumericRange<T>(T value, string fieldName, T? min = null, T? max = null) 
            where T : struct, IComparable<T>
        {
            if (min.HasValue && value.CompareTo(min.Value) < 0)
            {
                ValidationResult.AddError($"Lauko '{fieldName}' reikšmė negali būti mažesnė už {min.Value}");
                return false;
            }

            if (max.HasValue && value.CompareTo(max.Value) > 0)
            {
                ValidationResult.AddError($"Lauko '{fieldName}' reikšmė negali būti didesnė už {max.Value}");
                return false;
            }

            return true;
        }

        protected bool ValidateDate(DateTime? value, string fieldName, DateTime? minDate = null, DateTime? maxDate = null)
        {
            if (!value.HasValue) return true; // Jei neprivaloma data

            if (minDate.HasValue && value < minDate)
            {
                ValidationResult.AddError($"Data '{fieldName}' negali būti ankstesnė nei {minDate.Value:yyyy-MM-dd}");
                return false;
            }

            if (maxDate.HasValue && value > maxDate)
            {
                ValidationResult.AddError($"Data '{fieldName}' negali būti vėlesnė nei {maxDate.Value:yyyy-MM-dd}");
                return false;
            }

            return true;
        }

        protected bool ValidateDecimal(decimal value, string fieldName, int maxDecimals = 2)
        {
            var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(value)[3])[2];
            if (decimalPlaces > maxDecimals)
            {
                ValidationResult.AddError(
                    $"Lauko '{fieldName}' reikšmė negali turėti daugiau nei {maxDecimals} skaičių po kablelio");
                return false;
            }
            return true;
        }

        protected bool ValidateEnum<T>(string value, string fieldName) where T : struct
        {
            if (string.IsNullOrEmpty(value)) return true; // Jei neprivalomas laukas

            if (!Enum.TryParse<T>(value, true, out _))
            {
                var validValues = string.Join(", ", Enum.GetNames(typeof(T)));
                ValidationResult.AddError(
                    $"Neteisinga '{fieldName}' reikšmė. Galimos reikšmės: {validValues}");
                return false;
            }
            return true;
        }

        // Specifiniai validacijos metodai
        protected bool ValidateEANCode(string eanCode, string fieldName = "EAN kodas")
        {
            if (string.IsNullOrEmpty(eanCode)) return true; // Jei neprivalomas

            // EAN-13 validacija
            if (!Regex.IsMatch(eanCode, @"^\d{13}$"))
            {
                ValidationResult.AddError($"{fieldName} turi būti sudarytas iš 13 skaitmenų");
                return false;
            }

            // Kontrolinės sumos tikrinimas
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int digit = eanCode[i] - '0';
                sum += digit * (i % 2 == 0 ? 1 : 3);
            }

            int checkDigit = (10 - (sum % 10)) % 10;
            if (checkDigit != (eanCode[12] - '0'))
            {
                ValidationResult.AddError($"Neteisingas {fieldName} kontrolinis skaitmuo");
                return false;
            }

            return true;
        }

        protected bool ValidateQRCode(string qrCode, string fieldName = "QR kodas")
        {
            if (string.IsNullOrEmpty(qrCode)) return true; // Jei neprivalomas

            // QR kodo formato validacija
            if (!Regex.IsMatch(qrCode, @"^[A-Za-z0-9\-_]{1,50}$"))
            {
                ValidationResult.AddError(
                    $"{fieldName} gali būti sudarytas tik iš raidžių, skaičių, brūkšnelių ir pabraukimų, " +
                    "maksimalus ilgis - 50 simbolių");
                return false;
            }

            return true;
        }

        protected bool ValidateBatchNumber(string batchNumber, string fieldName = "Partijos numeris")
        {
            if (string.IsNullOrEmpty(batchNumber)) return true; // Jei neprivalomas

            // Formato validacija: YYYYMMDD-XXX
            if (!Regex.IsMatch(batchNumber, @"^\d{8}-[A-Z0-9]{3}$"))
            {
                ValidationResult.AddError(
                    $"{fieldName} turi atitikti formatą YYYYMMDD-XXX " +
                    "(data ir trys simboliai po brūkšnelio)");
                return false;
            }

            // Datos validacija
            var dateStr = batchNumber.Substring(0, 8);
            if (!DateTime.TryParseExact(dateStr, "yyyyMMdd", 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                ValidationResult.AddError($"Neteisinga data {fieldName}");
                return false;
            }

            if (date > DateTime.Now)
            {
                ValidationResult.AddError($"{fieldName} data negali būti ateityje");
                return false;
            }

            return true;
        }

        protected bool ValidateLocationCode(string locationCode, string fieldName = "Vietos kodas")
        {
            if (string.IsNullOrEmpty(locationCode)) return true; // Jei neprivalomas

            // Formato validacija: ZONA-PRAEJIMAS-LENTYNA-VIETA-DEZE
            var pattern = @"^[A-Z0-9]+-[A-Z0-9]+-[A-Z0-9]+-[A-Z0-9]+(-[A-Z0-9]+)?$";
            if (!Regex.IsMatch(locationCode, pattern))
            {
                ValidationResult.AddError(
                    $"{fieldName} turi atitikti formatą ZONA-PRAEJIMAS-LENTYNA-VIETA(-DEZE), " +
                    "kur kiekviena dalis gali būti sudaryta tik iš didžiųjų raidžių ir skaičių");
                return false;
            }

            return true;
        }

        // Pagalbiniai metodai
        protected bool IsValidDate(string value)
        {
            return DateTime.TryParse(value, out _);
        }

        protected bool IsValidDecimal(string value)
        {
            return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
        }

        protected bool IsValidInteger(string value)
        {
            return int.TryParse(value, out _);
        }
    }
}
