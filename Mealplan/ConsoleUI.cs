using Spectre.Console;
using System;
using System.Linq;

namespace Mealplan;

public class ConsoleUI
{
    
    private readonly DataManager dataManager;

    public ConsoleUI(DataManager dataManager)
    {
        this.dataManager = dataManager;
    }

    public void WelcomeScreen()
    {
  

    var startChoice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[green]Welcome to Meal Planner:[/]")
            .AddChoices(new[] { "Start", "Exit" }));

    if (startChoice == "Exit")
    {
        AnsiConsole.MarkupLine("[red]Goodbye![/]");
        return; 
    }

    while (true)
    {
        AnsiConsole.Clear();
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What would you like to do?")
                .PageSize(10) 
                .AddChoices(new[] { "Meal Planner", "Tracker", "Inventory", "Store", "History", "Exit" }));

        if (choice == "Exit") break;

        switch (choice)
        {
            case "Meal Planner": RunPlanner(); break;
            case "Tracker": RunTracker(); break;
            case "Inventory": RunInventoryMenu(); break;
            case "Store": RunStore(); break;
            case "History": ShowHistory(); break;
        }
    }
}

    private void RunPlanner()
    {
        dataManager.LoadRecipes();
        dataManager.LoadInventory();

        var available = dataManager.Recipes.Where(r => 
            dataManager.Inventory.Any(i => r.Name.Contains(i.Name, StringComparison.OrdinalIgnoreCase))).ToList();

        if (!available.Any()) {
            AnsiConsole.MarkupLine("[red]No meals available based on inventory.[/]");
            Console.ReadKey();
            return;
        }

        var meal = AnsiConsole.Prompt(new SelectionPrompt<Meal>().Title("Available Meals:").UseConverter(m => m.Name).AddChoices(available));
        AnsiConsole.MarkupLine($"[green]Planned:[/] {meal.Name}");
        Console.ReadKey();

        AnsiConsole.MarkupLine("\n[yellow]Task complete. Press any key to return to the main menu...[/]");
        Console.ReadKey(true);
    }

    private void RunTracker()
    {
        dataManager.LoadRecipes();
        dataManager.LoadInventory();

        var available = dataManager.Recipes.Where(r => 
            dataManager.Inventory.Any(i => r.Name.Contains(i.Name, StringComparison.OrdinalIgnoreCase))).ToList();

        var meal = AnsiConsole.Prompt(new SelectionPrompt<Meal>().Title("What did you eat?").UseConverter(m => m.Name).AddChoices(available));

        dataManager.AddToTracker(meal);
        dataManager.DeductFromInventory(meal.Name);

        AnsiConsole.MarkupLine($"[green]Logged {meal.Name} and updated inventory![/]");
        Console.ReadKey();

        AnsiConsole.MarkupLine("\n[yellow]Task complete. Press any key to return to the main menu...[/]");
        Console.ReadKey(true);
    }

    private void RunInventoryMenu()
    {
        while (true)
        {
            AnsiConsole.Clear();
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Inventory Management[/]")
                    .AddChoices(new[] { "View Stock", "Add Item", "Remove Item", "Back" }));

            if (choice == "Back") break;

            switch (choice)
            {
                case "View Stock":
                    dataManager.LoadInventory();
                    var table = new Table().AddColumns("Ingredient", "Quantity");
                    foreach (var item in dataManager.Inventory)
                        table.AddRow(Markup.Escape(item.Name), item.Quantity);
                    AnsiConsole.Write(table);
                    AnsiConsole.MarkupLine("\n[grey]Press any key to return...[/]");
                    Console.ReadKey();
                    break;

                case "Add Item":
                    string name = AnsiConsole.Ask<string>("Item Name:");
                    int qtyToAdd = AnsiConsole.Ask<int>("Quantity:");
                    dataManager.LoadInventory();
                    var existing = dataManager.Inventory.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (existing != null) {
                        int.TryParse(existing.Quantity, out int current);
                        existing.Quantity = (current + qtyToAdd).ToString();
                    } else {
                        dataManager.Inventory.Add(new Ingredient { Name = name, Quantity = qtyToAdd.ToString() });
                    }
                    dataManager.SaveInventory();
                    AnsiConsole.MarkupLine("[green]Inventory Updated![/]");
                    System.Threading.Thread.Sleep(1000);
                    break;

                case "Remove Item":
                    dataManager.LoadInventory();
                    var toRemove = AnsiConsole.Prompt(new SelectionPrompt<Ingredient>().Title("Delete which item?").UseConverter(i => $"{i.Name} ({i.Quantity})").AddChoices(dataManager.Inventory));
                    dataManager.Inventory.Remove(toRemove);
                    dataManager.SaveInventory();
                    AnsiConsole.MarkupLine("[red]Item Removed.[/]");
                    System.Threading.Thread.Sleep(1000);
                    break;
            }
        }
    }

    private void RunStore()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[yellow]Store: Add to Inventory[/]").RuleStyle("grey"));

        dataManager.LoadInventory();
        var inventory = dataManager.Inventory;

        string itemName = AnsiConsole.Ask<string>("What item did you [green]buy[/]?");

        var itemToUpdate = inventory.FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        
        if (itemToUpdate == null)
        {
            itemToUpdate = new Ingredient { Name = itemName, Quantity = "0" };
            inventory.Add(itemToUpdate);
        }

        var amountBought = AnsiConsole.Ask<double>($"How many [green]{itemToUpdate.Name}[/] did you buy?");
        var pricePerUnit = AnsiConsole.Ask<double>($"Price per unit for [green]{itemToUpdate.Name}[/]?");
        double totalCost = amountBought * pricePerUnit;

        double currentStock = 0;
        double.TryParse(itemToUpdate.Quantity, out currentStock);
        
        itemToUpdate.Quantity = (currentStock + amountBought).ToString(); 

        dataManager.SaveInventory();

        AnsiConsole.MarkupLine("\n[bold green]SUCCESS![/]");
        AnsiConsole.MarkupLine($"Added [white]{amountBought.ToString()}[/] to [white]{itemToUpdate.Name}[/].");
        AnsiConsole.MarkupLine($"Total Cost: [bold yellow]${totalCost.ToString("F2")}[/]");
        AnsiConsole.MarkupLine($"New Total Stock: [blue]{itemToUpdate.Quantity}[/]");

        AnsiConsole.WriteLine("\nPress any key to return to the main menu...");
        Console.ReadKey(true);
    }

    private void ShowHistory()
    {
        dataManager.LoadTracker();
        var table = new Table().AddColumns("Date", "Meal");
        foreach (var item in dataManager.History)
            table.AddRow(item.DateEaten.ToShortDateString(), Markup.Escape(item.MealName));
        AnsiConsole.Write(table);
        Console.ReadKey();
    }
    
}
