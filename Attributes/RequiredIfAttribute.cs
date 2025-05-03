using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

public class RequiredIfAttribute : ValidationAttribute
{
    public string PropertyName { get; set; }
    public object ExpectedValue { get; set; }

    public RequiredIfAttribute(string propertyName, object expectedValue)
    {
        PropertyName = propertyName;
        ExpectedValue = expectedValue;
    }

    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        var instance = context.ObjectInstance;
        var property = instance.GetType().GetProperty(PropertyName);

        if (property == null)
            return new ValidationResult($"Property {PropertyName} not found.");

        var propertyValue = property.GetValue(instance);

        if (Equals(propertyValue, ExpectedValue) && value == null)
            return new ValidationResult(ErrorMessage);

        return ValidationResult.Success;
    }
}