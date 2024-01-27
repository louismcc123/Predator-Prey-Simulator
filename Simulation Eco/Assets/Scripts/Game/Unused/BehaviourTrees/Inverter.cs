using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inverter : Node
{
    protected Node node;

    public Inverter(Node node)
    {
        this.node = node;
    }

    public override NodeState Evaluate()
    {
        switch (node.Evaluate())
        {
            case NodeState.Running:
            _nodeState = NodeState.Running;
                break;
            case NodeState.Success:
            _nodeState = NodeState.Failure;
                break;
            case NodeState.Failure:
                _nodeState = NodeState.Success;
                break;
            default:
                break;
        }
        return _nodeState;
    }
}
