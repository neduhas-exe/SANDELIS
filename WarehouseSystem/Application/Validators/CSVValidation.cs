using System.Text;
using System.Text.RegularExpressions;

namespace WarehouseSystem.Validators
{
    /// <summary>
    /// CSV file validation and data integrity checks
    /// </summary>
    public static class CSVValidation
    {
        public static ValidationResult ValidateCSVStructure(string content, string expectedHeader)
        {
            var result = new ValidationResult();
            
            try
            {
                using var reader = new StringReader(content);
                var header = reader.ReadLine();

                if (string.IsNullOrEmpty(header))
                {
                    result.AddError("CSV file is empty");
                    return result;
                }

                if (header.Trim() != expectedHeader.Trim())
                {
                    result.AddError($"CSV header mismatch. Expected: {expectedHeader}, Got: {header}");
                    return result;
                }

                var lineNumber = 1;
                var expectedColumnCount = expectedHeader.Split(',').Length;

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    
                    // Skip empty lines
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Validate line structure
                    if (!IsValidCSVLine(line, expectedColumnCount))
                    {
                        result.AddError($"Invalid CSV structure at line {lineNumber}");
                        continue;
                    }

                    // Validate data integrity
                    var validation = ValidateCSVLineData(line, lineNumber);
                    if (!validation.IsValid)
                    {
                        result.Errors.AddRange(validation.Errors);
                    }
                }
            }
            catch (Exception ex)
            {
                result.AddError($"CSV validation error: {ex.Message}");
            }

            return result;
        }

        public static bool IsValidCSVLine(string line, int expectedColumnCount)
        {
            var inQuotes = false;
            var columnCount = 1;

            for (var i = 0; i < line.Length; i++)
            {
                if (line[i] == '\"')
                {
                    inQuotes = !inQuotes;
                }
                else if (line[i] == ',' && !inQuotes)
                {
                    columnCount++;
                }
            }

            return columnCount == expectedColumnCount;
        }

        public static ValidationResult ValidateCSVLineData(string line, int lineNumber)
        {
            var result = new ValidationResult();
            var columns = ParseCSVLine(line);

            foreach (var (value, index) in columns.Select((v, i) => (v, i)))
            {
                // Check for proper escaping of quotes
                if (value.Contains("\"") && !Regex.IsMatch(value, "^\".*\"$"))
                {
                    result.AddError($"Improperly escaped quotes in column {index + 1} at line {lineNumber}");
                }

                // Check for invalid characters
                if (value.Any(c => c < 32 || c > 126))
                {
                    result.AddError($"Invalid characters in column {index + 1} at line {lineNumber}");
                }

                // Validate numeric values
                if (ShouldBeNumeric(index) && !string.IsNullOrEmpty(value))
                {
                    if (!decimal.TryParse(value, out _))
                    {
                        result.AddError($"Invalid numeric value in column {index + 1} at line {lineNumber}");
                    }
                }

                // Validate dates
                if (ShouldBeDate(index) && !string.IsNullOrEmpty(value))
                {
                    if (!DateTime.TryParse(value, out _))
                    {
                        result.AddError($"Invalid date format in column {index + 1} at line {lineNumber}");
                    }
                }
            }

            return result;
        }

        private static List<string> ParseCSVLine(string line)
        {
            var columns = new List<string>();
            var currentColumn = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                if (line[i] == '\"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '\"')
                    {
                        // Handle escaped quotes
                        currentColumn.Append('\"');
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

        private static bool ShouldBeNumeric(int columnIndex)
        {
            // Define which column indices should contain numeric values
            return new[] { 6, 7, 8, 9, 10, 11, 12 }.Contains(columnIndex);
        }

        private static bool ShouldBeDate(int columnIndex)
        {
            // Define which column indices should contain date values
            return new[] { 13, 14, 15 }.Contains(columnIndex);
        }

        public static class CSVDataIntegrityChecker
        {
            public static ValidationResult CheckDataIntegrity(string filePath, string backupPath)
            {
                var result = new ValidationResult();

                try
                {
                    // Check if backup exists
                    if (!File.Exists(backupPath))
                    {
                        result.AddError("Backup file not found");
                        return result;
                    }

                    var currentContent = File.ReadAllText(filePath);
                    var backupContent = File.ReadAllText(backupPath);

                    // Compare row counts
                    var currentRows = currentContent.Split('\n').Length;
                    var backupRows = backupContent.Split('\n').Length;

                    if (currentRows < backupRows)
                    {
                        result.AddError($"Possible data loss detected. Current rows: {currentRows}, Backup rows: {backupRows}");
                    }

                    // Check for data corruption
                    if (IsDataCorrupted(currentContent))
                    {
                        result.AddError("Data corruption detected in current file");
                    }

                    // Validate data consistency
                    var consistencyCheck = ValidateDataConsistency(currentContent, backupContent);
                    if (!consistencyCheck.IsValid)
                    {
                        result.Errors.AddRange(consistencyCheck.Errors);
                    }
                }
                catch (Exception ex)
                {
                    result.AddError($"Data integrity check failed: {ex.Message}");
                }

                return result;
            }

            private static bool IsDataCorrupted(string content)
            {
                try
                {
                    var lines = content.Split('\n');
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        
                        // Check for incomplete lines
                        if (line.EndsWith(','))
                            return true;

                        // Check for unbalanced quotes
                        if (line.Count(c => c == '"') % 2 != 0)
                            return true;

                        // Check for invalid UTF-8 characters
                        if (!IsValidUtf8(line))
                            return true;
                    }
                    return false;
                }
                catch
                {
                    return true;
                }
            }

            private static bool IsValidUtf8(string text)
            {
                try
                {
                    var bytes = Encoding.UTF8.GetBytes(text);
                    var decoded = Encoding.UTF8.GetString(bytes);
                    return text == decoded;
                }
                catch
                {
                    return false;
                }
            }

            private static ValidationResult ValidateDataConsistency(string currentContent, string backupContent)
            {
                var result = new ValidationResult();
                var currentLines = currentContent.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
                var backupLines = backupContent.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

                // Compare headers
                if (currentLines[0] != backupLines[0])
                {
                    result.AddError("CSV headers do not match between current and backup files");
                }

                // Check for missing required columns
                var currentHeader = currentLines[0].Split(',');
                var requiredColumns = GetRequiredColumns();
                foreach (var column in requiredColumns)
                {
                    if (!currentHeader.Contains(column))
                    {
                        result.AddError($"Required column '{column}' is missing");
                    }
                }

                return result;
            }

            private static string[] GetRequiredColumns()
            {
                return new[]
                {
                    "Id",
                    "Name",
                    "EANCode",
                    "Category",
                    "UnitOfMeasure",
                    "PurchasePrice",
                    "RetailPrice",
                    "VATRate",
                    "Currency",
                    "CreatedAt"
                };
            }
        }

        public static class CSVBackupManager
        {
            public static async Task CreateBackupAsync(string sourceFilePath, string backupDirectory)
            {
                var fileName = Path.GetFileName(sourceFilePath);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupPath = Path.Combine(backupDirectory, $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}.bak");

                // Ensure backup directory exists
                Directory.CreateDirectory(backupDirectory);

                // Create backup with compression
                await using var sourceStream = File.OpenRead(sourceFilePath);
                await using var backupStream = File.Create(backupPath);
                await using var compress = new System.IO.Compression.GZipStream(backupStream, System.IO.Compression.CompressionMode.Compress);
                await sourceStream.CopyToAsync(compress);
            }

            public static async Task RestoreFromBackupAsync(string backupPath, string targetPath)
            {
                if (!File.Exists(backupPath))
                {
                    throw new FileNotFoundException("Backup file not found", backupPath);
                }

                // Decompress and restore
                await using var backupStream = File.OpenRead(backupPath);
                await using var decompress = new System.IO.Compression.GZipStream(backupStream, System.IO.Compression.CompressionMode.Decompress);
                await using var targetStream = File.Create(targetPath);
                await decompress.CopyToAsync(targetStream);
            }

            public static async Task<DateTime?> GetLastBackupDateAsync(string backupDirectory, string filePattern)
            {
                if (!Directory.Exists(backupDirectory))
                    return null;

                var backupFiles = Directory.GetFiles(backupDirectory, filePattern)
                                         .OrderByDescending(f => File.GetLastWriteTime(f));

                var lastBackup = backupFiles.FirstOrDefault();
                return lastBackup != null ? File.GetLastWriteTime(lastBackup) : null;
            }
        }
    }
}
