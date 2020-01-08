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
        public List<string> alligments;
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


        public List<string> GetCurrentAlligment()
        {
            if (leftChild == null && rightChild == null)
            {
                List<string> result = new List<string>();
                result.Add(name);
                return result;
            }

            return alligments;
        }

        public void PrintAlligments()
        {
            Console.WriteLine();

            foreach (var alligment in alligments)
            {
                Console.WriteLine(alligment);
            }
        }
    }
}
