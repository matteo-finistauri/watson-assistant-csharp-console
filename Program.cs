using IBM.Cloud.SDK.Core.Authentication.Iam;
using IBM.Cloud.SDK.Core.Http.Exceptions;
using IBM.Watson.Assistant.v2;
using IBM.Watson.Assistant.v2.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace WatsonAssistantInterface
{
    class Program
    {
        static bool verboseLogging = true;

        static void Main(string[] args)
        {
            string version = "2019-02-28";
            string url = "https://gateway-fra.watsonplatform.net/assistant/api";
            string apikey = "0Q5wiiRDtH981JhPBToyqxlrpnrD6XmHirWBXutaBZNC";
            string assistantId = "d1cc83a7-8407-4d7e-9bd2-031e76e6eb84";

            IamConfig config = new IamConfig(apikey: $"{apikey}");
            AssistantService service = new AssistantService($"{version}", config);
            service.SetEndpoint($"{url}");

            try
            {
                var result = service.CreateSession(assistantId: $"{assistantId}");
                Log(result.Response);
                var sessionId = result.Result.SessionId;
                string text = "Hello";
                string lastType = string.Empty;
                Dictionary<int, string> options = new Dictionary<int, string>();
                do
                {
                    var messageInput = new MessageInput() { Text = text };
                    var result2 = service.Message(assistantId, sessionId, messageInput);
                    Log(result2.Response);
                    var response = JsonConvert.DeserializeObject<Welcome>(result2.Response);
                    if (response.Output.UserDefined?.UiAction?.NotificationDisplay?.DisplayText != null)
                    {
                        DoService("**" + response.Output.UserDefined?.UiAction?.NotificationDisplay?.DisplayText + "**");
                    }

                    foreach (var generic in response.Output.Generic)
                    {
                        switch (generic.ResponseType)
                        {
                            case "text":
                                Print(generic.Text);
                                break;
                            case "option":
                                Print(generic.Title);
                                options.Clear();
                                for (int i = 0; i < generic.Options.Length; i++)
                                {
                                    var option = generic.Options[i];
                                    Console.WriteLine($"{i + 1}: {option.Label}");
                                    options.Add(i + 1, option.Value.Input.Text);
                                }

                                break;
                        }

                        if (generic.Typing.HasValue && generic.Typing.Value)
                        {
                            Print("...");
                            Thread.Sleep(300);
                        }
                    }

                    if (response.Output.Actions != null)
                    {
                        foreach (var action in response.Output.Actions)
                        {
                            DoService($"---> {action.ResultVariable} = {action.Type}.{action.Name}");
                            var messageInput2 = new MessageInput() { Text = "" };
                            MessageContextSkills skills = new MessageContextSkills();
                            MessageContextSkill skill = new MessageContextSkill();
                            skill.UserDefined = new Dictionary<string, object>();
                            skill.UserDefined.Add(action.ResultVariable, "123456");
                            skills.Add("main skill", skill);
                            var messageContext = new MessageContext() { Skills = skills };
                            var result3 = service.Message(assistantId, sessionId, messageInput2, context: messageContext);
                            Log(result3.Response);
                        }
                    }

                    lastType = response.Output.Generic.LastOrDefault()?.ResponseType ?? string.Empty;
                    Console.Write("User: ");
                    text = Console.ReadLine();
                    if (lastType == "option")
                    {
                        if (int.TryParse(text, out int value) && options.ContainsKey(value))
                        {
                            text = options[value];
                        }
                    }
                } while (text != "Exit");
            }
            catch (ServiceResponseException e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }

            Console.WriteLine("\n\nPress any key to close.");
            Console.ReadKey();
        }

        private static void Log(string value)
        {
            if (verboseLogging)
                Console.WriteLine(value);
        }

        private static void Print(string value)
        {
            Console.WriteLine("Bot: " + value);
        }

        private static void DoService(string value)
        {
            Console.WriteLine(value);
        }
    }

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

    internal class ParseStringConverter : JsonConverter
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
