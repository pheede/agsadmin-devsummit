// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using Newtonsoft.Json;

namespace ArcGISRESTAdmin.Classes
{
    internal class UnixDateTimeConverter : Newtonsoft.Json.Converters.DateTimeConverterBase
    {

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            long value = Convert.ToInt64(reader.Value);

            return EncodingHelper.DateTimeFromUnixTimestampMillis(value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, EncodingHelper.GetUnixTimestampMillis((DateTime)value));
        }
    }
}
