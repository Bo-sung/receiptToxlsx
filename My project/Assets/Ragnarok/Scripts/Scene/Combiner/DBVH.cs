using UnityEngine;
using System.Collections.Generic;

namespace EditorTool
{
    public interface IHasAABB
    {
        AABB GetAABB();
    }

    public struct AABB
    {
        private static Vector3[] dirs = new Vector3[] { Vector3.right, Vector3.up, Vector3.forward };

        public Vector3 min;
        public Vector3 max;
        public float surfaceArea;

        public AABB(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;

            surfaceArea = 0f;
            surfaceArea = ComputeSurfaceArea();
        }

        public AABB Merge(ref AABB aabb)
        {
            return new AABB(Vector3.Min(aabb.min, min), Vector3.Max(aabb.max, max));
        }

        public bool Contains(ref AABB aabb)
        {
            return
                aabb.min.x >= min.x &&
                aabb.max.x <= max.x &&
                aabb.min.y >= min.y &&
                aabb.max.y <= max.y &&
                aabb.min.z >= min.z &&
                aabb.max.z <= max.z;
        }

        public bool Overlaps(ref AABB aabb)
        {
            return
                aabb.min.x < max.x &&
                aabb.max.x > min.x &&
                aabb.min.y < max.y &&
                aabb.max.y > min.y &&
                aabb.min.z < max.z &&
                aabb.max.z > min.z;
        }

        public bool OverlapsRay(ref Vector3 pos, ref Vector3 dir)
        {
            for (int i = 0; i < 3; ++i)
            {
                Vector3 cross = Vector3.Cross(dirs[i], dir);

                float o = Vector3.Dot(cross, pos);

                float minProj = float.MaxValue;
                float maxProj = float.MinValue;

                for (int j = 0; j < 8; ++j)
                {
                    float x = ((j & 1) != 0) ? min.x : max.x;
                    float y = ((j & 2) != 0) ? min.y : max.y;
                    float z = ((j & 4) != 0) ? min.z : max.z;

                    float v = Vector3.Dot(cross, new Vector3(x, y, z));

                    minProj = Mathf.Min(v, minProj);
                    maxProj = Mathf.Max(v, maxProj);
                }

                if ((o - minProj) * (o - maxProj) > 0)
                    return false;
            }

            return true;
        }

        private float ComputeSurfaceArea()
        {
            float dx = max.x - min.x;
            float dy = max.y - min.y;
            float dz = max.z - min.z;

            return dx * dy + dy * dz + dz * dz * 2;
        }
    }

    public class AABBTree
    {
        private class AABBNode
        {
            public const int NULL = int.MaxValue;

            public AABB aabb = new AABB();
            public IHasAABB obj = null;

            public int parentNodeIndex = NULL;
            public int leftNodeIndex = NULL;
            public int rightNodeIndex = NULL;
            public int nextNodeIndex = NULL;
            public int id;

            public bool IsLeaf() { return leftNodeIndex == NULL; }

            public void Reset()
            {
                aabb = new AABB();
                obj = null;
                parentNodeIndex = NULL;
                leftNodeIndex = NULL;
                rightNodeIndex = NULL;
                nextNodeIndex = NULL;
                id = -1;
            }
        }

        private Stack<int>[] stackPool;
        private Dictionary<IHasAABB, int> objToIndexMap;
        private List<AABBNode> nodes;

        private int rootNodeIndex;
        private int nextFreeNodeIndex;
        private int growthSize;

        public AABBTree(int initialSize, int threadCount = 1)
        {
            objToIndexMap = new Dictionary<IHasAABB, int>();
            growthSize = initialSize;

            rootNodeIndex = AABBNode.NULL;
            nextFreeNodeIndex = 0;

            nodes = new List<AABBNode>((int)initialSize);

            for (int i = 0; i < initialSize; ++i)
            {
                AABBNode node = new AABBNode();
                nodes.Add(node);
                node.nextNodeIndex = i + 1;
            }

            nodes[nodes.Count - 1].nextNodeIndex = AABBNode.NULL;

            stackPool = new Stack<int>[threadCount];
            for (int i = 0; i < stackPool.Length; ++i)
                stackPool[i] = new Stack<int>();
        }

        private int AllocateNode()
        {
            if (nextFreeNodeIndex == AABBNode.NULL)
            {
                nextFreeNodeIndex = nodes.Count;

                for (int i = 0; i < growthSize; ++i)
                {
                    AABBNode node = new AABBNode();
                    nodes.Add(node);
                    node.nextNodeIndex = nodes.Count;
                }

                nodes[nodes.Count - 1].nextNodeIndex = AABBNode.NULL;
            }

            int nodeIndex = nextFreeNodeIndex;
            AABBNode allocatedNode = nodes[nodeIndex];

            allocatedNode.parentNodeIndex = AABBNode.NULL;
            allocatedNode.leftNodeIndex = AABBNode.NULL;
            allocatedNode.rightNodeIndex = AABBNode.NULL;

            nextFreeNodeIndex = allocatedNode.nextNodeIndex;

            return nodeIndex;
        }

        private void DeallocateNode(int nodeIndex)
        {
            AABBNode deallocatedNode = nodes[nodeIndex];
            deallocatedNode.Reset();
            deallocatedNode.nextNodeIndex = nextFreeNodeIndex;
            nextFreeNodeIndex = nodeIndex;
        }

        public void InsertObject(IHasAABB obj, int id = -1)
        {
            int nodeIndex = AllocateNode();
            nodes[nodeIndex].aabb = obj.GetAABB();
            nodes[nodeIndex].obj = obj;
            nodes[nodeIndex].id = id;

            InsertLeaf(nodeIndex);
            objToIndexMap.Add(obj, nodeIndex);
        }

        public void RemoveObject(IHasAABB obj)
        {
            int nodeIndex = objToIndexMap[obj];
            RemoveLeaf(nodeIndex);
        }

        public void UpdateObject(IHasAABB obj)
        {
            int nodeIndex = objToIndexMap[obj];
            AABB aabb = obj.GetAABB();
            UpdateLeaf(nodeIndex, ref aabb);
        }

        private void InsertLeaf(int leafNodeIndex)
        {
            if (rootNodeIndex == AABBNode.NULL)
            {
                rootNodeIndex = leafNodeIndex;
                return;
            }

            int treeNodeIndex = rootNodeIndex;
            AABBNode leafNode = nodes[leafNodeIndex];

            int loopCount = 0;
            while (nodes[treeNodeIndex].IsLeaf() == false)
            {
                ++loopCount;
                if (loopCount > 10000)
                    throw new System.Exception("Loop_InsertLeaf");

                AABBNode treeNode = nodes[treeNodeIndex];
                int leftNodeIndex = treeNode.leftNodeIndex;
                int rightNodeIndex = treeNode.rightNodeIndex;
                AABBNode leftNode = nodes[leftNodeIndex];
                AABBNode rightNode = nodes[rightNodeIndex];

                AABB combinedAABB = treeNode.aabb.Merge(ref leafNode.aabb);

                float newParentNodeCost = 2 * combinedAABB.surfaceArea;
                float minimumPushDownCost = 2 * (combinedAABB.surfaceArea - treeNode.aabb.surfaceArea);

                float costLeft;
                float costRight;

                if (leftNode.IsLeaf())
                {
                    costLeft = leafNode.aabb.Merge(ref leftNode.aabb).surfaceArea + minimumPushDownCost;
                }
                else
                {
                    AABB newLeftAABB = leftNode.aabb.Merge(ref leftNode.aabb);
                    costLeft = (newLeftAABB.surfaceArea - leftNode.aabb.surfaceArea) + minimumPushDownCost;
                }

                if (rightNode.IsLeaf())
                {
                    costRight = leftNode.aabb.Merge(ref rightNode.aabb).surfaceArea + minimumPushDownCost;
                }
                else
                {
                    AABB newRightAABB = leafNode.aabb.Merge(ref rightNode.aabb);
                    costRight = (newRightAABB.surfaceArea - rightNode.aabb.surfaceArea) + minimumPushDownCost;
                }

                if (newParentNodeCost < costRight && newParentNodeCost < costRight)
                    break;

                if (costLeft < costRight)
                    treeNodeIndex = leftNodeIndex;
                else
                    treeNodeIndex = rightNodeIndex;
            }

            int leafSiblingIndex = treeNodeIndex;
            AABBNode leafSibling = nodes[leafSiblingIndex];
            int oldParentIndex = leafSibling.parentNodeIndex;
            int newParentIndex = AllocateNode();
            AABBNode newParent = nodes[newParentIndex];
            newParent.parentNodeIndex = oldParentIndex;
            newParent.aabb = leafNode.aabb.Merge(ref leafSibling.aabb);
            newParent.leftNodeIndex = leafSiblingIndex;
            newParent.rightNodeIndex = leafNodeIndex;
            leafNode.parentNodeIndex = newParentIndex;
            leafSibling.parentNodeIndex = newParentIndex;

            if (oldParentIndex == AABBNode.NULL)
            {
                rootNodeIndex = newParentIndex;
            }
            else
            {
                AABBNode oldParent = nodes[oldParentIndex];
                if (oldParent.leftNodeIndex == leafSiblingIndex)
                    oldParent.leftNodeIndex = newParentIndex;
                else
                    oldParent.rightNodeIndex = newParentIndex;
            }

            treeNodeIndex = leafNode.parentNodeIndex;
            //Debug.Log(CheckTree(rootNodeIndex));
            FixUpwardTree(treeNodeIndex);
        }

        private bool CheckTree(int treeNodeIndex)
        {
            if (treeNodeIndex == AABBNode.NULL)
                return true;

            if (nodes[treeNodeIndex].leftNodeIndex != AABBNode.NULL)
                if (treeNodeIndex != nodes[nodes[treeNodeIndex].leftNodeIndex].parentNodeIndex)
                    return false;

            if (nodes[treeNodeIndex].rightNodeIndex != AABBNode.NULL)
                if (treeNodeIndex != nodes[nodes[treeNodeIndex].rightNodeIndex].parentNodeIndex)
                    return false;

            if (CheckTree(nodes[treeNodeIndex].leftNodeIndex) == false)
                return false;

            if (CheckTree(nodes[treeNodeIndex].rightNodeIndex) == false)
                return false;

            return true;
        }

        private void RemoveLeaf(int leafNodeIndex)
        {
            if (leafNodeIndex == rootNodeIndex)
            {
                rootNodeIndex = AABBNode.NULL;
                return;
            }

            AABBNode leafNode = nodes[leafNodeIndex];
            int parentNodeIndex = leafNode.parentNodeIndex;
            AABBNode parentNode = nodes[parentNodeIndex];
            int grandParentNodeIndex = parentNode.parentNodeIndex;
            int siblingNodeIndex = parentNode.leftNodeIndex == leafNodeIndex ? parentNode.rightNodeIndex : parentNode.leftNodeIndex;
            AABBNode siblingNode = nodes[siblingNodeIndex];

            if (grandParentNodeIndex != AABBNode.NULL)
            {
                AABBNode grandParentNode = nodes[grandParentNodeIndex];
                if (grandParentNode.leftNodeIndex == parentNodeIndex)
                    grandParentNode.leftNodeIndex = siblingNodeIndex;
                else
                    grandParentNode.rightNodeIndex = siblingNodeIndex;
                siblingNode.parentNodeIndex = grandParentNodeIndex;
                DeallocateNode(parentNodeIndex);
                FixUpwardTree(grandParentNodeIndex);
            }
            else
            {
                rootNodeIndex = siblingNodeIndex;
                siblingNode.parentNodeIndex = AABBNode.NULL;
                DeallocateNode(parentNodeIndex);
            }

            leafNode.parentNodeIndex = AABBNode.NULL;
        }

        private void UpdateLeaf(int leafNodeIndex, ref AABB aabb)
        {
            AABBNode node = nodes[leafNodeIndex];

            if (node.aabb.Contains(ref aabb))
                return;

            RemoveLeaf(leafNodeIndex);
            node.aabb = aabb;
            InsertLeaf(leafNodeIndex);
        }

        private void FixUpwardTree(int treeNodeIndex)
        {
            int loopCount = 0;
            while (treeNodeIndex != AABBNode.NULL)
            {
                ++loopCount;
                if (loopCount > 10000)
                    throw new System.Exception("Loop_FixUpwardTree");

                AABBNode treeNode = nodes[treeNodeIndex];

                AABBNode leftnode = nodes[treeNode.leftNodeIndex];
                AABBNode rightNode = nodes[treeNode.rightNodeIndex];
                treeNode.aabb = leftnode.aabb.Merge(ref rightNode.aabb);

                treeNodeIndex = treeNode.parentNodeIndex;
            }
        }

        public void QueryOverlaps(IHasAABB obj, List<IHasAABB> overlaps, int ignore = -1, int threadIndex = 0)
        {
            overlaps.Clear();
            AABB testAABB = obj.GetAABB();

            Stack<int> stack = stackPool[threadIndex];
            stack.Push(rootNodeIndex);
            while (stack.Count > 0)
            {
                int nodeIndex = stack.Pop();

                if (nodeIndex == AABBNode.NULL) continue;

                AABBNode node = nodes[nodeIndex];
                if (node.aabb.Overlaps(ref testAABB))
                {
                    if (node.IsLeaf() && node.obj != obj && (ignore == -1 || node.id != ignore))
                    {
                        overlaps.Add(node.obj);
                    }
                    else
                    {
                        stack.Push(node.leftNodeIndex);
                        stack.Push(node.rightNodeIndex);
                    }
                }
            }

            stack.Clear();
        }

        public void QueryRay(Vector3 pos, Vector3 dir, List<IHasAABB> overlaps, int ignore = -1, int threadIndex = 0)
        {
            overlaps.Clear();

            Stack<int> stack = stackPool[threadIndex];

            stack.Push(rootNodeIndex);
            while (stack.Count > 0)
            {
                int nodeIndex = stack.Pop();

                if (nodeIndex == AABBNode.NULL) continue;

                AABBNode node = nodes[nodeIndex];
                if (node.aabb.OverlapsRay(ref pos, ref dir))
                {
                    if (node.IsLeaf() && (ignore == -1 || node.id != ignore))
                    {
                        overlaps.Add(node.obj);
                    }
                    else
                    {
                        stack.Push(node.leftNodeIndex);
                        stack.Push(node.rightNodeIndex);
                    }
                }
            }

            stack.Clear();
        }
    }
}