using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class NoteZigzagMesh : MonoBehaviour
{
    [Header("Danh sách điểm (local space), điểm đầu -> điểm cuối")]
    public List<Vector3> points = new List<Vector3>
    {
        new Vector3(0, 0, 0),
        new Vector3(0.5f, 1.5f, 0),
        new Vector3(-0.3f, 3f, 0),
        new Vector3(0, 4.5f, 0),
    };

    [Header("Độ rộng note (luôn nằm ngang)")]
    public float width = 1f;

    [Header("Kích thước cap tính theo trục Y (world units)")]
    public float capStartLength = 0.3f;
    public float capEndLength = 0.3f;

    [Header("4 mốc UV (V), tăng dần 0->1")]
    public float uvP0 = 0f;
    public float uvP1 = 0.15f;
    public float uvP2 = 0.85f;
    public float uvP3 = 1f;

    private Mesh mesh;
    private MeshFilter mf;

    private struct Node
    {
        public Vector3 pos;
        public float uv;
        public Node(Vector3 p, float u) { pos = p; uv = u; }
    }

    void Awake()
    {
        mf = GetComponent<MeshFilter>();
        mesh = new Mesh { name = "NoteZigzag" };
        mf.mesh = mesh;
        Rebuild();
    }

    public void SetPoints(List<Vector3> newPoints)
    {
        points = newPoints;
        Rebuild();
    }

    public void Rebuild()
    {
        if (points == null || points.Count < 2) return;

        float y0 = points[0].y;
        float yN = points[points.Count - 1].y;
        float totalH = yN - y0;
        if (Mathf.Abs(totalH) < 0.0001f) return;

        float sign = Mathf.Sign(totalH);
        float pathAbsHeight = Mathf.Abs(totalH);

        float capStart = capStartLength;
        float capEnd = capEndLength;
        float capTotal = capStart + capEnd;
        if (capTotal > pathAbsHeight)
        {
            float scale = pathAbsHeight / capTotal;
            capStart *= scale;
            capEnd *= scale;
        }

        // Y đánh dấu ranh giới (theo hướng sign)
        float yCapStartEnd = y0 + sign * capStart;   // hết cap đầu
        float yCapEndStart = yN - sign * capEnd;     // bắt đầu cap cuối

        // Hàm tính UV.V theo giá trị y bất kỳ dọc path
        float ComputeUV(float y)
        {
            float sy = y * sign;
            float sy0 = y0 * sign;
            float syCapStartEnd = yCapStartEnd * sign;
            float syCapEndStart = yCapEndStart * sign;
            float syN = yN * sign;

            if (sy <= syCapStartEnd)
            {
                float t = (syCapStartEnd > sy0) ? Mathf.InverseLerp(sy0, syCapStartEnd, sy) : 0f;
                return Mathf.Lerp(uvP0, uvP1, t);
            }
            else if (sy <= syCapEndStart)
            {
                float t = (syCapEndStart > syCapStartEnd) ? Mathf.InverseLerp(syCapStartEnd, syCapEndStart, sy) : 0f;
                return Mathf.Lerp(uvP1, uvP2, t);
            }
            else
            {
                float t = (syN > syCapEndStart) ? Mathf.InverseLerp(syCapEndStart, syN, sy) : 1f;
                return Mathf.Lerp(uvP2, uvP3, t);
            }
        }

        // Xây danh sách node: điểm gốc + điểm chèn tại ranh giới cap
        List<Node> nodes = new List<Node>();
        nodes.Add(new Node(points[0], uvP0));

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 A = points[i];
            Vector3 B = points[i + 1];

            float sA = A.y * sign, sB = B.y * sign;

            // Các ranh giới có thể rơi vào đoạn này
            List<float> breakYs = new List<float>();
            float syCapStartEnd = yCapStartEnd * sign;
            float syCapEndStart = yCapEndStart * sign;

            if (syCapStartEnd > sA && syCapStartEnd < sB) breakYs.Add(yCapStartEnd);
            if (syCapEndStart > sA && syCapEndStart < sB) breakYs.Add(yCapEndStart);

            // Sắp xếp theo thứ tự đi qua trong đoạn (theo signed y)
            breakYs.Sort((a, b) => (a * sign).CompareTo(b * sign));

            foreach (float breakY in breakYs)
            {
                float t = Mathf.InverseLerp(A.y, B.y, breakY);
                Vector3 pos = Vector3.Lerp(A, B, t);
                nodes.Add(new Node(pos, ComputeUV(breakY)));
            }

            // Thêm điểm B (trừ khi là điểm cuối cùng thì gán uvP3 chính xác)
            bool isLast = (i == points.Count - 2);
            float uvB = isLast ? uvP3 : ComputeUV(B.y);
            nodes.Add(new Node(B, uvB));
        }

        // Build mesh từ danh sách node
        int nodeCount = nodes.Count;
        Vector3 halfWidthOffset = new Vector3(width * 0.5f, 0f, 0f);

        Vector3[] vertices = new Vector3[nodeCount * 2];
        Vector2[] uv = new Vector2[nodeCount * 2];

        for (int i = 0; i < nodeCount; i++)
        {
            vertices[i * 2] = nodes[i].pos - halfWidthOffset;     // trái
            vertices[i * 2 + 1] = nodes[i].pos + halfWidthOffset; // phải
            uv[i * 2] = new Vector2(0, nodes[i].uv);
            uv[i * 2 + 1] = new Vector2(1, nodes[i].uv);
        }

        int quadCount = nodeCount - 1;
        int[] triangles = new int[quadCount * 6];
        for (int i = 0; i < quadCount; i++)
        {
            int l0 = i * 2, r0 = i * 2 + 1, l1 = i * 2 + 2, r1 = i * 2 + 3;
            int t = i * 6;
            triangles[t] = l0; triangles[t + 1] = l1; triangles[t + 2] = r0;
            triangles[t + 3] = l1; triangles[t + 4] = r1; triangles[t + 5] = r0;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (points == null) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < points.Count - 1; i++)
            Gizmos.DrawLine(transform.TransformPoint(points[i]), transform.TransformPoint(points[i + 1]));
        Gizmos.color = Color.cyan;
        foreach (var p in points) Gizmos.DrawSphere(transform.TransformPoint(p), 0.05f);
    }
#endif
}