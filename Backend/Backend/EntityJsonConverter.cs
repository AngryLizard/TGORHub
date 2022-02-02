using Backend.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Backend
{
    /// <summary>
    /// Define the way all our entity Types are converted to and from json for API output
    /// </summary>
    /// <Typeparam name="EntityType"></Typeparam>
    public class EntityJsonConverter<EntityType> : JsonConverter<EntityType> where EntityType : EntityModel, new()
    {
        /// <summary>
        /// Generate empty Type from entity data.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override EntityType Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var entity = new EntityType();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return entity;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName)
                    {
                        case nameof(entity.Id):
                            entity.Id = reader.GetInt64();
                            break;
                        case nameof(entity.Name):
                            entity.Name = reader.GetString()??"";
                            break;
                        case nameof(entity.Date):
                            entity.Date = reader.GetDateTime();
                            break;
                    }
                }
            }
            throw new JsonException();
        }

        /// <summary>
        /// Write only id, name and date insteadof whole model.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="entity"></param>
        /// <param name="options"></param>
        public override void Write(
            Utf8JsonWriter writer,
            EntityType entity,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber(nameof(entity.Id), entity.Id);
            writer.WriteString(nameof(entity.Name), entity.Name);
            writer.WriteString(nameof(entity.Date), entity.Date.ToString());
            writer.WriteEndObject();
        }

        /// <summary>
        /// All entity Types are allowed
        /// </summary>
        /// <param name="TypeToConvert"></param>
        /// <returns></returns>
        public override bool CanConvert(Type TypeToConvert)
        {
            bool isAssignable = typeof(EntityType).IsAssignableFrom(TypeToConvert);
            return isAssignable;
        }
    }

    /// <summary>
    /// List-version of our entity Type conversion
    /// </summary>
    /// <Typeparam name="EntityType"></Typeparam>
    public class EntityListJsonConverter<EntityType> : JsonConverter<ICollection<EntityType>> where EntityType : EntityModel, new()
    {
        /// <summary>
        /// User our custom entity Type conversion for all list items
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="TypeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override ICollection<EntityType> Read(
            ref Utf8JsonReader reader,
            Type TypeToConvert,
            JsonSerializerOptions options)
        {
            string? json = reader.GetString();
            if (json != null)
            {
                var opt = new JsonSerializerOptions(options);
                opt.Converters.Add(new EntityJsonConverter<EntityType>());
                ICollection<EntityType>? collection = JsonSerializer.Deserialize<ICollection<EntityType>>(json, opt);
                if(collection != null)
                {
                    return collection;
                }
            }
            return new List<EntityType>();
        }

        /// <summary>
        /// User our custom entity Type conversion for all list items
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="entities"></param>
        /// <param name="options"></param>
        public override void Write(
            Utf8JsonWriter writer,
            ICollection<EntityType> entities,
            JsonSerializerOptions options)
        {
            var opt = new JsonSerializerOptions(options);
            opt.Converters.Add(new EntityJsonConverter<EntityType>());
            writer.WriteStringValue(JsonSerializer.Serialize(entities, opt));
        }

        /// <summary>
        /// All entity Type lists are allowed
        /// </summary>
        /// <param name="TypeToConvert"></param>
        /// <returns></returns>
        public override bool CanConvert(Type TypeToConvert)
        {
            bool isAssignable = typeof(ICollection<EntityType>).IsAssignableFrom(TypeToConvert);
            return isAssignable;
        }
    }
}