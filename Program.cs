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
        private const int MaxTickCount = 2500;
        private static int _currentTick = 0;

        private static List<TreeNode> _captureNodes = new List<TreeNode>();
        private static List<TreeNode> _otherNodes = new List<TreeNode>();

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

                    Simulator.Bonuses = model.Params.Bonuses.Select(b => new MapBonus(b)).ToList();
                    Simulator.Enemies = enemyPlayersModel.Select(p => new Player(p)).ToList();
                    ReSetEnemyFearDic(Simulator.Enemies);

                    Simulator.MyTerritory = new HashSet<Point>(myPlayerModel.Territory.Select(t => new Point(t)));
                    foreach (var enemy in enemyPlayersModel)
                    {
                        Simulator.EnemyTerritory.UnionWith(enemy.Territory.Select(t => new Point(t)));
                    }

                    var firstNode = new TreeNode {My = new Player(myPlayerModel), Parent = null, Depth = 0};
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

        private static void ReSetEnemyFearDic(List<Player> enemies)
        {
            var moveSize = World.Width;

            for (int x = World.MinX; x <= World.MaxX; x += moveSize)
            {
                for (int y = World.MinY; y <= World.MaxY; y += moveSize)
                {
                    Simulator.EnemyFearDic[new Point(x, y)] = MaxTickCount;
                }
            }

            foreach (var enemy in enemies)
            {
                Simulator.EnemyFearDic[enemy.Position] = 0;
                for (int i = moveSize; i <= World.MaxX; i += moveSize)
                {
                    int moveScore = i / moveSize;

                    var upKey = new Point(enemy.Position.X, enemy.Position.Y + i);
                    if (Simulator.EnemyFearDic.ContainsKey(upKey))
                    {
                        var upVal = enemy.Direction == Direction.Down ? moveScore + 2 : moveScore;
                        Simulator.EnemyFearDic[upKey] = Simulator.EnemyFearDic[upKey] > upVal
                            ? upVal
                            : Simulator.EnemyFearDic[upKey];
                    }
                    var downKey = new Point(enemy.Position.X, enemy.Position.Y - i);
                    if (Simulator.EnemyFearDic.ContainsKey(downKey))
                    {
                        var downVal = enemy.Direction == Direction.Up ? moveScore + 2 : moveScore;
                        Simulator.EnemyFearDic[downKey] = Simulator.EnemyFearDic[downKey] > downVal
                            ? downVal
                            : Simulator.EnemyFearDic[downKey];
                    }
                    var rightKey = new Point(enemy.Position.X + i, enemy.Position.Y);
                    if (Simulator.EnemyFearDic.ContainsKey(rightKey))
                    {
                        var rightVal = enemy.Direction == Direction.Left ? moveScore + 2 : moveScore;
                        Simulator.EnemyFearDic[rightKey] = Simulator.EnemyFearDic[rightKey] > rightVal
                            ? rightVal
                            : Simulator.EnemyFearDic[rightKey];
                    }
                    var leftKey = new Point(enemy.Position.X - i, enemy.Position.Y);
                    if (Simulator.EnemyFearDic.ContainsKey(leftKey))
                    {
                        var leftVal = enemy.Direction == Direction.Right ? moveScore + 2 : moveScore;
                        Simulator.EnemyFearDic[leftKey] = Simulator.EnemyFearDic[leftKey] > leftVal
                            ? leftVal
                            : Simulator.EnemyFearDic[leftKey];
                    }


                    var diagonalVal = moveScore + 1;
                    for (int j = enemy.Position.Y - i; j <= enemy.Position.Y + i; j += moveSize)
                    {
                        var leftSideKey = new Point(enemy.Position.X - i, j);
                        if (Simulator.EnemyFearDic.ContainsKey(leftSideKey))
                        {
                            Simulator.EnemyFearDic[leftSideKey] = Simulator.EnemyFearDic[leftSideKey] > diagonalVal
                                ? diagonalVal
                                : Simulator.EnemyFearDic[leftSideKey];
                        }
                    }
                    for (int j = enemy.Position.X - i; j <= enemy.Position.X + i; j += moveSize)
                    {
                        var upSideKey = new Point(j, enemy.Position.Y + i);
                        if (Simulator.EnemyFearDic.ContainsKey(upSideKey))
                        {
                            Simulator.EnemyFearDic[upSideKey] = Simulator.EnemyFearDic[upSideKey] > diagonalVal
                                ? diagonalVal
                                : Simulator.EnemyFearDic[upSideKey];
                        }
                    }
                    for (int j = enemy.Position.Y - i; j <= enemy.Position.Y + i; j += moveSize)
                    {
                        var rightSideKey = new Point(enemy.Position.X + i, j);
                        if (Simulator.EnemyFearDic.ContainsKey(rightSideKey))
                        {
                            Simulator.EnemyFearDic[rightSideKey] = Simulator.EnemyFearDic[rightSideKey] > diagonalVal
                                ? diagonalVal
                                : Simulator.EnemyFearDic[rightSideKey];
                        }
                    }
                    for (int j = enemy.Position.X - i; j <= enemy.Position.X + i; j += moveSize)
                    {
                        var downSideKey = new Point(j, enemy.Position.Y - i);
                        if (Simulator.EnemyFearDic.ContainsKey(downSideKey))
                        {
                            Simulator.EnemyFearDic[downSideKey] = Simulator.EnemyFearDic[downSideKey] > diagonalVal
                                ? diagonalVal
                                : Simulator.EnemyFearDic[downSideKey];
                        }
                    }
                }
            }
        }
    }
}
