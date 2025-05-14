using CalculatorApp.Abstractions;

public class SubtractOperation : ICalculatorModule
{
    public string Name => "Subtract";

    public double Calculate(double a, double b) => a - b;
}
