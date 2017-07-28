namespace TeamCity.MSBuild.Logger
{
    internal struct Property
    {
        public readonly string Name;

        public readonly string Value;

        public Property(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
