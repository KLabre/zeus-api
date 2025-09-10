using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zeus.Api.Infrastructure
{
    public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateOnly.FromDateTime(reader.GetDateTime());
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            var isoDate = value.ToString("O");
            writer.WriteStringValue(isoDate);
        }
    }

    public class DateTimeConverter : JsonConverter<DateTime>
    {
        private const string DateTimeFormat = "dd/MM/yyyy hh:mm:ss tt";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType != JsonTokenType.String)
                throw new NotImplementedException();

            var dateTimeString = reader.GetString();

            if(!DateTime.TryParseExact(dateTimeString, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime dateTime))
                throw new JsonException();

            return dateTime;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToUniversalTime().ToString(DateTimeFormat, CultureInfo.InvariantCulture));
        }
    }
}
