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
                    var myPlayer = new Player(myPlayerModel);

                    Simulator.Bonuses = model.Params.Bonuses.Select(b => new MapBonus(b)).ToList();
                    Simulator.Enemies = enemyPlayersModel.Select(p => new Player(p)).ToList();
                    Simulator.MyTerritory = new HashSet<Point>(myPlayerModel.Territory.Select(t => new Point(t)));
                    foreach (var enemy in enemyPlayersModel)
                    {
                        Simulator.EnemyTerritory.UnionWith(enemy.Territory.Select(t => new Point(t)));
                    }

                    ReSetMovesDic(Simulator.Enemies, myPlayer, Simulator.MyTerritory);
                    var firstNode = new TreeNode {My = myPlayer, Parent = null, Depth = 0};
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

        private static void ReSetMovesDic(List<Player> enemies, Player myPlayer, HashSet<Point> myTerritory)
        {
            var moveSize = World.Width;

            for (int x = World.MinX; x <= World.MaxX; x += moveSize)
            {
                for (int y = World.MinY; y <= World.MaxY; y += moveSize)
                {
                    Simulator.EnemyMoveDic[new Point(x, y)] = MaxTickCount;
                }
            }

            foreach (var enemy in enemies)
            {
                // доводим до точки поворота
                var turnEnemyPosition = enemy.Position;
                if (!Simulator.EnemyMoveDic.ContainsKey(turnEnemyPosition))
                {
                    for (int i = 5; i <= 25; i += 5)
                    {
                        turnEnemyPosition.X += i;
                        if (Simulator.EnemyMoveDic.ContainsKey(turnEnemyPosition))
                        {
                            break;
                        }
                    }

                    turnEnemyPosition = enemy.Position;
                    for (int i = 5; i <= 25; i += 5)
                    {
                        turnEnemyPosition.Y += i;
                        if (Simulator.EnemyMoveDic.ContainsKey(turnEnemyPosition))
                        {
                            break;
                        }
                    }
                }

                SetMoveDic(Simulator.EnemyMoveDic, turnEnemyPosition, enemy.Direction);
            }
        }

        public static int GetMovesToTerritory(Player myPlayer, HashSet<Point> territory)
        {
            var moveSize = World.Width;
            var position = myPlayer.Position;
            var direction = myPlayer.Direction;

            for (int i = moveSize; i <= MaxDepth * moveSize; i += moveSize)
            {
                int moveScore = i / moveSize;

                var upKey = new Point(position.X, position.Y + i);
                if (territory.Contains(upKey))
                {
                    return direction == Direction.Down ? moveScore + 2 : moveScore;
                }
                var downKey = new Point(position.X, position.Y - i);
                if (territory.Contains(downKey))
                {
                    return direction == Direction.Up ? moveScore + 2 : moveScore;
                }
                var rightKey = new Point(position.X + i, position.Y);
                if (territory.Contains(rightKey))
                {
                    return direction == Direction.Left ? moveScore + 2 : moveScore;
                }
                var leftKey = new Point(position.X - i, position.Y);
                if (territory.Contains(leftKey))
                {
                    return direction == Direction.Right ? moveScore + 2 : moveScore;
                }


                var diagonalVal = moveScore + 1;
                for (int j = position.Y - i; j <= position.Y + i; j += moveSize)
                {
                    var leftSideKey = new Point(position.X - i, j);
                    if (territory.Contains(leftSideKey))
                    {
                        return diagonalVal;
                    }
                }
                for (int j = position.X - i; j <= position.X + i; j += moveSize)
                {
                    var upSideKey = new Point(j, position.Y + i);
                    if (territory.Contains(upSideKey))
                    {
                        return diagonalVal;
                    }
                }
                for (int j = position.Y - i; j <= position.Y + i; j += moveSize)
                {
                    var rightSideKey = new Point(position.X + i, j);
                    if (territory.Contains(rightSideKey))
                    {
                        return diagonalVal;
                    }
                }
                for (int j = position.X - i; j <= position.X + i; j += moveSize)
                {
                    var downSideKey = new Point(j, position.Y - i);
                    if (territory.Contains(downSideKey))
                    {
                        return diagonalVal;
                    }
                }
            }
            return 30;
        }

        private static void SetMoveDic(Dictionary<Point, int> moveDic, Point position, string direction)
        {
            var moveSize = World.Width;

            moveDic[position] = 0;
            for (int i = moveSize; i <= World.MaxX; i += moveSize)
            {
                int moveScore = i / moveSize;

                var upKey = new Point(position.X, position.Y + i);
                if (moveDic.ContainsKey(upKey))
                {
                    var upVal = direction == Direction.Down ? moveScore + 2 : moveScore;
                    moveDic[upKey] = moveDic[upKey] > upVal
                        ? upVal
                        : moveDic[upKey];
                }
                var downKey = new Point(position.X, position.Y - i);
                if (moveDic.ContainsKey(downKey))
                {
                    var downVal = direction == Direction.Up ? moveScore + 2 : moveScore;
                    moveDic[downKey] = moveDic[downKey] > downVal
                        ? downVal
                        : moveDic[downKey];
                }
                var rightKey = new Point(position.X + i, position.Y);
                if (moveDic.ContainsKey(rightKey))
                {
                    var rightVal = direction == Direction.Left ? moveScore + 2 : moveScore;
                    moveDic[rightKey] = moveDic[rightKey] > rightVal
                        ? rightVal
                        : moveDic[rightKey];
                }
                var leftKey = new Point(position.X - i, position.Y);
                if (moveDic.ContainsKey(leftKey))
                {
                    var leftVal = direction == Direction.Right ? moveScore + 2 : moveScore;
                    moveDic[leftKey] = moveDic[leftKey] > leftVal
                        ? leftVal
                        : moveDic[leftKey];
                }


                var diagonalVal = moveScore + 1;
                for (int j = position.Y - i; j <= position.Y + i; j += moveSize)
                {
                    var leftSideKey = new Point(position.X - i, j);
                    if (moveDic.ContainsKey(leftSideKey))
                    {
                        moveDic[leftSideKey] = moveDic[leftSideKey] > diagonalVal
                            ? diagonalVal
                            : moveDic[leftSideKey];
                    }
                }
                for (int j = position.X - i; j <= position.X + i; j += moveSize)
                {
                    var upSideKey = new Point(j, position.Y + i);
                    if (moveDic.ContainsKey(upSideKey))
                    {
                        moveDic[upSideKey] = moveDic[upSideKey] > diagonalVal
                            ? diagonalVal
                            : moveDic[upSideKey];
                    }
                }
                for (int j = position.Y - i; j <= position.Y + i; j += moveSize)
                {
                    var rightSideKey = new Point(position.X + i, j);
                    if (moveDic.ContainsKey(rightSideKey))
                    {
                        moveDic[rightSideKey] = moveDic[rightSideKey] > diagonalVal
                            ? diagonalVal
                            : moveDic[rightSideKey];
                    }
                }
                for (int j = position.X - i; j <= position.X + i; j += moveSize)
                {
                    var downSideKey = new Point(j, position.Y - i);
                    if (moveDic.ContainsKey(downSideKey))
                    {
                        moveDic[downSideKey] = moveDic[downSideKey] > diagonalVal
                            ? diagonalVal
                            : moveDic[downSideKey];
                    }
                }
            }
        }
    }
}
