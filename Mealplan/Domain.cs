using System;

namespace Mealplan;

public class Meal
{
    public string Name { get; set; } = string.Empty;
}

public class TrackedMeal
{
    public string MealName { get; set; } = string.Empty;
    public DateTime DateEaten { get; set; }
}

public class Ingredient
{
    public string Name { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
}