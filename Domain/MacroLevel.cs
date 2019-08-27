using System.Linq;

namespace MiniAiCupPaperio
{
    public static class MacroLevel
    {
        private static int _leftScore;
        private static int _rightScore;
        private static int _upScore;
        private static int _downScore;
        private static int _totalScore;

        public static int GetDirectionScore(Point position, string direction)
        {
            // притяжение к территории противника
            int directionBonus = 0;

            // отталкивание от собственной территории
            int directionPenalty = 0;

            double ratio = 0;
            if (direction == Direction.Left)
            {
                ratio = (double)_leftScore / _totalScore;

                if (Global.MyTerritory.Contains(position) && 
                    Global.MyTerritory.Contains(new Point(position.X - World.Width, position.Y)) && 
                    Global.MyTerritory.Contains(new Point(position.X - World.Width * 2, position.Y)))
                {
                    directionPenalty = 1;
                }
            }
            else if (direction == Direction.Right)
            {
                ratio = (double)_rightScore / _totalScore;

                if (Global.MyTerritory.Contains(position) && 
                    Global.MyTerritory.Contains(new Point(position.X + World.Width, position.Y)) && 
                    Global.MyTerritory.Contains(new Point(position.X + World.Width * 2, position.Y)))
                {
                    directionPenalty = 1;
                }
            }
            else if (direction == Direction.Up)
            {
                ratio = (double)_upScore / _totalScore;

                if (Global.MyTerritory.Contains(position) && 
                    Global.MyTerritory.Contains(new Point(position.X, position.Y - World.Width)) && 
                    Global.MyTerritory.Contains(new Point(position.X, position.Y - World.Width * 2)))
                {
                    directionPenalty = 1;
                }
            }
            else if (direction == Direction.Down)
            {
                ratio = (double) _downScore / _totalScore;

                if (Global.MyTerritory.Contains(position) && 
                    Global.MyTerritory.Contains(new Point(position.X, position.Y + World.Width)) && 
                    Global.MyTerritory.Contains(new Point(position.X, position.Y + World.Width * 2)))
                {
                    directionPenalty = 1;
                }
            }

            if (ratio > 0.3)
            {
                directionBonus = 1;
            }

            if (ratio > 0.6)
            {
                directionBonus = 2;
            }

            return directionBonus - directionPenalty;
        }

        public static void SetDirectionsScore(Point position)
        {
            var moveSize = World.Width;
            for (int i = moveSize; i <= World.MaxX; i += moveSize)
            {
                var upKey = new Point(position.X, position.Y + i);
                if (PointExtension.IsOnTheWorld(upKey))
                {
                    _upScore += GetPointScore(upKey);
                }

                var downKey = new Point(position.X, position.Y - i);
                if (PointExtension.IsOnTheWorld(downKey))
                {
                    _downScore += GetPointScore(downKey);
                }

                var rightKey = new Point(position.X + i, position.Y);
                if (PointExtension.IsOnTheWorld(rightKey))
                {
                    _rightScore += GetPointScore(rightKey);
                }

                var leftKey = new Point(position.X - i, position.Y);
                if (PointExtension.IsOnTheWorld(leftKey))
                {
                    _leftScore += GetPointScore(leftKey);
                }


                for (int j = position.Y - i; j <= position.Y + i; j += moveSize)
                {
                    var leftSideKey = new Point(position.X - i, j);
                    if (PointExtension.IsOnTheWorld(leftSideKey))
                    {
                        _leftScore += GetPointScore(leftSideKey);
                    }
                }

                for (int j = position.X - i; j <= position.X + i; j += moveSize)
                {
                    var upSideKey = new Point(j, position.Y + i);
                    if (PointExtension.IsOnTheWorld(upSideKey))
                    {
                        _upScore += GetPointScore(upSideKey);
                    }
                }

                for (int j = position.Y - i; j <= position.Y + i; j += moveSize)
                {
                    var rightSideKey = new Point(position.X + i, j);
                    if (PointExtension.IsOnTheWorld(rightSideKey))
                    {
                        _rightScore += GetPointScore(rightSideKey);
                    }
                }

                for (int j = position.X - i; j <= position.X + i; j += moveSize)
                {
                    var downSideKey = new Point(j, position.Y - i);
                    if (PointExtension.IsOnTheWorld(downSideKey))
                    {
                        _downScore += GetPointScore(downSideKey);
                    }
                }
            }

            _totalScore = _leftScore + _rightScore + _upScore + _downScore;
        }

        private static int GetPointScore(Point point)
        {
            if (Global.Enemies.Any(e => e.Position.Equals(point)))
            {
                return -30;
            }

            if (Global.EnemyTerritory.Contains(point))
            {
                return 5;
            }

            if (Global.MyTerritory.Contains(point))
            {
                return 0;
            }

            return 1;
        }
    }
}
