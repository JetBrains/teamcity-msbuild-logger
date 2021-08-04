namespace TeamCity.MSBuild.Logger
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ColorStorage : IColorStorage
    {
        public Color? Color { get; private set; }

        public void SetColor(Color color) => Color = color;

        public void ResetColor() => Color = default;
    }
}