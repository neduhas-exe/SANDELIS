using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;

namespace WarehouseSystem.Application.Validation.BusinessRules
{
    public interface IValidationHistoryRepository
    {
        Task SaveValidationResult(ValidationHistoryRecord record);
        Task<IEnumerable<ValidationHistoryRecord>> GetValidationHistory(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<ValidationHistoryRecord>> GetValidationHistoryByType(string entityType, int entityId);
        Task<ValidationHistoryRecord> GetLastValidation(string entityType, int entityId);
    }

    public class ValidationHistoryRepository : IValidationHistoryRepository
    {
        private readonly string _historyFilePath;
        private static readonly SemaphoreSlim _lock = new(1, 1);

        public ValidationHistoryRepository()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _historyFilePath = Path.Combine(baseDirectory, "Data", "validation_history.csv");
            
            // Sukuriame direktoriją jei jos nėra
            Directory.CreateDirectory(Path.Combine(baseDirectory, "Data"));
            
            // Sukuriame failą jei jo nėra
            if (!File.Exists(_historyFilePath))
            {
                File.WriteAllText(_historyFilePath, GetCsvHeader());
            }
        }

        public async Task SaveValidationResult(ValidationHistoryRecord record)
        {
            await _lock.WaitAsync();
            try
            {
                var line = ToCsvLine(record);
                await File.AppendAllLinesAsync(_historyFilePath, new[] { line });
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<IEnumerable<ValidationHistoryRecord>> GetValidationHistory(
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            var records = new List<ValidationHistoryRecord>();
            await _lock.WaitAsync();
            try
            {
                var lines = await File.ReadAllLinesAsync(_historyFilePath);
                foreach (var line in lines.Skip(1)) // Praleidžiame antraštę
                {
                    var record = FromCsvLine(line);
                    if (record != null && 
                        (!startDate.HasValue || record.ValidationDate >= startDate) &&
                        (!endDate.HasValue || record.ValidationDate <= endDate))
                    {
                        records.Add(record);
                    }
                }
            }
            finally
            {
                _lock.Release();
            }
            return records;
        }

        public async Task<IEnumerable<ValidationHistoryRecord>> GetValidationHistoryByType(
            string entityType, 
            int entityId)
        {
            var records = await GetValidationHistory();
            return records.Where(r => 
                r.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase) && 
                r.EntityId == entityId);
        }

        public async Task<ValidationHistoryRecord> GetLastValidation(string entityType, int entityId)
        {
            var records = await GetValidationHistoryByType(entityType, entityId);
            return records.OrderByDescending(r => r.ValidationDate).FirstOrDefault();
        }

        private string GetCsvHeader() =>
            "ValidationDate,EntityType,EntityId,ValidationAction,IsSuccess,ErrorCount,WarningCount,Messages,ValidatedByUser";

        private string ToCsvLine(ValidationHistoryRecord record)
        {
            return $"{record.ValidationDate:yyyy-MM-dd HH:mm:ss}," +
                   $"\"{EscapeCsvField(record.EntityType)}\"," +
                   $"{record.EntityId}," +
                   $"\"{EscapeCsvField(record.ValidationAction)}\"," +
                   $"{record.IsSuccess}," +
                   $"{record.ErrorCount}," +
                   $"{record.WarningCount}," +
                   $"\"{EscapeCsvField(record.Messages)}\"," +
                   $"\"{EscapeCsvField(record.ValidatedByUser)}\"";
        }

        private ValidationHistoryRecord FromCsvLine(string line)
        {
            try
            {
                var parts = line.Split(',');
                return new ValidationHistoryRecord
                {
                    ValidationDate = DateTime.Parse(parts[0]),
                    EntityType = UnescapeCsvField(parts[1]),
                    EntityId = int.Parse(parts[2]),
                    ValidationAction = UnescapeCsvField(parts[3]),
                    IsSuccess = bool.Parse(parts[4]),
                    ErrorCount = int.Parse(parts[5]),
                    WarningCount = int.Parse(parts[6]),
                    Messages = UnescapeCsvField(parts[7]),
                    ValidatedByUser = UnescapeCsvField(parts[8])
                };
            }
            catch
            {
                return null;
            }
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            return field.Replace("\"", "\"\"");
        }

        private string UnescapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            return field.Trim('"').Replace("\"\"", "\"");
        }
    }

    public class ValidationHistoryRecord
    {
        public DateTime ValidationDate { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public string ValidationAction { get; set; }
        public bool IsSuccess { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public string Messages { get; set; }
        public string ValidatedByUser { get; set; }
    }
}
