using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mealplan;

public class DataManager
{
    public List<Meal> Recipes { get; set; } = new List<Meal>();
    public List<Ingredient> Inventory { get; set; } = new List<Ingredient>();
    public List<TrackedMeal> History { get; set; } = new List<TrackedMeal>();

    private string inventoryPath = "inventory.csv";
    private string recipesPath = "recipes.csv";
    private string trackerPath = "tracker.csv";

    public void LoadRecipes()
    {
        if (!File.Exists(recipesPath)) return;
        Recipes.Clear();
        var lines = File.ReadAllLines(recipesPath);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            if (parts.Length >= 2) Recipes.Add(new Meal { Name = parts[1].Trim(' ', '"') });
        }
    }

    public void LoadInventory()
    {
        if (!File.Exists(inventoryPath)) return;
        Inventory.Clear();
        var lines = File.ReadAllLines(inventoryPath);
        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(',');
            if (parts.Length >= 2) Inventory.Add(new Ingredient { Name = parts[0].Trim(), Quantity = parts[1].Trim() });
        }
    }

    public void LoadTracker()
    {
        if (!File.Exists(trackerPath)) return;
        History.Clear();
        var lines = File.ReadAllLines(trackerPath);
        foreach (var line in lines)
        {
            var parts = line.Split(',');
            if (parts.Length >= 2 && DateTime.TryParse(parts[0], out DateTime date))
                History.Add(new TrackedMeal { DateEaten = date, MealName = parts[1] });
        }
    }

    public void SaveInventory()
    {
        var lines = new List<string> { "Name,Quantity" };
        lines.AddRange(Inventory.Select(i => $"{i.Name},{i.Quantity}"));
        File.WriteAllLines(inventoryPath, lines);
    }

    public void AddToTracker(Meal meal)
    {
        string line = $"{DateTime.Now.ToShortDateString()},{meal.Name}";
        File.AppendAllLines(trackerPath, new[] { line });
    }

    public void DeductFromInventory(string mealName)
    {
        var item = Inventory.FirstOrDefault(i => mealName.Contains(i.Name, StringComparison.OrdinalIgnoreCase));
        if (item != null && int.TryParse(item.Quantity, out int qty) && qty > 0)
        {
            item.Quantity = (qty - 1).ToString();
            SaveInventory();
        }
    }
}