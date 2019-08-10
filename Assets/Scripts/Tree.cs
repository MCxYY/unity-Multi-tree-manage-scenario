using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : INode
{
    public Bounds bound { get; set; }
    private Node root;
    public int maxDepth { get; }
    public int maxChildCount { get; }

    public Tree(Bounds bound)
    {
        this.bound = bound;
        this.maxDepth = 5;
        this.maxChildCount = 4;
        root = new Node(bound, 0, this);
    }

    public void InsertObj(ObjData obj)
    {
        root.InsertObj(obj);
    }

    public void TriggerMove(Camera camera)
    {
        root.TriggerMove(camera);
    }

    public void DrawBound()
    {
        root.DrawBound();
    }
}
