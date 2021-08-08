using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MeshType {
  Cube,
  Keystone,
};

[ExecuteInEditMode]
public class MeshCreator : MonoBehaviour {
  public Transform creationPoint;
  public GameObject cubePrefab;
  public Material material;
  public MeshType meshType;

  public int tunnelWidth = 4;
  public int tunnelLength = 5;
  public float tunnelRadius = 4;

  CameraMove cameraMove;

  void Awake() {
    cameraMove = FindObjectOfType<CameraMove>();
  }

  void AddQuad(
    List<Vector3> vertices, List<Vector2> uvs, List<int> triangles,
    Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4
  ) {
    int v = vertices.Count;

    vertices.Add(point1);
    vertices.Add(point2);
    vertices.Add(point3);
    vertices.Add(point4);

    uvs.Add(new Vector2(0, 0));
    uvs.Add(new Vector2(0, 1));
    uvs.Add(new Vector2(1, 1));
    uvs.Add(new Vector2(1, 0));

    triangles.Add(v);
    triangles.Add(v + 1);
    triangles.Add(v + 2);
    triangles.Add(v);
    triangles.Add(v + 2);
    triangles.Add(v + 3);
  }

  void AddPgram(
    List<Vector3> vertices, List<Vector2> uvs, List<int> triangles,
    Vector3 origin, Vector3 axis1, Vector3 axis2
  ) {
    AddQuad(vertices, uvs, triangles,
      origin, origin + axis1, origin + axis1 + axis2, origin + axis2);
  }

  Mesh CreateCube() {
    Mesh mesh = new Mesh();
    List<Vector3> vertices = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> triangles = new List<int>();

    AddPgram(vertices, uvs, triangles, Vector3.forward, Vector3.back, Vector3.right); // Bottom
    AddPgram(vertices, uvs, triangles, Vector3.up, Vector3.forward, Vector3.right); // Top
    AddPgram(vertices, uvs, triangles, Vector3.zero, Vector3.up, Vector3.right); // Front
    AddPgram(vertices, uvs, triangles, Vector3.right, Vector3.up, Vector3.forward); // Right
    AddPgram(vertices, uvs, triangles, Vector3.right + Vector3.forward, Vector3.up, Vector3.left); // Back
    AddPgram(vertices, uvs, triangles, Vector3.forward, Vector3.up, Vector3.back); // Left

    mesh.SetVertices(vertices);
    mesh.SetUVs(0, uvs);
    mesh.SetTriangles(triangles, 0);
    mesh.RecalculateNormals();

    return mesh;
  }

  Mesh CreateKeystone(Vector3 origin, int tLength, float iRadius) {
    Mesh mesh = new Mesh();
    List<Vector3> vertices = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> triangles = new List<int>();

    bool curveRight = iRadius < 0;

    Quaternion rotation = Quaternion.AngleAxis(90f / tLength,
      curveRight ? Vector3.up : Vector3.down);

    // Points on a quad relative to the origin point.
    Vector3 pBackInner = Vector3.right * iRadius;
    Vector3 pBackOuter = Vector3.right * (iRadius + Mathf.Sign(iRadius));
    Vector3 pForwardOuter = rotation * pBackOuter;
    Vector3 pForwardInner = rotation * pBackInner;

    // Arrange the points clockwise starting at back left.
    Vector3 point1 = curveRight ? pBackOuter : pBackInner;
    Vector3 point2 = curveRight ? pForwardOuter : pForwardInner;
    Vector3 point3 = curveRight ? pForwardInner : pForwardOuter;
    Vector3 point4 = curveRight ? pBackInner : pBackOuter;

    Vector3 point1a = point1 + Vector3.up;
    Vector3 point2a = point2 + Vector3.up;
    Vector3 point3a = point3 + Vector3.up;
    Vector3 point4a = point4 + Vector3.up;

    AddQuad(vertices, uvs, triangles, point2, point1, point4, point3); // Bottom
    AddQuad(vertices, uvs, triangles, point1a, point2a, point3a, point4a); // Top
    AddQuad(vertices, uvs, triangles, point1, point1a, point4a, point4); // Front
    AddQuad(vertices, uvs, triangles, point4, point4a, point3a, point3); // Right
    AddQuad(vertices, uvs, triangles, point3, point3a, point2a, point2); // Back
    AddQuad(vertices, uvs, triangles, point2, point2a, point1a, point1); // Left

    mesh.SetVertices(vertices);
    mesh.SetUVs(0, uvs);
    mesh.SetTriangles(triangles, 0);
    mesh.RecalculateNormals();

    return mesh;
  }

  public GameObject CreateMesh(MeshType type, Vector3 origin, int tLength, float iRadius) {
    Mesh mesh;
    if (type == MeshType.Cube) {
      mesh = CreateCube();
    }
    else {
      mesh = CreateKeystone(origin, tLength, iRadius);
    }

    GameObject gameObject = new GameObject(type.ToString(),
      typeof(MeshFilter), typeof(MeshRenderer), typeof(SerializeMesh));
    gameObject.GetComponent<MeshFilter>().mesh = mesh;
    gameObject.GetComponent<MeshRenderer>().material = material;
    gameObject.GetComponent<SerializeMesh>().Serialize();

    return gameObject;
  }

  public GameObject CreateMesh() {
    return CreateMesh(meshType, Vector3.zero, tunnelLength, tunnelRadius);
  }

  public Tunnel CreateStraightTunnel(int width, int length) {
    Tunnel tunnel = new GameObject($"Straight Tunnel {length}", typeof(Tunnel))
      .GetComponent<Tunnel>();
    tunnel.transform.parent = transform;
    tunnel.width = width;
    tunnel.length = length;
    tunnel.radius = 0;

    float halfWidth = width / 2f;

    for (int z = 0; z < length; z++) {
      for (float x = -halfWidth; x < halfWidth; x++) {
        Instantiate(cubePrefab,
          new Vector3(x, -halfWidth - 1, z), Quaternion.identity, tunnel.transform);
        Instantiate(cubePrefab,
          new Vector3(x, halfWidth, z), Quaternion.identity, tunnel.transform);
        Instantiate(cubePrefab,
          new Vector3(-halfWidth - 1, x, z), Quaternion.identity, tunnel.transform);
        Instantiate(cubePrefab,
          new Vector3(halfWidth, x, z), Quaternion.identity, tunnel.transform);
      }
    }

    tunnel.transform.position = creationPoint.position;
    tunnel.transform.rotation = creationPoint.rotation;

    tunnel.start = creationPoint.position;
    creationPoint.position += creationPoint.rotation * Vector3.forward * length;
    tunnel.end = creationPoint.position;

    return tunnel;
  }

  public Tunnel CreateCurvedTunnel(int width, int length, float radius) {
    Tunnel tunnel = new GameObject($"Curved Tunnel {length} {radius}", typeof(Tunnel))
      .GetComponent<Tunnel>();
    tunnel.transform.parent = transform;
    tunnel.width = width;
    tunnel.length = length;
    tunnel.radius = radius;

    Vector3 origin = Vector3.right * radius;
    float halfWidth = width / 2f;
    float innerWall = Mathf.Abs(radius) - halfWidth - 1;
    float outerWall = Mathf.Abs(radius) + halfWidth;

    for (float ar = innerWall; ar <= outerWall; ar++) {
      float r = ar * Mathf.Sign(radius);

      if (ar == innerWall || ar == outerWall) {
        // Create walls
        GameObject keystone = CreateMesh(MeshType.Keystone, origin, length, r);
        keystone.transform.position = Vector3.down * halfWidth;
        keystone.transform.parent = tunnel.transform;

        for (int z = 0; z < length; z++) {
          Quaternion rotation = Quaternion.AngleAxis(90f / length * z,
            radius > 0 ? Vector3.down : Vector3.up);

          for (int y = 0; y < width; y++) {
            if (z > 0 || y > 0) {
              Instantiate(keystone, Vector3.up * (y - halfWidth), rotation, tunnel.transform);
            }
          }
        }
      }
      else {
        // Create floor/ceiling
        GameObject floor = CreateMesh(MeshType.Keystone, origin, length, r);
        floor.transform.position = Vector3.down * (halfWidth + 1);
        floor.transform.parent = tunnel.transform;

        GameObject ceiling = Instantiate(
          floor, Vector3.up * halfWidth, Quaternion.identity, tunnel.transform);

        for (int z = 1; z < length; z++) {
          Quaternion rotation = Quaternion.AngleAxis(90f / length * z,
            radius > 0 ? Vector3.down : Vector3.up);

          Instantiate(floor, floor.transform.position, rotation, tunnel.transform);
          Instantiate(ceiling, ceiling.transform.position, rotation, tunnel.transform);
        }
      }
    }

    tunnel.origin = creationPoint.position - creationPoint.rotation * origin;
    tunnel.transform.position = tunnel.origin;
    tunnel.transform.rotation *= creationPoint.rotation;

    tunnel.start = creationPoint.position;
    creationPoint.position += creationPoint.rotation
      * (Vector3.forward * Mathf.Abs(radius) + Vector3.left * radius);
    creationPoint.rotation *= Quaternion.AngleAxis(90, radius > 0 ? Vector3.down : Vector3.up);
    tunnel.end = creationPoint.position;

    return tunnel;
  }

  public Tunnel CreateTunnel(int width, int length, float radius) {
    Tunnel tunnel = radius == 0
      ? CreateStraightTunnel(width, length)
      : CreateCurvedTunnel(width, length, radius);

    cameraMove.initialTunnels.Add(tunnel);
    cameraMove.tunnels.Enqueue(tunnel);

    return tunnel;
  }

  public Tunnel CreateTunnel() {
    return CreateTunnel(tunnelWidth, tunnelLength, tunnelRadius);
  }

  public void Reset() {
    creationPoint.transform.position = transform.position;
    creationPoint.transform.rotation = transform.rotation;

    cameraMove.initialTunnels.Clear();
    cameraMove.tunnels.Clear();

    foreach (Tunnel tunnel in FindObjectsOfType<Tunnel>()) {
      DestroyImmediate(tunnel.gameObject);
    }
  }
}
