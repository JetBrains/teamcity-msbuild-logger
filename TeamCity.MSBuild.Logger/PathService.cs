namespace TeamCity.MSBuild.Logger
{
    using System;
    using System.IO;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class PathService: IPathService
    {
        public string GetFileName(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            return Path.GetFileName(path);
        }
    }
}
