using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MeshType {
  Cube,
  Keystone,
};

public enum CurveDirection {
  Right,
  Down,
  Left,
  Up,
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
  public CurveDirection tunnelCurveDirection = CurveDirection.Right;

  CameraMove cameraMove;

  Vector3[] curveAxes = new Vector3[] {
    Vector3.up,
    Vector3.right,
    Vector3.down,
    Vector3.left,
  };

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

  Mesh CreateKeystone(int tLength, float iRadius) {
    Mesh mesh = new Mesh();
    List<Vector3> vertices = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> triangles = new List<int>();

    Quaternion rotation = Quaternion.AngleAxis(90f / tLength, Vector3.up);

    // Points on a quad relative to the origin point.
    Vector3 pBackInner = Vector3.left * iRadius;
    Vector3 pBackOuter = Vector3.left * (iRadius + 1);
    Vector3 pForwardOuter = rotation * pBackOuter;
    Vector3 pForwardInner = rotation * pBackInner;

    // Arrange the points clockwise starting at back left.
    Vector3 point1 = pBackOuter;
    Vector3 point2 = pForwardOuter;
    Vector3 point3 = pForwardInner;
    Vector3 point4 = pBackInner;

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

  public GameObject CreateMesh(MeshType type, int tLength, float iRadius) {
    Mesh mesh;
    if (type == MeshType.Cube) {
      mesh = CreateCube();
    }
    else {
      mesh = CreateKeystone(tLength, iRadius);
    }

    GameObject gameObject = new GameObject(type.ToString(),
      typeof(MeshFilter), typeof(MeshRenderer), typeof(SerializeMesh));
    gameObject.GetComponent<MeshFilter>().mesh = mesh;
    gameObject.GetComponent<MeshRenderer>().material = material;
    gameObject.GetComponent<SerializeMesh>().Serialize();

    return gameObject;
  }

  public GameObject CreateMesh() {
    return CreateMesh(meshType, tunnelLength, tunnelRadius);
  }

  public Transform AddPoint(Tunnel tunnel, string name, Vector3 position, Quaternion rotation) {
    GameObject point = new GameObject(name);
    point.transform.position = position;
    point.transform.rotation = rotation;
    point.transform.parent = tunnel.transform;
    return point.transform;
  }

  public Tunnel CreateStraightTunnel(int width, int length) {
    Tunnel tunnel = new GameObject($"Straight Tunnel {length}", typeof(Tunnel))
      .GetComponent<Tunnel>();
    tunnel.transform.parent = transform;
    tunnel.width = width;
    tunnel.length = length;
    tunnel.radius = 0;

    tunnel.start = AddPoint(tunnel, "Start", Vector3.zero, Quaternion.identity);
    tunnel.end = AddPoint(tunnel, "End", Vector3.forward * length, Quaternion.identity);

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

    creationPoint.position += creationPoint.rotation * Vector3.forward * length;

    return tunnel;
  }

  public Tunnel CreateCurvedTunnel(int width, int length, float radius, CurveDirection curveDir) {
    Tunnel tunnel = new GameObject($"Curved Tunnel {length} {radius} {curveDir}", typeof(Tunnel))
      .GetComponent<Tunnel>();
    tunnel.transform.parent = transform;
    tunnel.width = width;
    tunnel.length = length;
    tunnel.radius = radius;

    tunnel.origin = AddPoint(tunnel, "Origin", Vector3.zero, Quaternion.identity);
    tunnel.start = AddPoint(tunnel, "Start", Vector3.left * radius, Quaternion.identity);
    tunnel.end = AddPoint(tunnel, "End", Vector3.forward * radius,
      Quaternion.AngleAxis(90, Vector3.up));

    float halfWidth = width / 2f;
    float innerWall = radius - halfWidth - 1;
    float outerWall = radius + halfWidth;

    for (float r = innerWall; r <= outerWall; r++) {
      if (r == innerWall || r == outerWall) {
        // Create walls
        GameObject keystone = CreateMesh(MeshType.Keystone, length, r);
        keystone.transform.position = Vector3.down * halfWidth;
        keystone.transform.parent = tunnel.transform;

        for (int z = 0; z < length; z++) {
          Quaternion rotation = Quaternion.AngleAxis(90f / length * z, Vector3.up);

          for (int y = 0; y < width; y++) {
            if (z > 0 || y > 0) {
              Instantiate(keystone, Vector3.up * (y - halfWidth), rotation, tunnel.transform);
            }
          }
        }
      }
      else {
        // Create floor/ceiling
        GameObject floor = CreateMesh(MeshType.Keystone, length, r);
        floor.transform.position = Vector3.down * (halfWidth + 1);
        floor.transform.parent = tunnel.transform;

        GameObject ceiling = Instantiate(
          floor, Vector3.up * halfWidth, Quaternion.identity, tunnel.transform);

        for (int z = 1; z < length; z++) {
          Quaternion rotation = Quaternion.AngleAxis(90f / length * z, Vector3.up);

          Instantiate(floor, floor.transform.position, rotation, tunnel.transform);
          Instantiate(ceiling, ceiling.transform.position, rotation, tunnel.transform);
        }
      }
    }

    Quaternion tunnelRotation = Quaternion.AngleAxis(90 * (int)curveDir, Vector3.back);
    Quaternion cameraRotation = Quaternion.AngleAxis(90, curveAxes[(int)curveDir]);

    tunnel.transform.position = creationPoint.position
      + creationPoint.rotation * Vector3.right * radius;
    tunnel.transform.rotation *= creationPoint.rotation;
    tunnel.transform.RotateAround(creationPoint.position, creationPoint.rotation * Vector3.back,
      90 * (int)curveDir);

    creationPoint.position = tunnel.end.position;
    creationPoint.rotation *= cameraRotation;

    return tunnel;
  }

  public Tunnel CreateTunnel(int width, int length, float radius, CurveDirection curveDir) {
    Tunnel tunnel = radius == 0
      ? CreateStraightTunnel(width, length)
      : CreateCurvedTunnel(width, length, radius, curveDir);

    cameraMove.initialTunnels.Add(tunnel);
    cameraMove.tunnels.Enqueue(tunnel);

    return tunnel;
  }

  public Tunnel CreateTunnel() {
    return CreateTunnel(tunnelWidth, tunnelLength, tunnelRadius, tunnelCurveDirection);
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
