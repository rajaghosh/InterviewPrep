using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Basic
{
    public class Node
    {
        public int data;
        public Node next;

        public Node(int info)
        {
            data = info;
            next = null;
        }
        public void Print() //Print the Linked List
        {
            Console.WriteLine("|" + data + "->");
            if (next != null)
            {
                next.Print();
            }
        }

        public void AddNewNodeAtEnd(int data)
        {
            if (next == null)
            {
                next = new Node(data);
            }
            else
            {
                next.AddNewNodeAtEnd(data);
            }
        }
    }

    public class MyList
    {
        public Node headNode;

        public MyList()
        {
            headNode = null;
        }

        public void AddToEnd(int data)
        {
            if (headNode == null)
            {
                headNode = new Node(data);

            }
            else
            {
                headNode.AddNewNodeAtEnd(data);
            }
        }

        public void Print()
        {
            if (headNode != null)
            {
                headNode.Print();
            }
        }
    }


    class LinkedList
    {
        public LinkedList()
        {
            //Node myNode = new Node(10);
            //myNode.AddNewNodeAtEnd(11);
            //myNode.AddNewNodeAtEnd(12);

            //myNode.Print();

            MyList list = new MyList();
            list.AddToEnd(1);
            list.AddToEnd(2);
            list.AddToEnd(3);
            list.Print();
        }
    }
}
