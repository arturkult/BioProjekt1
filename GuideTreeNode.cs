using System;
using System.Collections.Generic;
namespace Projekt1
{
    public class GuideTreeNode
    {
        public double distanceToChild;
        public GuideTreeNode leftChild;
        public GuideTreeNode rightChild;
        public string name;

        public GuideTreeNode(double distanceToChild, GuideTreeNode leftChild,
            GuideTreeNode rightChild, string name = "")
        {
            this.distanceToChild = distanceToChild;
            this.leftChild = leftChild;
            this.rightChild = rightChild;
            this.name = name;
        }
    }
}
