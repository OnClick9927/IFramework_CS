﻿using System.Collections.Generic;

namespace IFramework.NodeAction
{
    [ScriptVersion(3)]
    abstract class ContainerNode : ActionNode, IContainerNode
    {
        protected int count
        {
            get
            {
                if (nodeList == null)
                    return -1;
                return nodeList.Count;
            }
        }

        public IActionNode last { get { return nodeList[nodeList.Count - 1]; } }

        protected ContainerNode()
        {
            nodeList = new List<ActionNode>();
        }
        protected List<ActionNode> nodeList;
        internal virtual void Append(ActionNode node)
        {
            nodeList.Add(node);
        }
        protected override void OnRecyle()
        {
            base.OnRecyle();
            for (int i = 0; i < nodeList.Count; i++)
                nodeList[i].Recyle();
            nodeList.Clear();
        }
        //protected override void OnNodeDispose()
        //{
        //    for (int i = 0; i < nodeList.Count; i++)
        //        nodeList[i].Dispose();
        //    nodeList.Clear();
        //    nodeList = null;
        //}

        //protected override void OnDataReset()
        //{
        //    base.OnDataReset();
        //    for (int i = 0; i < nodeList.Count; i++)
        //        nodeList[i].ResetData();
        //}
        protected override void OnNodeReset()
        {
            for (int i = 0; i < nodeList.Count; i++)
                nodeList[i].NodeReset();
        }

    }

}
