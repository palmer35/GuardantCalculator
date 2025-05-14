using System.Reflection;
using CalculatorApp.Abstractions;

var modules = new List<ICalculatorModule>();
var pluginDir = Path.Combine(AppContext.BaseDirectory, "Plugins");

if (Directory.Exists(pluginDir))
{
    foreach (var file in Directory.GetFiles(pluginDir, "*.dll"))
    {
        try
        {
            var asm = Assembly.LoadFrom(file);

            var types = asm.GetTypes().Where(t => typeof(ICalculatorModule).IsAssignableFrom(t) && !t.IsInterface);
            foreach (var type in types)
            {
                var instance = (ICalculatorModule?)Activator.CreateInstance(type);
                if (instance != null)
                {
                    modules.Add(instance);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке {file}: {ex.Message}");
        }
    }
}

Console.WriteLine("Модули загружены:");
foreach (var mod in modules)
{
    Console.WriteLine($" - {mod.Name}: 5 и 3 = {mod.Calculate(5, 3)}");
}
