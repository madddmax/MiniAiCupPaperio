using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MiniAiCupPaperio
{
    class Program
    {
        private const int MaxDepth = 5; // 7
        static List<TreeNode> _maxDepthNodes = new List<TreeNode>();

        static void Main(string[] args)
        {
            //Debugger.Launch();
            while (true)
            {
                try
                {
                    var input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input))
                    {
                        continue;
                    }

                    var jObject = JObject.Parse(input);
                    var model = JsonConvert.DeserializeObject<InputModel>(jObject.ToString());

                    if (model.Type == ModelType.StartGame)
                    {
                        World.XCount = model.Params.XCount;
                        World.YCount = model.Params.YCount;
                        World.Speed = model.Params.Speed;
                        World.Width = model.Params.Width;
                        continue;
                    }

                    if (model.Type == ModelType.EndGame)
                    {
                        break;
                    }

                    _maxDepthNodes = new List<TreeNode>();

                    var my = model.Params.Players.First(p => p.Key == "i").Value;
                    var enemies = model.Params.Players.Where(p => p.Key != "i").Select(p => p.Value).ToList();
                    var tree = new TreeNode {My = my, Parent = null, Depth = 0};

                    BuildTree(tree, enemies);

                    var maxScore = _maxDepthNodes.Max(n => n.My.Score);
                    var maxScoreNode = _maxDepthNodes.First(n => n.My.Score == maxScore);
                    while (maxScoreNode.Depth != 1)
                    {
                        maxScoreNode = maxScoreNode.Parent;
                    }

                    Console.WriteLine("{{\"command\": \"{0}\"}}", maxScoreNode.My.Direction);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        static void BuildTree(TreeNode tree, List<PlayerModel> enemies)
        {
            var possibleDirections = Direction.GetPossible(tree.My.Direction);
            foreach (var direction in possibleDirections)
            {
                var next = Simulator.GetNext(tree.My, direction, enemies, tree.Depth);
                if (next == null)
                {
                    continue;
                }

                var nextNode = new TreeNode {My = next, Parent = tree, Depth = tree.Depth + 1 };
                if (nextNode.Depth == MaxDepth)
                {
                    _maxDepthNodes.Add(nextNode);
                    continue;
                }

                BuildTree(nextNode, enemies);
            }
        }
    }
}
