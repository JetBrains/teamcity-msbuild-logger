namespace TeamCity.MSBuild.Logger
{
    internal interface IColorStorage
    {
        Color? Color { get;}

        void SetColor(Color color);

        void ResetColor();
    }
}
