using Microsoft.Extensions.DependencyInjection;

namespace WarehouseSystem.Application.Validators
{
    public static class BusinessRuleValidatorRegistration
    {
        public static IServiceCollection AddBusinessRuleValidators(this IServiceCollection services)
        {
            // Registruojame visus verslo taisyklių validatorius
            services.AddScoped<ProductBusinessRuleValidator>();
            services.AddScoped<QRCodeBusinessRuleValidator>();
            services.AddScoped<WarehouseBusinessRuleValidator>();
            services.AddScoped<MovementBusinessRuleValidator>();

            // Registruojame validacijos rezultatų servisą
            services.AddScoped<IValidationResultService, ValidationResultService>();

            return services;
        }
    }

    public interface IValidationResultService
    {
        ValidationResult CombineResults(params ValidationResult[] results);
        bool HasBlockingErrors(ValidationResult result);
        string FormatValidationMessages(ValidationResult result, bool includeWarnings = true);
    }

    public class ValidationResultService : IValidationResultService
    {
        public ValidationResult CombineResults(params ValidationResult[] results)
        {
            var combinedResult = new ValidationResult();

            foreach (var result in results)
            {
                if (result != null)
                {
                    foreach (var error in result.Errors)
                    {
                        combinedResult.AddError(error.Message);
                    }

                    foreach (var warning in result.Warnings)
                    {
                        combinedResult.AddWarning(warning.Message);
                    }
                }
            }

            return combinedResult;
        }

        public bool HasBlockingErrors(ValidationResult result)
        {
            if (result == null) return false;
            return result.Errors.Any();
        }

        public string FormatValidationMessages(ValidationResult result, bool includeWarnings = true)
        {
            if (result == null) return "Validation result is null";

            var messages = new List<string>();

            // Pridedame klaidas
            if (result.Errors.Any())
            {
                messages.Add("Klaidos:");
                messages.AddRange(result.Errors.Select(e => $"- {e.Message}"));
            }

            // Pridedame įspėjimus, jei reikia
            if (includeWarnings && result.Warnings.Any())
            {
                if (messages.Any()) messages.Add(string.Empty); // Tuščia eilutė tarp klaidų ir įspėjimų
                messages.Add("Įspėjimai:");
                messages.AddRange(result.Warnings.Select(w => $"- {w.Message}"));
            }

            return messages.Any() 
                ? string.Join(Environment.NewLine, messages) 
                : "Validacija sėkminga";
        }
    }
}
