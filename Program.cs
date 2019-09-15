using IBM.Cloud.SDK.Core.Authentication.Iam;
using IBM.Cloud.SDK.Core.Http.Exceptions;
using IBM.Watson.Assistant.v2;
using IBM.Watson.Assistant.v2.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WatsonAssistantInterface.Properties;

namespace WatsonAssistantInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            IamConfig config = new IamConfig(Settings.Default.ApiKey);
            AssistantService service = new AssistantService(Settings.Default.ApiVersion, config);
            service.SetEndpoint(Settings.Default.ApiUrl);

            try
            {
                var sessionResponse = service.CreateSession(Settings.Default.AssistantId);
                Log(sessionResponse.Response);
                var sessionId = sessionResponse.Result.SessionId;
                string text = "Hello";
                string lastType = string.Empty;
                Dictionary<int, string> options = new Dictionary<int, string>();
                do
                {
                    var messageInput = new MessageInput() { Text = text };
                    var messageResponse = service.Message(Settings.Default.AssistantId, sessionId, messageInput);
                    Log(messageResponse.Response);
                    var response = JsonConvert.DeserializeObject<Welcome>(messageResponse.Response);
                    if (response.Output.UserDefined?.UiAction?.NotificationDisplay?.DisplayText != null)
                    {
                        DoService("***" + response.Output.UserDefined?.UiAction?.NotificationDisplay?.DisplayText + "***");
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
                            Thread.Sleep(Settings.Default.TypingTime);
                        }
                    }

                    if (response.Output.Actions != null)
                    {
                        foreach (var action in response.Output.Actions)
                        {
                            DoService($"---> {action.ResultVariable} = {action.Type}.{action.Name}");
                            var actionMessageInput = new MessageInput() { Text = "" };
                            MessageContextSkills skills = new MessageContextSkills();
                            MessageContextSkill skill = new MessageContextSkill();
                            skill.UserDefined = new Dictionary<string, object>();
                            skill.UserDefined.Add(action.ResultVariable, "123456");
                            skills.Add("main skill", skill);
                            var messageContext = new MessageContext() { Skills = skills };
                            var actionMessage = service.Message(Settings.Default.AssistantId, sessionId, actionMessageInput, context: messageContext);
                            Log(actionMessage.Response);
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
                } while (text != Settings.Default.ExitKeyword);
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
            if (Settings.Default.VerboseLogging)
            {
                Console.WriteLine(value);
            }
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
}
