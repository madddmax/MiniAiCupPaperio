using System;
using System.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MiniAiCupPaperio
{
    class Program
    {
        static PlayerModel Me;

        static void Main(string[] args)
        {
            //Debugger.Launch();
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
                            World.XCount = model.Params.XCount;
                            World.YCount = model.Params.YCount;
                            World.Speed = model.Params.Speed;
                            World.Width = model.Params.Width;
                        }
                        else if (model.Type == ModelType.EndGame)
                        {
                            break;
                        }
                        else
                        {
                            Me = model.Params.Players.First(p => p.Key == "i").Value;
                            var possibleDirections = Direction.GetPossible(Me.Direction);
                            foreach (var direction in possibleDirections)
                            {
                                var nextModel = Simulator.GetNext(Me, direction);
                                if (nextModel == null)
                                {
                                    continue;
                                }

                                Console.WriteLine("{{\"command\": \"{0}\"}}", direction);
                                break;
                            }

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
