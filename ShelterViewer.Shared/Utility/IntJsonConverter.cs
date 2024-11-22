using MudBlazor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterViewer.Shared.Utility;

public class IntJsonConverter : JsonConverter
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(int) || typeToConvert == typeof(int?);
    }

    public override object? ReadJson(JsonReader reader, Type typeToConvert, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Integer)
        {
            return Convert.ToInt32(reader.Value);
        }

        return 0;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        writer.WriteValue(value);
    }
}
