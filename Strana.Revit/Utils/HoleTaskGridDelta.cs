namespace Strana.Revit.HoleTask.Utils
{
    public class HoleTaskGridDelta(double deltaGridNumber, double deltaGridSymbol, double deltaGridMax)
    {
        public double DeltaGridNumber { get; } = deltaGridNumber;
        public double deltaGridLetter {  get; } = deltaGridSymbol;
        public double deltaGridMax {  get; } = deltaGridSymbol;

    }

    public class CopyOfHoleTaskGridDelta(double deltaGridNumber, double deltaGridSymbol, double deltaGridMax)
    {
        public double DeltaGridNumber { get; } = deltaGridNumber;
        public double deltaGridLetter { get; } = deltaGridSymbol;
        public double deltaGridMax { get; } = deltaGridSymbol;

    }
}

