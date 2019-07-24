using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MiniAiCupPaperio
{
    class Program
    {
        static void Main(string[] args)
        {
            Debugger.Launch();
            while (true)
            {
                try
                {
                    var input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input))
                    {
                        var jObject = JObject.Parse(input);
                        var model = JsonConvert.DeserializeObject<InputModel>(jObject.ToString());

                        if (model.Type == ModelType.StartGame)
                        {
                            continue;
                        }
                        else if (model.Type == ModelType.EndGame)
                        {
                            break;
                        }
                        else
                        {
                            Console.WriteLine("{{\"command\": \"{0}\"}}", Direction.Down);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            } 
        }
    }
}
