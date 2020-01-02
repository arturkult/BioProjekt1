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

        public void joinGroups(int indexOfFirstGroup, int indexOfSecondGroup, double distance)
        {
            double distanceToChild = distance / 2;
            GuideTreeNode newGuideTreeNode =
                new GuideTreeNode(distanceToChild, groups[indexOfFirstGroup], groups[indexOfSecondGroup]);

            updateGroups();
            if (groups.Count == 1)
            {
                rootNode = groups[0];
            }

        }

        private void updateGroups(GuideTreeNode guideTreeNode, int indexOfFirstGroup, int indexOfSecondGroup)
        {

        }

        public void printTree()
        {
            if (rootNode != null)
            {
                printSubtree(rootNode, "");
            }

        }

        private void printSubtree(GuideTreeNode subtreeRoot, string indentation) {
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

                Console.Write(subtreeRoot.distanceToChild + " -> ");
                printSubtree(subtreeRoot.leftChild, indentation + additionalIndentation);
                Console.Write(subtreeRoot.distanceToChild + " -> ");
                printSubtree(subtreeRoot.rightChild, indentation + additionalIndentation);
            }
        }

    }
}
