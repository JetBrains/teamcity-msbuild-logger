namespace TeamCity.MSBuild.Logger.Tests
{
    using Shouldly;
    using Xunit;

    public class ColorStorageTests
    {
        [Fact]
        public void ShouldReturnNullColorByDefault()
        {
            // Given
            var storage = new ColorStorage();

            // When

            // Then
            storage.Color.ShouldBe(default);
        }

        [Fact]
        public void ShouldStoreColor()
        {
            // Given
            var storage = new ColorStorage();

            // When
            storage.SetColor(Color.Error);

            // Then
            storage.Color.ShouldBe(Color.Error);
        }

        [Fact]
        public void ShouldResetColor()
        {
            // Given
            var storage = new ColorStorage();

            // When
            storage.SetColor(Color.Error);
            storage.SetColor(Color.Warning);
            storage.ResetColor();

            // Then
            storage.Color.ShouldBe(default);
        }

        [Fact]
        public void ShouldResetToNullWhenLastReset()
        {
            // Given
            var storage = new ColorStorage();

            // When
            storage.SetColor(Color.Error);
            storage.ResetColor();

            // Then
            storage.Color.ShouldBe(default);
        }

        [Fact]
        public void ShouldResetToNullWhenTooManyReset()
        {
            // Given
            var storage = new ColorStorage();

            // When
            storage.SetColor(Color.Error);
            storage.ResetColor();
            storage.ResetColor();
            storage.ResetColor();
            storage.ResetColor();

            // Then
            storage.Color.ShouldBe(default);
        }
    }
}
