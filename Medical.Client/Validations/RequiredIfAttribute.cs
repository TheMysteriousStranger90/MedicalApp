using System.ComponentModel.DataAnnotations;

namespace Medical.Client.Validations;

[AttributeUsage(AttributeTargets.Property)]
public class RequiredIfAttribute : ValidationAttribute
{
    private readonly string _propertyName;
    private readonly object _desiredValue;

    public RequiredIfAttribute(string propertyName, object desiredValue)
    {
        _propertyName = propertyName;
        _desiredValue = desiredValue;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var property = validationContext.ObjectType.GetProperty(_propertyName);
        if (property == null)
        {
            return new ValidationResult($"Unknown property {_propertyName}");
        }

        var propertyValue = property.GetValue(validationContext.ObjectInstance);
        var isRequired = propertyValue?.Equals(_desiredValue) ?? false;

        if (isRequired && (value == null || (value is string str && string.IsNullOrWhiteSpace(str))))
        {
            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.");
        }

        return ValidationResult.Success;
    }
}