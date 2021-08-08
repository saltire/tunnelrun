using UnityEngine;

[ExecuteInEditMode]
public class SerializeMesh : MonoBehaviour {
  [SerializeField] Vector3[] vertices;
  [SerializeField] Vector2[] uv;
  [SerializeField] int[] triangles;
  [SerializeField] bool serialized = false;

  void Awake() {
    if (serialized) {
      GetComponent<MeshFilter>().sharedMesh = Rebuild();
    }
  }

  // void Start() {
  //   if (!serialized) {
  //     Serialize();
  //   }
  // }

  public void Serialize() {
    Mesh mesh = GetComponent<MeshFilter>().sharedMesh;

    vertices = mesh.vertices;
    uv = mesh.uv;
    triangles = mesh.triangles;
    serialized = true;
  }

  public Mesh Rebuild() {
    Mesh mesh = new Mesh();
    mesh.vertices = vertices;
    mesh.uv = uv;
    mesh.triangles = triangles;

    mesh.RecalculateNormals();
    // mesh.RecalculateBounds();

    return mesh;
  }
}
