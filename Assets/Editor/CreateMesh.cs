using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshCreator))]
public class CreateMesh : Editor {
  public float testFloat;

  public override void OnInspectorGUI() {
    DrawDefaultInspector();

    MeshCreator mc = (MeshCreator)target;

    if (GUILayout.Button($"Create {mc.meshType.ToString()} Mesh")) {
      mc.CreateMesh();
    }

    if (GUILayout.Button("Create Tunnel")) {
      mc.CreateTunnel();
    }

    if (GUILayout.Button("Reset")) {
      mc.Reset();
    }

    if (GUILayout.Button("Build Loop")) {
      mc.CreateTunnel(mc.tunnelWidth, mc.tunnelLength, 0);
      mc.CreateTunnel(mc.tunnelWidth, mc.tunnelLength, mc.tunnelRadius);
      mc.CreateTunnel(mc.tunnelWidth, mc.tunnelLength, mc.tunnelRadius);
      mc.CreateTunnel(mc.tunnelWidth, mc.tunnelLength, 0);
      mc.CreateTunnel(mc.tunnelWidth, mc.tunnelLength, 0);
      mc.CreateTunnel(mc.tunnelWidth, mc.tunnelLength, mc.tunnelRadius);
      mc.CreateTunnel(mc.tunnelWidth, mc.tunnelLength, mc.tunnelRadius);
      mc.CreateTunnel(mc.tunnelWidth, mc.tunnelLength, 0);
    }
  }
}
