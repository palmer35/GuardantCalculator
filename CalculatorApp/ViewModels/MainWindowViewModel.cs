using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using CalculatorApp.Abstractions;
using IniParser;
using IniParser.Model;
using ReactiveUI;

namespace CalculatorApp.ViewModels
{
    // 1) Простая реализация ICommand для асинхронных действий
    public class AsyncCommand : ICommand
    {
        private readonly Func<Task> _execute;
        public AsyncCommand(Func<Task> execute) => _execute = execute;

        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public async void Execute(object? parameter)
        {
            try { await _execute(); }
            catch (Exception ex) { Debug.WriteLine($"[AsyncCommand] {ex}"); }
        }
    }

    // 2) Основная VM
    public class MainWindowViewModel : ReactiveObject
    {
        // выражение и результат
        private string _expression = "";
        public string Expression
        {
            get => _expression;
            set => this.RaiseAndSetIfChanged(ref _expression, value);
        }

        private string _result = "";
        public string Result
        {
            get => _result;
            private set => this.RaiseAndSetIfChanged(ref _result, value);
        }

        // сообщение о статусе загрузки модулей
        private string _statusMessage = "";
        public string StatusMessage
        {
            get => _statusMessage;
            private set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        // список имён загруженных модулей
        public ObservableCollection<string> ModuleNames { get; } = new();

        // команды
        public ICommand CalculateCommand { get; }
        public ICommand ReloadModulesCommand { get; }

        // внутреннее хранилище экземпляров модулей
        private readonly List<ICalculatorModule> _modules = new();

        public MainWindowViewModel()
        {
            // 1) Настраиваем команду перезагрузки
            ReloadModulesCommand = new AsyncCommand(LoadModulesAndReportAsync);

            // 2) Делаем начальную загрузку и сразу устанавливаем статус
            _ = InitialLoadModulesAsync();

            // 3) Команда вычисления
            CalculateCommand = new AsyncCommand(DoCalculationAsync);
        }

        // Асинхронная вычислительная команда
        private async Task DoCalculationAsync()
        {
            await Dispatcher.UIThread.InvokeAsync(Calculate);
        }

        // ——— Начальная загрузка при старте приложения ———
        private async Task InitialLoadModulesAsync()
        {
            _modules.Clear();
            await Dispatcher.UIThread.InvokeAsync(ModuleNames.Clear);

            // Загружаем модули в фоне
            await Task.Run(() => LoadModules());

            // Обновляем UI
            foreach (var m in _modules)
                await Dispatcher.UIThread.InvokeAsync(() => ModuleNames.Add(m.Name));

            // Устанавливаем статус
            StatusMessage = _modules.Count > 0
                ? $"Загружено модулей: {_modules.Count}"
                : "Не найдено ни одного модуля";
        }

        // ——— Перезагрузка по кнопке ———
        private async Task LoadModulesAndReportAsync()
        {
            _modules.Clear();
            await Dispatcher.UIThread.InvokeAsync(ModuleNames.Clear);

            try
            {
                StatusMessage = "Перезагрузка модулей...";
                await Task.Run(() => LoadModules());

                foreach (var m in _modules)
                    await Dispatcher.UIThread.InvokeAsync(() => ModuleNames.Add(m.Name));

                StatusMessage = _modules.Count > 0
                    ? $"Перезагрузка прошла успешно: загружено {_modules.Count} модулей."
                    : "Перезагрузка выполнена, но не найдено ни одного модуля.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoadModulesAndReportAsync] {ex}");
                StatusMessage = $"Перезагрузка не удалась: {ex.Message}";
            }
        }

        // Синхронная логика загрузки модулей
        private void LoadModules()
        {
            var iniPath = Path.Combine(AppContext.BaseDirectory, "config.ini");
            if (!File.Exists(iniPath))
                throw new FileNotFoundException("config.ini не найден", iniPath);

            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(iniPath);
            var section = data["Modules"];

            foreach (var entry in section)
            {
                var rel = entry.Value;
                var full = Path.Combine(AppContext.BaseDirectory, rel);
                if (!File.Exists(full))
                {
                    Debug.WriteLine($"[LoadModules] DLL не найдена: {full}");
                    continue;
                }

                var asm = Assembly.LoadFrom(full);
                var type = asm.GetTypes()
                              .FirstOrDefault(t =>
                                  typeof(ICalculatorModule).IsAssignableFrom(t)
                                  && !t.IsInterface && !t.IsAbstract);
                if (type == null)
                {
                    Debug.WriteLine($"[LoadModules] ICalculatorModule не найден в {full}");
                    continue;
                }

                var inst = (ICalculatorModule)Activator.CreateInstance(type)!;
                _modules.Add(inst);
                Debug.WriteLine($"[LoadModules] Загрузили модуль {inst.Name}");
            }
        }

        // Вычисление через DataTable
        private void Calculate()
        {
            try
            {
                var dt = new DataTable();
                var value = dt.Compute(Expression, "");
                PostResult(Convert.ToDouble(value).ToString("F2"));
            }
            catch (Exception ex)
            {
                PostResult("Ошибка: " + ex.Message);
            }
        }

        private void PostResult(string text)
        {
            Dispatcher.UIThread.Post(() => Result = text);
        }
    }
}
