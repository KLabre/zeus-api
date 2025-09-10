using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Zeus.Api.Infrastructure
{
    /// <summary>
    /// Provides condition validation based on related property value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RequiredIfAttribute : ValidationAttribute
    {
        #region Properties
        public string OtherProperty { get; private set; }
        public string? OtherPropertyDisplayName { get; set; }
        public object OtherPropertyValue { get; private set; }
        public bool IsInverted { get; set; }

        public override bool RequiresValidationContext
        {
            get { return true; }
        }
        #endregion

        #region Constructor
        public RequiredIfAttribute(string otherProperty, object otherPropertyValue)
            : base(errorMessage: "'{0}' is required because '{1} has a value {3}'{2}'.")
        {
            OtherProperty = otherProperty;
            OtherPropertyValue = otherPropertyValue;
            IsInverted = false;
        }
        #endregion

        public override string FormatErrorMessage(string name)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                ErrorMessageString,
                name,
                OtherPropertyDisplayName ?? OtherProperty,
                OtherPropertyValue,
                IsInverted ? "ohter than " : "of ");
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if(validationContext == null)
                throw new ArgumentNullException(nameof(validationContext));

            var otherProperty = validationContext.ObjectType.GetProperty(OtherProperty);
            if (otherProperty == null)
                return new ValidationResult(
                    string.Format(CultureInfo.CurrentCulture, "Could not find a property named '{0}'", OtherProperty));

            var otherValue = otherProperty.GetValue(validationContext.ObjectInstance);
            
            // Check if this value is actually required and validate it
            if(!IsInverted && Equals(otherValue, OtherPropertyValue) || 
                IsInverted && !Equals(otherValue, OtherPropertyValue))
            {
                if (value == null)
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));

                // assitional check for string so they're not empty
                var val = value as string;
                if (val != null && val.Trim().Length == 0)
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }

            return ValidationResult.Success;
        }
    }
}
