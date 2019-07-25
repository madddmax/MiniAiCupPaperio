using System;
using System.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MiniAiCupPaperio
{
    class Program
    {
        static World World;
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
                            World = new World
                            {
                                XCount = model.Params.XCount,
                                YCount = model.Params.YCount,
                                Speed = model.Params.Speed,
                                Width = model.Params.Width
                            };
                        }
                        else if (model.Type == ModelType.EndGame)
                        {
                            break;
                        }
                        else
                        {
                            Me = model.Params.Players.First(p => p.Key == "i").Value;
                            int x = Me.Position[0];
                            int y = Me.Position[1];

                            bool hasBorderTerritory = Me.Territory.Any(t => t[0] == World.MinX || t[0] == World.MaxX || t[1] == World.MinY || t[1] == World.MaxY);
                            if(hasBorderTerritory)
                            {
                                if (y == World.MinY && x < World.MaxX)
                                {
                                    Console.WriteLine("{{\"command\": \"{0}\"}}", Direction.Right);
                                }
                                else if (x == World.MaxX && y < World.MaxY)
                                {
                                    Console.WriteLine("{{\"command\": \"{0}\"}}", Direction.Up);
                                }
                                else if (y == World.MaxY && x > World.MinX)
                                {
                                    Console.WriteLine("{{\"command\": \"{0}\"}}", Direction.Left);
                                }
                                else if (x == World.MinX && y > World.MinY)
                                {
                                    Console.WriteLine("{{\"command\": \"{0}\"}}", Direction.Down);
                                }
                                else
                                {
                                    Console.WriteLine("{{\"command\": \"{0}\"}}", Direction.Left);
                                }

                                continue;
                            }

                            if (Me.Lines.GroupBy(l => l[0]).Count() > 1)
                            {
                                Console.WriteLine("{{\"command\": \"{0}\"}}", Direction.Up);
                            }
                            else if (y == World.MinY && x > World.MinX)
                            {
                                Console.WriteLine("{{\"command\": \"{0}\"}}", Direction.Left);
                            }
                            else
                            {
                                Console.WriteLine("{{\"command\": \"{0}\"}}", Direction.Down);
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
