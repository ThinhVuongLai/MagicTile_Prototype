using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TileZigzagSprite : MonoBehaviour
{
    [Header("Sprite gốc để lấy texture (kéo tile.jpg/sprite vào đây)")]
    public Sprite sourceSprite;

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

    private SpriteRenderer sr;
    private Sprite runtimeSprite; // instance riêng, không phải asset gốc

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (sourceSprite == null)
        {
            Debug.LogError("Chưa gán sourceSprite!");
            return;
        }

        // Tạo 1 sprite instance MỚI từ cùng texture/rect của sourceSprite
        runtimeSprite = Sprite.Create(
    sourceSprite.texture,
    sourceSprite.rect,
    new Vector2(0.5f, 0.5f),   // pivot
    sourceSprite.pixelsPerUnit,
    0,                          // extrude
    SpriteMeshType.FullRect,    // <- bắt buộc phải FullRect
    Vector4.zero,               // border (không cần 9-slice ở đây)
    false                       // generateFallbackPhysicsShape
);
        runtimeSprite.name = sourceSprite.name + "_runtime";

        sr.sprite = runtimeSprite;

        Rebuild();
    }

    public void SetPoints(List<Vector3> newPoints)
    {
        points = newPoints;
        Rebuild();
    }

    public void Rebuild()
    {
        if (points == null || points.Count < 2 || runtimeSprite == null) return;

        int nodeCount = points.Count;
        Vector3 halfWidthOffset = new Vector3(width * 0.5f, 0f, 0f);

        Vector2[] vertices = new Vector2[nodeCount * 2];
        for (int i = 0; i < nodeCount; i++)
        {
            Vector3 p = points[i];
            vertices[i * 2] = p - halfWidthOffset;
            vertices[i * 2 + 1] = p + halfWidthOffset;
        }

        int quadCount = nodeCount - 1;
        ushort[] triangles = new ushort[quadCount * 6];
        for (int i = 0; i < quadCount; i++)
        {
            ushort l0 = (ushort)(i * 2), r0 = (ushort)(i * 2 + 1);
            ushort l1 = (ushort)(i * 2 + 2), r1 = (ushort)(i * 2 + 3);
            int t = i * 6;
            triangles[t] = l0; triangles[t + 1] = l1; triangles[t + 2] = r0;
            triangles[t + 3] = l1; triangles[t + 4] = r1; triangles[t + 5] = r0;
        }

        runtimeSprite.OverrideGeometry(vertices, triangles);
    }

    void OnDestroy()
    {
        // Dọn instance runtime để tránh leak memory khi note bị destroy
        if (runtimeSprite != null)
            Destroy(runtimeSprite);
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