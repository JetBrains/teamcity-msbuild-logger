namespace TeamCity.MSBuild.Logger
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ColorStorage : IColorStorage
    {
        private Color? _color = default(Color?);

        public Color? Color => _color;

        public void SetColor(Color color)
        {
            _color = color;
        }

        public void ResetColor()
        {
            _color = default(Color?);
        }
    }
}