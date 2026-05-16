using System;
using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace FSMGraph
{
    [ScriptedImporter(1, FSMGraph.AssetExtension)]
    internal class FSMImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var graph = GraphDatabase.LoadGraphForImporter<FSMGraph>(ctx.assetPath);
            if (graph == null)
            {
                Debug.LogError($"Failed to load FSM graph asset: {ctx.assetPath}");
                return;
            }

            var runtimeAsset = ScriptableObject.CreateInstance<FSMRuntimeGraph>();
            var nodeMap = new Dictionary<INode, FSMRuntimeNode>();

            foreach (var node in graph.GetNodes())
            {
                FSMRuntimeNode runtimeNode = node switch
                {
                    StateNode stateNode => new FSMRuntimeStateNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        StateType = TryGetOptionValue<string>(stateNode, StateNode.StateNameOptionName),
                    },
                    TransitionNode transitionNode => new FSMRuntimeTransitionNode
                    {
                        Id = Guid.NewGuid().ToString(),
                        Properties = CreateTransitionProperties(ctx, transitionNode),
                    },
                    StartNode _ => new FSMRuntimeStartNode { Id = Guid.NewGuid().ToString() },
                    _ => null,
                };

                if (runtimeNode != null)
                {
                    runtimeAsset.Nodes.Add(runtimeNode);
                    nodeMap[node] = runtimeNode;
                }
            }

            static TransitionProperties CreateTransitionProperties(AssetImportContext context, TransitionNode transitionNode)
            {
                var props = ScriptableObject.CreateInstance<TransitionProperties>();
                props.ButtonDown = TryGetPortValue<string>(transitionNode, TransitionNode.ButtonDownPortName);
                props.IsGrounded = TryGetPortValue<bool>(transitionNode, TransitionNode.IsGroundedPortName);
                props.IsFall = TryGetPortValue<bool>(transitionNode, TransitionNode.IsFallPortName);
                props.UseIsWall = TryGetPortValue<bool>(transitionNode, TransitionNode.UseIsWallPortName);
                props.IsWall = TryGetPortValue<bool>(transitionNode, TransitionNode.IsWallPortName);
                props.CanJump = TryGetPortValue<bool>(transitionNode, TransitionNode.CanJumpPortName);
                props.CanDoubleJump = TryGetPortValue<bool>(transitionNode, TransitionNode.CanDoubleJumpPortName);
                props.IsEnemyStepped = TryGetPortValue<bool>(transitionNode, TransitionNode.IsEnemySteppedPortName);
                props.IsOppositionMove = TryGetPortValue<bool>(transitionNode, TransitionNode.IsOppositionMovePortName);
                props.CanDash = TryGetPortValue<bool>(transitionNode, TransitionNode.CanDashPortName);
                props.DashFinished = TryGetPortValue<bool>(transitionNode, TransitionNode.DashFinishedPortName);
                props.HorizontalInput = TryGetPortValue<float>(transitionNode, TransitionNode.HorizontalInputPortName);
                props.HorizontalInputOperator = TryGetPortValue<ComparisonOperator>(transitionNode, TransitionNode.HorizontalInputOperatorPortName);
                context.AddObjectToAsset($"TransitionProperties_{props.GetInstanceID()}", props);
                return props;
            }

            static T TryGetPortValue<T>(INode node, string portName)
            {
                var port = node.GetInputPortByName(portName);
                if (port != null && port.TryGetValue(out T value))
                    return value;
                return default;
            }

            static T TryGetOptionValue<T>(INode node, string optionName)
            {
                var stateNode = node as StateNode;
                var option = stateNode.GetNodeOptionByName(optionName);
                if (option.TryGetValue(out T value))
                    return value;
                return default;
            }

            foreach (var node in graph.GetNodes())
            {
                var outputPort = node.GetOutputPortByName(FSMNode.EXECUTION_PORT_DEFAULT_NAME);
                if (outputPort == null)
                    continue;

                var connectedPorts = new List<IPort>();
                outputPort.GetConnectedPorts(connectedPorts);

                foreach (var connectedPort in connectedPorts)
                {
                    var toNode = connectedPort.GetNode();
                    if (nodeMap.TryGetValue(node, out var fromRuntimeNode) && nodeMap.TryGetValue(toNode, out var toRuntimeNode))
                    {
                        runtimeAsset.Connections.Add(new FSMRuntimeConnection
                        {
                            FromNodeId = fromRuntimeNode.Id,
                            ToNodeId = toRuntimeNode.Id,
                        });
                    }
                }
            }

            ctx.AddObjectToAsset("RuntimeAsset", runtimeAsset);
            ctx.SetMainObject(runtimeAsset);
        }
    }
}