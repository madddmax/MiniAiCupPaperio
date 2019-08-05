﻿using System;
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
        private static List<TreeNode> _allNodes = new List<TreeNode>();

        private const int MaxTickCount = 1500;
        static int _currentTick = 0;

        private static void Main(string[] args)
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

                    _allNodes = new List<TreeNode>();
                    _currentTick = model.Params.TickNum;
                    Simulator.Enemies = model.Params.Players.Where(p => p.Key != "i").Select(p => p.Value).ToList();

                    var my = model.Params.Players.First(p => p.Key == "i").Value;
                    var tree = new TreeNode {My = my, Parent = null, Depth = 0};

                    BuildTree(tree);

                    var maxScore = _allNodes.Max(n => n.My.Score);
                    var maxScoreNode = _allNodes.First(n => n.My.Score == maxScore);
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

        private static void BuildTree(TreeNode tree)
        {
            var possibleDirections = Direction.GetPossible(tree.My.Direction);
            foreach (var direction in possibleDirections)
            {
                var next = Simulator.GetNext(tree.My, direction, tree.Depth);
                if (next == null)
                {
                    // движение невозможно/опасно
                    continue;
                }

                var nextNode = new TreeNode {My = next, Parent = tree, Depth = tree.Depth + 1 };
                _allNodes.Add(nextNode);
                if (nextNode.Depth == MaxDepth)
                {
                    // достигнута максимальная глубина рассчета
                    continue;
                }

                if (_currentTick + (nextNode.Depth + 1) * World.OneMoveTicks > MaxTickCount)
                {
                    // рассчет невозможен, т.к. конец игры
                    continue;
                }

                BuildTree(nextNode);
            }
        }
    }
}
