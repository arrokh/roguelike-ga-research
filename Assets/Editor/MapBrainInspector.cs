/*
	Made by Sunny Valle Studio
	(https://svstudio.itch.io)
*/
using UnityEditor;
using UnityEngine;

namespace SVS.ChessMaze
{
	[CustomEditor(typeof(MapBrain))]
	public class MapBrainInspector : Editor
	{
		MapBrain mapBrain;

		private void OnEnable()
		{
			mapBrain = (MapBrain)target;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (Application.isPlaying)
			{
				GUI.enabled = !mapBrain.IsAlgorithmRunning;
				if(GUILayout.Button("Run Genetic Algorithm"))
				{
					mapBrain.RunAlgorithm();
				}
			}
		}
	}
}

