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

                    var me = model.Params.Players.First(p => p.Key == "i").Value;
                    var tree = new TreeNode {Model = me, Parent = null, Depth = 0};
                    BuildTree(tree);


                    var maxScore = _maxDepthNodes.Max(n => n.Model.Score);
                    var maxScoreNode = _maxDepthNodes.First(n => n.Model.Score == maxScore);
                    while (maxScoreNode.Depth != 1)
                    {
                        maxScoreNode = maxScoreNode.Parent;
                    }

                    Console.WriteLine("{{\"command\": \"{0}\"}}", maxScoreNode.Model.Direction);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        static void BuildTree(TreeNode tree)
        {
            var possibleDirections = Direction.GetPossible(tree.Model.Direction);
            foreach (var direction in possibleDirections)
            {
                var next = Simulator.GetNext(tree.Model, direction);
                if (next == null)
                {
                    continue;
                }

                var nextNode = new TreeNode {Model = next, Parent = tree, Depth = tree.Depth + 1 };
                if (nextNode.Depth == MaxDepth)
                {
                    _maxDepthNodes.Add(nextNode);
                    continue;
                }

                BuildTree(nextNode);
            }
        }
    }
}
