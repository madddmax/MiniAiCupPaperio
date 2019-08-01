using System.Linq;

namespace MiniAiCupPaperio
{
    public static class Simulator
    {
        public static PlayerModel GetNext(PlayerModel me, string direction)
        {
            var nextModel = (PlayerModel) me.Clone();

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

            if (me.Lines.Any(l => l[0] == x && l[1] == y))
            {
                return null;
            }

            nextModel.Position[0] = x;
            nextModel.Position[1] = y;

            return nextModel;
        }
    }
}
