using CalculatorApp.Abstractions;

namespace AddModule
{
    public class AddOperation : ICalculatorModule
    {
        public string Name => "Add";

        public double Calculate(double a, double b) => a + b;
    }
}
