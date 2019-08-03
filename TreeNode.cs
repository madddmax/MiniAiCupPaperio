using System.Collections.Generic;

namespace MiniAiCupPaperio
{
    public class TreeNode
    {
        public PlayerModel Model { get; set; }
        public TreeNode Parent { get; set; }
        public int Depth { get; set; }
    }
}
