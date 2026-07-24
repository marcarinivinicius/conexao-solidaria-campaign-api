using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConexaoSolidaria.CampaignApi.Api.Configuration;

// Aceita ISO 8601 ("2026-07-24") e o formato brasileiro comum ("24/07/2026"),
// ja que usuarios preenchendo o Swagger na mao tendem a digitar dd/MM/yyyy.
public class FlexibleDateTimeConverter : JsonConverter<DateTime>
{
    private static readonly string[] FormatosAceitos = ["dd/MM/yyyy", "dd/MM/yyyy HH:mm:ss"];

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var valor = reader.GetString();

        if (DateTime.TryParse(valor, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var isoData))
        {
            return isoData;
        }

        if (DateTime.TryParseExact(valor, FormatosAceitos, CultureInfo.InvariantCulture, DateTimeStyles.None, out var brData))
        {
            return brData;
        }

        throw new JsonException($"Data invalida: '{valor}'. Use ISO 8601 (yyyy-MM-dd) ou dd/MM/yyyy.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
}
