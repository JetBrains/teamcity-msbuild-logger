namespace TeamCity.MSBuild.Logger
{
    using System;

    internal struct HierarchicalKey
    {
        private readonly string _key;

        public HierarchicalKey([NotNull] string key)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is HierarchicalKey && Equals((HierarchicalKey) obj);
        }

        public override int GetHashCode()
        {
            return _key.GetHashCode();
        }

        private bool Equals(HierarchicalKey other)
        {
            return string.Equals(_key, other._key);
        }

        public override string ToString()
        {
            return _key;
        }
    }
}
