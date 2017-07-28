namespace TeamCity.MSBuild.Logger
{
    using System;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class DefaultConsole : IConsole
    {
        public void Write(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Console.Write(text);
            }
        }
    }
}
