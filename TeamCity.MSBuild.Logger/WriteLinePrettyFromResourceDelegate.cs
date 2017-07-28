namespace TeamCity.MSBuild.Logger
{
    internal delegate void WriteLinePrettyFromResourceDelegate(int indentLevel, string resourceString, params object[] args);
}
