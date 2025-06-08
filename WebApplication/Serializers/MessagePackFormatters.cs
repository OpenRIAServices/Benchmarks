using MessagePack;
using MessagePack.Formatters;
using OpenRiaServices.Client.Benchmarks.Server.Cities;

#pragma warning disable MsgPack009 // Colliding formatters

namespace WebApp.Serializers
{
    class CustomResolver(IMessagePackFormatter<City> formatter) : IFormatterResolver
    {

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            if (typeof(City) == typeof(T))
                return (IMessagePackFormatter<T>)formatter;
            return null;
        }
    }

    class CityFormatter : MessagePack.Formatters.IMessagePackFormatter<City>
    {
        public City Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            throw new NotImplementedException();

        }

        public void Serialize(ref MessagePackWriter writer, City value, MessagePackSerializerOptions options)
        {
            writer.WriteMapHeader(6);
            writer.Write("CalculatedCounty"u8); writer.Write(value.CalculatedCounty);
            //writer.Write("County"u8); writer.WriteString(value.County);
            writer.Write("CountyName"u8); writer.Write(value.CountyName);
            writer.Write("Name"u8); writer.Write(value.Name);
            writer.Write("StateName"u8); writer.Write(value.StateName);
            //writer.Write("ZipCodes"u8); writer.Write(value.ZipCodes);
            writer.Write("ZoneID"u8); writer.Write(value.ZoneID);
            writer.Write("ZoneName"u8); writer.Write(value.ZoneName);
        }
    }

    class CityFlatFormatter : MessagePack.Formatters.IMessagePackFormatter<City>
    {
        public City Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            throw new NotImplementedException();

        }

        public void Serialize(ref MessagePackWriter writer, City value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(6);
            //writer.Write("CalculatedCounty"u8);
            writer.Write(value.CalculatedCounty);
            //writer.Write("County"u8); writer.WriteString(value.County);
            //writer.Write("CountyName"u8); 
            writer.Write(value.CountyName);
            //writer.Write("Name"u8); 
            writer.Write(value.Name);
            //writer.Write("StateName"u8);
            writer.Write(value.StateName);
            //writer.Write("ZipCodes"u8); writer.Write(value.ZipCodes);
            //writer.Write("ZoneID"u8); 
            writer.Write(value.ZoneID);
            //writer.Write("ZoneName"u8); 
            writer.Write(value.ZoneName);
        }
    }
}

