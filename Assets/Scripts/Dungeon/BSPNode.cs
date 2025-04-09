namespace RogueNightmare.Dungeon
{
    using System.Collections.Generic;
    using UnityEngine;

    public class BSPNode
    {
        // Binary Space Partitioning
        private RectInt area;
        private BSPNode left;
        private BSPNode right;
        private List<RectInt> rooms = new List<RectInt>();
        private const int minSplitSize = 10;

        public BSPNode(RectInt area)
        {
            this.area = area;
        }

        public List<RectInt> Split(int minSize, int maxSize)
        {
            Queue<BSPNode> nodes = new Queue<BSPNode>();
            List<RectInt> finalRooms = new List<RectInt>();
            nodes.Enqueue(this);

            while (nodes.Count > 0)
            {
                BSPNode node = nodes.Dequeue();
                if (node.area.width >= maxSize || node.area.height >= maxSize)
                {
                    if (node.SplitNode(minSize))
                    {
                        nodes.Enqueue(node.left);
                        nodes.Enqueue(node.right);
                        continue;
                    }
                }

                RectInt room = node.CreateRoom(minSize);
                finalRooms.Add(room);
            }

            return finalRooms;
        }

        private bool SplitNode(int minSize)
        {
            bool splitH = Random.value > 0.5f;

            if (area.width > area.height && area.height / area.width >= 1.25f)
                splitH = false;
            else if (area.height > area.width && area.width / area.height >= 1.25f)
                splitH = true;

            int max = (splitH ? area.height : area.width) - minSize;
            if (max <= minSize)
                return false;

            int split = Random.Range(minSize, max);

            if (splitH)
            {
                left = new BSPNode(new RectInt(area.xMin, area.yMin, area.width, split));
                right = new BSPNode(new RectInt(area.xMin, area.yMin + split, area.width, area.height - split));
            }
            else
            {
                left = new BSPNode(new RectInt(area.xMin, area.yMin, split, area.height));
                right = new BSPNode(new RectInt(area.xMin + split, area.yMin, area.width - split, area.height));
            }

            return true;
        }

        private RectInt CreateRoom(int minSize)
        {
            int roomWidth = Random.Range(minSize, area.width - 1);
            int roomHeight = Random.Range(minSize, area.height - 1);
            int roomX = Random.Range(area.xMin + 1, area.xMax - roomWidth - 1);
            int roomY = Random.Range(area.yMin + 1, area.yMax - roomHeight - 1);

            return new RectInt(roomX, roomY, roomWidth, roomHeight);
        }
    }
}