using System;
namespace Projekt1
{
    public class GuideTree
    {
        public GuideTreeNode rootNode = null;

        public GuideTree()
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

                Console.Write(subtreeRoot.distanceToLeftChild + " -> ");
                printSubtree(subtreeRoot.leftChild, indentation + additionalIndentation);
                Console.Write(subtreeRoot.distanceToRightChild + " -> ");
                printSubtree(subtreeRoot.rightChild, indentation + additionalIndentation);
            }

        }

    }
}
