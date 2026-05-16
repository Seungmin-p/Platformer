using System;
using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace FSMGraph
{
    [Serializable]
    [Graph(AssetExtension)]
    internal class FSMGraph : Graph
    {
        const string k_graphName = "FSM Graph";

        internal const string AssetExtension = "fsm";

        [MenuItem("Assets/Create/FSM Graph")]
        static void CreateAssetFile()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<FSMGraph>(k_graphName);
        }

        public override void OnGraphChanged(GraphLogger infos)
        {
            base.OnGraphChanged(infos);
            CheckGraphErrors(infos);
        }

        void CheckGraphErrors(GraphLogger infos)
        {
            // Add validation logic if needed
        }
    }
}