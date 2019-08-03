using System.Linq;

namespace MiniAiCupPaperio
{
    public static class Simulator
    {
        public static PlayerModel GetNext(PlayerModel model, string direction)
        {
            var nextModel = (PlayerModel) model.Clone();

            int x = nextModel.Position[0];
            int y = nextModel.Position[1];

            if (direction == Direction.Left)
            {
                x -= World.Width;
                if (x < World.MinX)
                {
                    return null;
                }
            }
            else if(direction == Direction.Right)
            {
                x += World.Width;
                if (x > World.MaxX)
                {
                    return null;
                }
            }
            else if (direction == Direction.Up)
            {
                y += World.Width;
                if (y > World.MaxY)
                {
                    return null;
                }
            }
            else if (direction == Direction.Down)
            {
                y -= World.Width;
                if (y < World.MinY)
                {
                    return null;
                }
            }

            nextModel.Direction = direction;
            nextModel.Position[0] = x;
            nextModel.Position[1] = y;

            if (nextModel.Lines.Any(l => l[0] == x && l[1] == y))
            {
                return null;
            }

            if (!nextModel.Territory.Any(t => t[0] == x && t[1] == y))
            {
                var lines = nextModel.Lines.ToList();
                lines.Add(nextModel.Position);
                nextModel.Lines = lines.ToArray();
            }
            else if (nextModel.Lines.Length > 0)
            {
                nextModel.Score += model.Lines.Length;
            }

            return nextModel;
        }
    }
}
