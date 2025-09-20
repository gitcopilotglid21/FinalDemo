using FluentValidation;
using QuickBite.API.DTOs;

namespace QuickBite.API.Validators
{
    /// <summary>
    /// Validator for UpdateMenuItemDto following BRD specifications
    /// Validates all business rules for updating menu items
    /// </summary>
    public class UpdateMenuItemDtoValidator : AbstractValidator<UpdateMenuItemDto>
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

        public UpdateMenuItemDtoValidator()
        {

            // Name validation - Optional for updates, but if provided must follow same rules
            When(x => !string.IsNullOrEmpty(x.Name), () =>
            {
                RuleFor(x => x.Name)
                    .Length(1, 100)
                    .WithMessage("Name must be between 1 and 100 characters")
                    .Matches("^[a-zA-Z0-9\\s'-]+$")
                    .WithMessage("Name can only contain letters, numbers, spaces, hyphens, and apostrophes");
            });

            // Description validation - Optional for updates
            When(x => !string.IsNullOrEmpty(x.Description), () =>
            {
                RuleFor(x => x.Description)
                    .Length(1, 500)
                    .WithMessage("Description must be between 1 and 500 characters");
            });

            // Price validation - Optional for updates, but if provided must follow same rules
            When(x => x.Price.HasValue, () =>
            {
                RuleFor(x => x.Price!.Value)
                    .GreaterThan(0)
                    .WithMessage("Price must be greater than 0")
                    .LessThanOrEqualTo(999.99m)
                    .WithMessage("Price cannot exceed $999.99")
                    .Must(price => HasValidDecimalPlaces(price))
                    .WithMessage("Price can have at most 2 decimal places");
            });

            // Category validation - Optional for updates, but if provided must be valid
            When(x => !string.IsNullOrEmpty(x.Category), () =>
            {
                RuleFor(x => x.Category)
                    .Must(category => _validCategories.Contains(category!))
                    .WithMessage($"Category must be one of: {string.Join(", ", _validCategories)}");
            });

            // Dietary tags validation - Optional for updates
            When(x => x.DietaryTags != null, () =>
            {
                RuleFor(x => x.DietaryTags!)
                    .Must(tags => tags.All(tag => _validDietaryTags.Contains(tag)))
                    .WithMessage($"All dietary tags must be from: {string.Join(", ", _validDietaryTags)}")
                    .Must(tags => tags.Count <= 10)
                    .WithMessage("Cannot have more than 10 dietary tags")
                    .Must(tags => tags.Distinct().Count() == tags.Count)
                    .WithMessage("Dietary tags must be unique");
            });

            // Custom validation to ensure at least one field is being updated
            RuleFor(x => x)
                .Must(dto => !string.IsNullOrEmpty(dto.Name) || 
                           !string.IsNullOrEmpty(dto.Description) || 
                           dto.Price.HasValue || 
                           !string.IsNullOrEmpty(dto.Category) || 
                           dto.DietaryTags != null)
                .WithMessage("At least one field must be provided for update");
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