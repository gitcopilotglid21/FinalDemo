using FluentValidation;

namespace QuickBite.API.Validators
{
    /// <summary>
    /// Validator for pagination parameters following BRD specifications
    /// </summary>
    public class PaginationValidator : AbstractValidator<PaginationParameters>
    {
        public PaginationValidator()
        {
            // Page validation - BRD: Must be positive
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0");

            // Limit validation - BRD: 1-100 items per page, default 20
            RuleFor(x => x.Limit)
                .GreaterThan(0)
                .WithMessage("Limit must be greater than 0")
                .LessThanOrEqualTo(100)
                .WithMessage("Limit cannot exceed 100 items per page");
        }
    }

    /// <summary>
    /// Parameters for pagination
    /// </summary>
    public class PaginationParameters
    {
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 20;
    }

    /// <summary>
    /// Validator for menu item query parameters
    /// </summary>
    public class MenuItemQueryValidator : AbstractValidator<MenuItemQueryParameters>
    {
        // Predefined categories as specified in BRD
        private readonly string[] _validCategories =
        {
            "Appetizers", "Salads", "Soups", "Main Course", "Desserts", "Beverages"
        };

        // Predefined dietary tags as specified in BRD
        private readonly string[] _validDietaryTags =
        {
            "Vegetarian", "Vegan", "Gluten-Free", "Dairy-Free", "Nut-Free", "Spicy", "Low-Carb"
        };

        public MenuItemQueryValidator()
        {
            // Page validation
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0");

            // Limit validation
            RuleFor(x => x.Limit)
                .GreaterThan(0)
                .WithMessage("Limit must be greater than 0")
                .LessThanOrEqualTo(100)
                .WithMessage("Limit cannot exceed 100 items per page");

            // Category validation - Optional, but if provided must be valid
            When(x => !string.IsNullOrEmpty(x.Category), () =>
            {
                RuleFor(x => x.Category)
                    .Must(category => _validCategories.Contains(category!))
                    .WithMessage($"Category must be one of: {string.Join(", ", _validCategories)}");
            });

            // Dietary tags validation - Optional, but if provided must be valid
            When(x => !string.IsNullOrEmpty(x.DietaryTags), () =>
            {
                RuleFor(x => x.DietaryTags)
                    .Must(dietaryTagsString =>
                    {
                        if (string.IsNullOrEmpty(dietaryTagsString)) return true;

                        var tags = dietaryTagsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(t => t.Trim())
                                                   .ToArray();

                        return tags.All(tag => _validDietaryTags.Contains(tag));
                    })
                    .WithMessage($"All dietary tags must be from: {string.Join(", ", _validDietaryTags)}");
            });

            // Search validation - Optional, but if provided should have reasonable length
            When(x => !string.IsNullOrEmpty(x.Search), () =>
            {
                RuleFor(x => x.Search)
                    .Length(1, 100)
                    .WithMessage("Search term must be between 1 and 100 characters");
            });
        }
    }

    /// <summary>
    /// Parameters for menu item queries
    /// </summary>
    public class MenuItemQueryParameters
    {
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 20;
        public string? Category { get; set; }
        public string? DietaryTags { get; set; }
        public string? Search { get; set; }
    }
}