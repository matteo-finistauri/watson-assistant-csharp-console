using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatsonAssistantInterface
{


    public partial class Welcome
    {
        [JsonProperty("output")]
        public Output Output { get; set; }
    }

    public partial class Output
    {
        [JsonProperty("generic")]
        public Generic[] Generic { get; set; }

        [JsonProperty("intents")]
        public Intent[] Intents { get; set; }

        [JsonProperty("entities")]
        public Entity[] Entities { get; set; }

        [JsonProperty("actions")]
        public Action[] Actions { get; set; }

        [JsonProperty("user_defined")]
        public UserDefined UserDefined { get; set; }
    }
    public partial class Action
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("parameters")]
        public Parameters Parameters { get; set; }

        [JsonProperty("result_variable")]
        public string ResultVariable { get; set; }
    }

    public partial class Parameters
    {
        [JsonProperty("chosen_acc")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long ChosenAcc { get; set; }
    }

    public partial class Entity
    {
        [JsonProperty("entity")]
        public string EntityEntity { get; set; }

        [JsonProperty("location")]
        public long[] Location { get; set; }

        [JsonProperty("groups", NullValueHandling = NullValueHandling.Ignore)]
        public Group[] Groups { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("confidence")]
        public long Confidence { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public Metadata Metadata { get; set; }
    }

    public partial class Group
    {
        [JsonProperty("group")]
        public string GroupGroup { get; set; }

        [JsonProperty("location")]
        public long[] Location { get; set; }
    }

    public partial class Metadata
    {
        [JsonProperty("calendar_type", NullValueHandling = NullValueHandling.Ignore)]
        public string CalendarType { get; set; }

        [JsonProperty("timezone", NullValueHandling = NullValueHandling.Ignore)]
        public string Timezone { get; set; }

        [JsonProperty("numeric_value", NullValueHandling = NullValueHandling.Ignore)]
        public long? NumericValue { get; set; }
    }


    public partial class Generic
    {
        [JsonProperty("response_type")]
        public string ResponseType { get; set; }

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("typing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Typing { get; set; }

        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public Option[] Options { get; set; }

        [JsonProperty("preference", NullValueHandling = NullValueHandling.Ignore)]
        public string Preference { get; set; }
    }

    public partial class Option
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("value")]
        public Value Value { get; set; }
    }

    public partial class Value
    {
        [JsonProperty("input")]
        public Input Input { get; set; }
    }

    public partial class Input
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public partial class Intent
    {
        [JsonProperty("intent")]
        public string IntentIntent { get; set; }

        [JsonProperty("confidence")]
        public long Confidence { get; set; }
    }
    public partial class UserDefined
    {
        [JsonProperty("ui_action")]
        public UiAction UiAction { get; set; }
    }

    public partial class UiAction
    {
        [JsonProperty("notification_display")]
        public NotificationDisplay NotificationDisplay { get; set; }
    }

    public partial class NotificationDisplay
    {
        [JsonProperty("DisplayText")]
        public string DisplayText { get; set; }
    }

    public class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}
