using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;

namespace WarehouseSystem.Validators
{
    public interface IDataValidationService
    {
        Task<ValidationResult> ValidateDataInput<T>(T data);
        Task<ValidationResult> ValidateDataOutput<T>(T data);
        Task<bool> ValidateCSVFile(string filePath, string expectedHeader);
        Task<ValidationResult> ValidateBeforeSave<T>(T entity);
        Task<ValidationResult> ValidateCSVImport<T>(Stream fileStream, string expectedHeader);
        Task<ValidationResult> ValidateCSVExport<T>(IEnumerable<T> data);
    }

    public class DataValidationService : IDataValidationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DataValidationService> _logger;

        public DataValidationService(
            IServiceProvider serviceProvider,
            ILogger<DataValidationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<ValidationResult> ValidateDataInput<T>(T data)
        {
            var validator = _serviceProvider.GetService<IValidator<T>>();
            if (validator == null)
            {
                throw new InvalidOperationException($"No validator found for type {typeof(T).Name}");
            }

            var validationContext = new ValidationContext<T>(data);
            var validationResult = await validator.ValidateAsync(validationContext);

            var result = new ValidationResult();
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    result.AddError($"{error.PropertyName}: {error.ErrorMessage}");
                }
            }

            // Additional data format validations
            result = await ValidateDataFormats(data, result);

            return result;
        }

        public async Task<ValidationResult> ValidateDataOutput<T>(T data)
        {
            var result = new ValidationResult();

            try
            {
                // Validate required properties
                var properties = typeof(T).GetProperties();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(data);
                    if (IsRequiredProperty(prop) && value == null)
                    {
                        result.AddError($"Required property {prop.Name} is null");
                        continue;
                    }

                    // Validate string lengths for CSV compatibility
                    if (value is string strValue)
                    {
                        if (strValue.Length > 32767) // Excel CSV limit
                        {
                            result.AddError($"Property {prop.Name} exceeds maximum allowed length");
                        }
                        if (strValue.Contains('\n') || strValue.Contains('\r'))
                        {
                            result.AddError($"Property {prop.Name} contains invalid line breaks");
                        }
                    }

                    // Validate numeric formats
                    if (IsNumericProperty(prop) && value != null)
                    {
                        if (!IsValidNumericFormat(value))
                        {
                            result.AddError($"Property {prop.Name} has invalid numeric format");
                        }
                    }

                    // Validate date formats
                    if (IsDateProperty(prop) && value != null)
                    {
                        if (!IsValidDateFormat(value))
                        {
                            result.AddError($"Property {prop.Name} has invalid date format");
                        }
                    }
                }

                // Validate data relationships and constraints
                await ValidateDataRelationships(data, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during data output validation");
                result.AddError($"Validation error: {ex.Message}");
            }

            return result;
        }

        public async Task<bool> ValidateCSVFile(string filePath, string expectedHeader)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogError("CSV file not found: {FilePath}", filePath);
                    return false;
                }

                using var stream = File.OpenRead(filePath);
                using var reader = new StreamReader(stream);
                
                var header = await reader.ReadLineAsync();
                if (header?.Trim() != expectedHeader.Trim())
                {
                    _logger.LogError("CSV header mismatch");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CSV file validation failed");
                return false;
            }
        }

        public async Task<ValidationResult> ValidateBeforeSave<T>(T entity)
        {
            var result = new ValidationResult();

            try
            {
                // Basic validation
                var inputValidation = await ValidateDataInput(entity);
                if (!inputValidation.IsValid)
                {
                    return inputValidation;
                }

                // Business rules validation
                var businessValidator = _serviceProvider.GetService<IBusinessRuleValidator<T>>();
                if (businessValidator != null)
                {
                    var context = new ValidationContext("Save", "System");
                    var businessValidation = await businessValidator.ValidateAsync(entity, context);
                    if (!businessValidation.IsValid)
                    {
                        result.Errors.AddRange(businessValidation.Errors);
                        return result;
                    }
                }

                // Validate data integrity
                await ValidateDataIntegrity(entity, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Validation before save failed");
                result.AddError($"Validation error: {ex.Message}");
            }

            return result;
        }

        public async Task<ValidationResult> ValidateCSVImport<T>(Stream fileStream, string expectedHeader)
        {
            var result = new ValidationResult();

            try
            {
                using var reader = new StreamReader(fileStream);
                var header = await reader.ReadLineAsync();

                if (header?.Trim() != expectedHeader.Trim())
                {
                    result.AddError("CSV header does not match expected format");
                    return result;
                }

                var lineNumber = 1;
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lineNumber++;
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var validation = ValidateCSVLine<T>(line, lineNumber);
                    if (!validation.IsValid)
                    {
                        result.Errors.AddRange(validation.Errors);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CSV import validation failed");
                result.AddError($"Import validation error: {ex.Message}");
            }

            return result;
        }

        public async Task<ValidationResult> ValidateCSVExport<T>(IEnumerable<T> data)
        {
            var result = new ValidationResult();

            try
            {
                foreach (var item in data)
                {
                    var itemValidation = await ValidateDataOutput(item);
                    if (!itemValidation.IsValid)
                    {
                        result.Errors.AddRange(itemValidation.Errors);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CSV export validation failed");
                result.AddError($"Export validation error: {ex.Message}");
            }

            return result;
        }

        #region Private Helper Methods

        private bool IsRequiredProperty(PropertyInfo prop)
        {
            return prop.GetCustomAttributes(typeof(RequiredAttribute), true).Any() ||
                   prop.PropertyType.IsValueType && !IsNullableType(prop.PropertyType);
        }

        private bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private bool IsNumericProperty(PropertyInfo prop)
        {
            return prop.PropertyType == typeof(int) ||
                   prop.PropertyType == typeof(decimal) ||
                   prop.PropertyType == typeof(double) ||
                   prop.PropertyType == typeof(float);
        }

        private bool IsDateProperty(PropertyInfo prop)
        {
            return prop.PropertyType == typeof(DateTime) ||
                   prop.PropertyType == typeof(DateTime?);
        }

        private bool IsValidNumericFormat(object value)
        {
            return value switch
            {
                int _ => true,
                decimal d => d != decimal.MinValue && d != decimal.MaxValue,
                double d => !double.IsInfinity(d) && !double.IsNaN(d),
                float f => !float.IsInfinity(f) && !float.IsNaN(f),
                _ => false
            };
        }

        private bool IsValidDateFormat(object value)
        {
            if (value is DateTime date)
            {
                return date != DateTime.MinValue && date != DateTime.MaxValue;
            }
            return false;
        }

        private async Task ValidateDataRelationships<T>(T data, ValidationResult result)
        {
            // Example: Validate foreign key relationships
            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
            {
                var foreignKeyAttr = prop.GetCustomAttributes(typeof(ForeignKeyAttribute), true).FirstOrDefault();
                if (foreignKeyAttr != null)
                {
                    var value = prop.GetValue(data);
                    if (value != null)
                    {
                        if (!await ValidateForeignKeyReference(prop.Name, value))
                        {
                            result.AddError($"Invalid foreign key reference for {prop.Name}");
                        }
                    }
                }
            }
        }

        private async Task<bool> ValidateForeignKeyReference(string propertyName, object value)
        {
            // Implementation would depend on your data access layer
            // This is just a placeholder
            return await Task.FromResult(true);
        }

        private ValidationResult ValidateCSVLine<T>(string line, int lineNumber)
        {
            var result = new ValidationResult();
            var columns = ParseCSVLine(line);
            var properties = typeof(T).GetProperties();

            if (columns.Count != properties.Length)
            {
                result.AddError($"Invalid column count at line {lineNumber}");
                return result;
            }

            for (var i = 0; i < columns.Count; i++)
            {
                var prop = properties[i];
                var value = columns[i];

                if (IsRequiredProperty(prop) && string.IsNullOrEmpty(value))
                {
                    result.AddError($"Required field {prop.Name} is empty at line {lineNumber}");
                }

                if (!ValidateColumnDataType(prop, value))
                {
                    result.AddError($"Invalid data type for {prop.Name} at line {lineNumber}");
                }
            }

            return result;
        }

        private List<string> ParseCSVLine(string line)
        {
            var columns = new List<string>();
            var currentColumn = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                if (line[i] == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentColumn.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (line[i] == ',' && !inQuotes)
                {
                    columns.Add(currentColumn.ToString().Trim());
                    currentColumn.Clear();
                }
                else
                {
                    currentColumn.Append(line[i]);
                }
            }

            columns.Add(currentColumn.ToString().Trim());
            return columns;
        }

        private bool ValidateColumnDataType(PropertyInfo prop, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return !IsRequiredProperty(prop);
            }

            try
            {
                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                return type.Name switch
                {
                    "Int32" => int.TryParse(value, out _),
                    "Decimal" => decimal.TryParse(value, out _),
                    "DateTime" => DateTime.TryParse(value, out _),
                    "Boolean" => bool.TryParse(value, out _),
                    _ => true // String or other types
                };
            }
            catch
            {
                return false;
            }
        }

        private async Task ValidateDataIntegrity<T>(T entity, ValidationResult result)
        {
            // Implement data integrity checks specific to your domain
            // This is just a placeholder for demonstration
            await Task.CompletedTask;
        }

        private async Task<ValidationResult> ValidateDataFormats<T>(T data, ValidationResult result)
        {
            // Additional format validations can be added here
            await Task.CompletedTask;
            return result;
        }

        #endregion
    }
}
