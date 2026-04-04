using System;

namespace Mealplan;

class Program
{
    static void Main(string[] args)
    {
        DataManager dataManager = new DataManager();

        ConsoleUI ui = new ConsoleUI(dataManager);

        ui.WelcomeScreen();
    }
}