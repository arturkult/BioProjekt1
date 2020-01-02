using System;
using System.Collections.Generic;
namespace Projekt1
{
    public class GuideTreeNode
    {
        public double distanceToLeftChild;
        public double distanceToRightChild;
        public GuideTreeNode leftChild;
        public GuideTreeNode rightChild;
        public string name;

        public GuideTreeNode(double distanceToLeftChild, double distanceToRightChild, GuideTreeNode leftChild,
            GuideTreeNode rightChild, string name = "")
        {
            this.distanceToLeftChild = distanceToLeftChild;
            this.distanceToRightChild = distanceToRightChild;
            this.leftChild = leftChild;
            this.rightChild = rightChild;
            this.name = name;
        }
    }
}
