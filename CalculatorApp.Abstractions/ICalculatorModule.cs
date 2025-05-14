namespace CalculatorApp.Abstractions
{
    public interface ICalculatorModule
    {
        string Name { get; }
        double Calculate(double a, double b);
    }
}
