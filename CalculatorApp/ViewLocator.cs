using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CalculatorApp.ViewModels;
using System;

namespace CalculatorApp
{
    public class ViewLocator : IDataTemplate
    {
        public Control Build(object data)
        {
            var name = data.GetType().FullName!.Replace("ViewModel", "View");
            var type = Type.GetType(name);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }

            return new TextBlock { Text = $"View for {data.GetType().Name} not found." };
        }

        public bool Match(object data)
        {
            return data is MainWindowViewModel; // Замените на ваш базовый класс ViewModel
        }
    }
}