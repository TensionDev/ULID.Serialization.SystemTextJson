using Moq;
using System;
using System.Buffers;
using System.Text;
using System.Text.Json;
using Xunit;

namespace TensionDev.ULID.Serialization.SystemTextJson.Tests
{
    public class UlidSystemTextJsonConverterTests : IDisposable
    {
        private bool disposedValue;

        private readonly UlidSystemTextJsonConverter _converter;

        public UlidSystemTextJsonConverterTests()
        {
            _converter = new UlidSystemTextJsonConverter();
        }

        [Theory]
        [InlineData(true, "00000000000000000000000000")]
        [InlineData(true, "7ZZZZZZZZZZZZZZZZZZZZZZZZZ")]
        [InlineData(true, "01ARZ3NDEKTSV4RRFFQ69G5FAV")]
        [InlineData(false, "00000000000000000000000000")]
        [InlineData(false, "7ZZZZZZZZZZZZZZZZZZZZZZZZZ")]
        [InlineData(false, "01ARZ3NDEKTSV4RRFFQ69G5FAV")]
        public void TestWrite(bool useNullOptions, string input)
        {
            // Arrange
            using var ms = new MemoryStream();
            using var writer = new Utf8JsonWriter(ms);
            Ulid value = Ulid.Parse(input);
            JsonSerializerOptions? options = useNullOptions ? null : new JsonSerializerOptions();

            // Act
            _converter.Write(writer, value, options);
            writer.Flush();
            string actual = Encoding.UTF8.GetString(ms.ToArray());

            // Assert
            string expected = JsonSerializer.Serialize(value.ToString());
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestReadNotString()
        {
            // Arrange
            // Use a canonical all-zero ULID representation which is commonly accepted by ULID parsers.
            const string input = "00000000000000000000000000";
            string jsonText = "0";
            byte[] json = Encoding.UTF8.GetBytes(jsonText);
            var reader = new Utf8JsonReader(json);
            reader.Read();

            // Act
            JsonException ex = null;
            try
            {
                _converter.Read(ref reader, typeof(Ulid), new JsonSerializerOptions());
            }
            catch (JsonException caught)
            {
                ex = caught;
            }

            // Assert
            Assert.NotNull(ex);
        }

        [Fact]
        public void TestReadEmptyString()
        {
            // Arrange
            // Use a canonical all-zero ULID representation which is commonly accepted by ULID parsers.
            const string input = "00000000000000000000000000";
            string jsonText = "\"\"";
            byte[] json = Encoding.UTF8.GetBytes(jsonText);
            var reader = new Utf8JsonReader(json);
            reader.Read();

            // Act
            JsonException ex = null;
            try
            {
                _converter.Read(ref reader, typeof(Ulid), new JsonSerializerOptions());
            }
            catch (JsonException caught)
            {
                ex = caught;
            }

            // Assert
            Assert.NotNull(ex);
        }

        [Theory]
        [InlineData("00000000000000000000000000")]
        [InlineData("7ZZZZZZZZZZZZZZZZZZZZZZZZZ")]
        [InlineData("01ARZ3NDEKTSV4RRFFQ69G5FAV")]
        public void TestReadString(string input)
        {
            // Arrange
            string jsonText = "\"" + input + "\"";
            byte[] json = Encoding.UTF8.GetBytes(jsonText);
            var reader = new Utf8JsonReader(json);
            reader.Read();

            // Act
            Ulid result = _converter.Read(ref reader, typeof(Ulid), new JsonSerializerOptions());

            // Assert
            // Compare textual forms to avoid depending on reference equality or unknown equality semantics of Ulid.
            Assert.Equal(input, result.ToString());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~UlidSystemTextJsonConverterTests()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}