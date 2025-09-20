using FluentValidation;
using QuickBite.API.DTOs;

namespace QuickBite.API.Validators
{
    /// <summary>
    /// Validator for CreateMenuItemDto following BRD specifications
    /// Validates all business rules for creating menu items
    /// Note: Uniqueness validation is handled in the controller for async compatibility
    /// </summary>
    public class CreateMenuItemDtoValidator : AbstractValidator<CreateMenuItemDto>
    {
        // Predefined categories as specified in BRD
        private readonly string[] _validCategories =
        {
            "Appetizers", "Salads", "Soups", "Main Course", "Desserts", "Beverages"
        };

        // Predefined dietary tags as specified in BRD
        private readonly string[] _validDietaryTags =
        {
            "Vegetarian", "Vegan", "Gluten-Free", "Dairy-Free", "Nut-Free", "Spicy", "Low-Carb", "Halal", "Kosher", "Jhatka", "Non-Vegetarian"
        };

        public CreateMenuItemDtoValidator()
        {

            // Name validation - BRD: Required, 1-100 characters
            // Note: Uniqueness validation moved to controller for async compatibility
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required")
                .Length(1, 100)
                .WithMessage("Name must be between 1 and 100 characters")
                .Matches("^[a-zA-Z0-9\\s'-]+$")
                .WithMessage("Name can only contain letters, numbers, spaces, hyphens, and apostrophes");

            // Description validation - BRD: Required, 1-500 characters
            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description is required")
                .Length(1, 500)
                .WithMessage("Description must be between 1 and 500 characters");

            // Price validation - BRD: Required, positive value, $0.01-$999.99, max 2 decimal places
            RuleFor(x => x.Price)
                .NotEmpty()
                .WithMessage("Price is required")
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0")
                .LessThanOrEqualTo(999.99m)
                .WithMessage("Price cannot exceed $999.99")
                .Must(price => HasValidDecimalPlaces(price))
                .WithMessage("Price can have at most 2 decimal places");

            // Category validation - BRD: Required, must be from predefined list
            RuleFor(x => x.Category)
                .NotEmpty()
                .WithMessage("Category is required")
                .Must(category => _validCategories.Contains(category))
                .WithMessage($"Category must be one of: {string.Join(", ", _validCategories)}");

            // Dietary tags validation - BRD: Optional, each tag must be from predefined list
            RuleFor(x => x.DietaryTags)
                .Must(tags => tags == null || tags.All(tag => _validDietaryTags.Contains(tag)))
                .WithMessage($"All dietary tags must be from: {string.Join(", ", _validDietaryTags)}")
                .Must(tags => tags == null || tags.Count <= 10)
                .WithMessage("Cannot have more than 10 dietary tags")
                .Must(tags => tags == null || tags.Distinct().Count() == tags.Count)
                .WithMessage("Dietary tags must be unique");
        }

        /// <summary>
        /// Validates that price has at most 2 decimal places
        /// </summary>
        private static bool HasValidDecimalPlaces(decimal price)
        {
            var decimalPlaces = BitConverter.GetBytes(decimal.GetBits(price)[3])[2];
            return decimalPlaces <= 2;
        }
    }
}