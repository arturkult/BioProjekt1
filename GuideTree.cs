using System;
using System.Collections.Generic;

namespace Projekt1
{
    public class GuideTree
    {
        public GuideTreeNode rootNode;
        private List<GuideTreeNode> groups;

        public GuideTree(List<GuideTreeNode> groups)
        {
            this.groups = groups;
        }

        public void JoinGroups(int indexOfFirstGroup, int indexOfSecondGroup, double distance)
        {
            double distanceToChild = distance / 2;
            GuideTreeNode newGuideTreeNode =
                new GuideTreeNode(distanceToChild, groups[indexOfFirstGroup], groups[indexOfSecondGroup]);

            UpdateGroups(newGuideTreeNode, indexOfFirstGroup, indexOfSecondGroup);
            if (groups.Count == 1)
            {
                rootNode = groups[0];
            }

        }

        private void UpdateGroups(GuideTreeNode newGuideTreeNode, int indexOfFirstGroup, int indexOfSecondGroup)
        {
            List<GuideTreeNode> newGroups = new List<GuideTreeNode>();

            for (int i = 0; i < groups.Count; i++)
            {
                if (i != indexOfFirstGroup && i != indexOfSecondGroup)
                {
                    newGroups.Add(groups[i]);
                }
                else if (i == indexOfFirstGroup)
                {
                    newGroups.Add(newGuideTreeNode);
                }
            }

            groups = newGroups;
        }

        public void PrintTree()
        {
            if (rootNode != null)
            {
                PrintSubtree(rootNode, "");
            }

        }

        private void PrintSubtree(GuideTreeNode subtreeRoot, string indentation) {
            if (subtreeRoot == null)
            {
                Console.WriteLine();
            }
            else if (subtreeRoot.leftChild == null || subtreeRoot.rightChild == null)
            {
                Console.WriteLine(indentation + subtreeRoot.name);
            }
            else
            {
                string additionalIndentation = "\t\t";

                Console.WriteLine(indentation + subtreeRoot.distanceToChild + " -> ");
                PrintSubtree(subtreeRoot.leftChild, indentation + additionalIndentation);
                Console.WriteLine(indentation + subtreeRoot.distanceToChild + " -> ");
                PrintSubtree(subtreeRoot.rightChild, indentation + additionalIndentation);
            }
        }

    }
}
