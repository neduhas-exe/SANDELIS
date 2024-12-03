using System.Collections.Generic;
using System.Linq;

namespace WarehouseSystem.Application.Validators
{
    public class ValidationResult
    {
        private readonly List<ValidationMessage> _messages;

        public ValidationResult()
        {
            _messages = new List<ValidationMessage>();
        }

        public bool IsValid => !_messages.Any(m => m.Type == ValidationMessageType.Error);
        public bool HasWarnings => _messages.Any(m => m.Type == ValidationMessageType.Warning);
        
        public IReadOnlyList<ValidationMessage> Messages => _messages.AsReadOnly();
        public IEnumerable<ValidationMessage> Errors => _messages.Where(m => m.Type == ValidationMessageType.Error);
        public IEnumerable<ValidationMessage> Warnings => _messages.Where(m => m.Type == ValidationMessageType.Warning);

        public void AddError(string message)
        {
            _messages.Add(new ValidationMessage(message, ValidationMessageType.Error));
        }

        public void AddWarning(string message)
        {
            _messages.Add(new ValidationMessage(message, ValidationMessageType.Warning));
        }

        public void AddInfo(string message)
        {
            _messages.Add(new ValidationMessage(message, ValidationMessageType.Info));
        }

        public void MergeWith(ValidationResult other)
        {
            if (other == null) return;
            _messages.AddRange(other.Messages);
        }

        public override string ToString()
        {
            if (!_messages.Any()) return "Validation passed successfully";

            var result = new List<string>();
            
            if (Errors.Any())
            {
                result.Add("Errors:");
                result.AddRange(Errors.Select(e => $"- {e.Message}"));
            }

            if (Warnings.Any())
            {
                result.Add("Warnings:");
                result.AddRange(Warnings.Select(w => $"- {w.Message}"));
            }

            var info = _messages.Where(m => m.Type == ValidationMessageType.Info);
            if (info.Any())
            {
                result.Add("Info:");
                result.AddRange(info.Select(i => $"- {i.Message}"));
            }

            return string.Join("\n", result);
        }
    }

    public class ValidationMessage
    {
        public string Message { get; }
        public ValidationMessageType Type { get; }

        public ValidationMessage(string message, ValidationMessageType type)
        {
            Message = message;
            Type = type;
        }
    }

    public enum ValidationMessageType
    {
        Info,
        Warning,
        Error
    }
}
