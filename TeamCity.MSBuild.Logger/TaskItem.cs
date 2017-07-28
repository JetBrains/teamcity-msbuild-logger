namespace TeamCity.MSBuild.Logger
{
    using System;
    using Microsoft.Build.Framework;

    internal struct TaskItem
    {
        public readonly string Name;

        public readonly ITaskItem Item;

        public TaskItem([NotNull] string name, [NotNull] ITaskItem item)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Item = item ?? throw new ArgumentNullException(nameof(item));
        }
    }
}
