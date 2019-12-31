using System;
using System.Collections.Generic;
namespace Projekt1
{
    public class GuideTreeNode
    {
        public double distanceToLeftChild;
        public GuideTreeNode leftChild;

        public double distanceToRightChild;
        public GuideTreeNode rightChild;

        public string name;

        public GuideTreeNode(double distanceToLeftChild, GuideTreeNode leftChild,
            double distanceToRightChild, GuideTreeNode rightChild, string name)
        {
            this.distanceToLeftChild = distanceToLeftChild;
            this.leftChild = leftChild;
            this.distanceToRightChild = distanceToRightChild;
            this.rightChild = rightChild;
            this.name = name;
        }
    }
}
