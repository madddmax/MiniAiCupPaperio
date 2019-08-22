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
        private const int MaxDepth = 9;
        private static List<TreeNode> _captureNodes = new List<TreeNode>();
        private static List<TreeNode> _otherNodes = new List<TreeNode>();

        private const int MaxTickCount = 2500;
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

                    _captureNodes = new List<TreeNode>();
                    _otherNodes = new List<TreeNode>();
                    _currentTick = model.Params.TickNum;

                    var myPlayerModel = model.Params.Players.First(p => p.Key == "i").Value;
                    var enemyPlayersModel = model.Params.Players.Where(p => p.Key != "i").Select(p => p.Value).ToList();

                    Simulator.MapBonuses = model.Params.Bonuses.Select(b => new MapBonus(b)).ToList();
                    Simulator.Enemies = enemyPlayersModel.Select(p => new EnemyPlayer(p)).ToList();
                    Simulator.MyTerritory = new HashSet<Point>(myPlayerModel.Territory.Select(t => new Point(t)));
                    foreach (var enemy in enemyPlayersModel)
                    {
                        Simulator.EnemyTerritory.UnionWith(enemy.Territory.Select(t => new Point(t)));
                    }

                    var firstNode = new TreeNode {My = new Player(myPlayerModel), Parent = null, Depth = 0};
                    SetScore(firstNode.My.Position);
                    BuildTree(firstNode);

                    var nodes = _captureNodes.Count > 0 ? _captureNodes : _otherNodes;
                    var maxScoreNode = nodes.OrderByDescending(n => n.My.Score).ThenBy(n => n.Depth).First();
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
            var possibleDirections = DirectionExtension.GetPossible(tree.My.Direction);
            foreach (var direction in possibleDirections)
            {
                var next = Simulator.GetNext(tree.My, direction, tree.Depth);
                if (next == null)
                {
                    // движение невозможно
                    continue;
                }

                var nextNode = new TreeNode {My = next, Parent = tree, Depth = tree.Depth + 1};
                if (nextNode.My.HasCapture)
                {
                    // произведен захват территории
                    _captureNodes.Add(nextNode);
                    continue;
                }

                _otherNodes.Add(nextNode);

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

        private static void SetScore(Point position)
        {
            var moveSize = World.Width;
            for (int i = moveSize; i <= World.MaxX; i += moveSize)
            {
                var upKey = new Point(position.X, position.Y + i);
                if (ContainsKey(upKey))
                {
                    Simulator.UpScore += GetScore(upKey);
                }

                var downKey = new Point(position.X, position.Y - i);
                if (ContainsKey(downKey))
                {
                    Simulator.DownScore += GetScore(downKey);
                }

                var rightKey = new Point(position.X + i, position.Y);
                if (ContainsKey(rightKey))
                {
                    Simulator.RightScore += GetScore(rightKey);
                }

                var leftKey = new Point(position.X - i, position.Y);
                if (ContainsKey(leftKey))
                {
                    Simulator.LeftScore += GetScore(leftKey);
                }


                for (int j = position.Y - i; j <= position.Y + i; j += moveSize)
                {
                    var leftSideKey = new Point(position.X - i, j);
                    if (ContainsKey(leftSideKey))
                    {
                        Simulator.LeftScore += GetScore(leftSideKey);
                    }
                }

                for (int j = position.X - i; j <= position.X + i; j += moveSize)
                {
                    var upSideKey = new Point(j, position.Y + i);
                    if (ContainsKey(upSideKey))
                    {
                        Simulator.UpScore += GetScore(upSideKey);
                    }
                }

                for (int j = position.Y - i; j <= position.Y + i; j += moveSize)
                {
                    var rightSideKey = new Point(position.X + i, j);
                    if (ContainsKey(rightSideKey))
                    {
                        Simulator.RightScore += GetScore(rightSideKey);
                    }
                }

                for (int j = position.X - i; j <= position.X + i; j += moveSize)
                {
                    var downSideKey = new Point(j, position.Y - i);
                    if (ContainsKey(downSideKey))
                    {
                        Simulator.DownScore += GetScore(downSideKey);
                    }
                }
            }

            Simulator.TotalScore = Simulator.LeftScore + Simulator.RightScore + Simulator.UpScore + Simulator.DownScore;
        }

        private static bool ContainsKey(Point point)
        {
            return point.X >= World.MinX && point.X <= World.MaxX &&
                   point.Y >= World.MinY && point.Y <= World.MaxY;
        }

        private static int GetScore(Point point)
        {
            if (Simulator.Enemies.Any(e => e.Position.Equals(point)))
            {
                return -30;
            }

            if (Simulator.EnemyTerritory.Contains(point))
            {
                return 5;
            }

            if (Simulator.MyTerritory.Contains(point))
            {
                return 0;
            }

            return 1;
        }
    }
}
