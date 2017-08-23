namespace TeamCity.MSBuild.Logger
{
    using System.Collections.Generic;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ColorStorage : IColorStorage
    {
        private readonly Stack<Color> _colors = new Stack<Color>();

        public Color? Color => _colors.Count > 0 ? _colors.Peek() : default(Color?);

        public void SetColor(Color color)
        {
            _colors.Push(color);
        }

        public void ResetColor()
        {
            if (_colors.Count > 0)
            {
                _colors.Pop();
            }
        }
    }
}