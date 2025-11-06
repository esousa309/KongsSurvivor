#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;

public static class ConvertSceneTo3D
{
    [MenuItem("Tools/Convert Scene To 3D Player Setup")]
    public static void Convert()
    {
        // 1) Find or create Player
        GameObject player = FindPlayer();
        if (player == null)
        {
            player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Kong";
            player.transform.position = new Vector3(0, 1, 0);
            Debug.Log("No Player found. Created 'Kong' Capsule.");
        }

        // Ensure 3D components on Player
        ConvertPlayerTo3D(player);

        // 2) Ensure Ground exists with 3D collider
        EnsureGround();

        // 3) Ensure Main Camera exists and follows Player
        EnsureCameraFollow(player);

        // 4) Set gravity sane default (scene-level)
        Physics.gravity = new Vector3(0, -9.81f, 0);

        Debug.Log("✅ Scene converted to 3D setup. Press Play to test: WASD = move, Shift = sprint, Space = jump.");
    }

    private static GameObject FindPlayer()
    {
        // Try common names/tags
        GameObject byTag = GameObject.FindGameObjectWithTag("Player");
        if (byTag) return byTag;

        string[] names = { "Kong", "Player", "Hero", "Character" };
        foreach (string n in names)
        {
            GameObject found = GameObject.Find(n);
            if (found) return found;
        }

        // Fallback: look for any object with a 2D or 3D rigidbody
        var anyRB = Object.FindObjectsByType<Rigidbody>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
        if (anyRB) return anyRB.gameObject;

        var anyRB2D = Object.FindObjectsByType<Rigidbody2D>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
        if (anyRB2D) return anyRB2D.gameObject;

        return null;
    }

    private static void ConvertPlayerTo3D(GameObject player)
    {
        // Remove 2D components if present
        foreach (var rb2d in player.GetComponents<Rigidbody2D>()) Object.DestroyImmediate(rb2d, true);
        foreach (var col2d in player.GetComponents<Collider2D>()) Object.DestroyImmediate(col2d, true);

        // Ensure a 3D collider (Capsule best for a character)
        if (!player.TryGetComponent<Collider>(out var _))
        {
            player.AddComponent<CapsuleCollider>();
        }

        // Ensure a 3D rigidbody
        if (!player.TryGetComponent<Rigidbody>(out var rb))
        {
            rb = player.AddComponent<Rigidbody>();
        }
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // Ensure PlayerController script exists/attached
        var pc = player.GetComponent<PlayerController>();
        if (pc == null) pc = player.AddComponent<PlayerController>();

        // Try to assign cameraTransform automatically
        var cam = Camera.main != null ? Camera.main.transform : null;
        if (cam != null) pc.cameraTransform = cam;

        // Make sure player is a bit above ground
        Vector3 pos = player.transform.position;
        if (pos.y < 0.5f) player.transform.position = new Vector3(pos.x, 1f, pos.z);
    }

    private static void EnsureGround()
    {
        var ground = GameObject.Find("Ground");
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            Debug.Log("Created Ground (Plane) at (0,0,0).");
        }
        // Ensure it has a 3D collider (Plane primitive already does)
        if (!ground.GetComponent<Collider>()) ground.AddComponent<BoxCollider>();
    }

    private static void EnsureCameraFollow(GameObject player)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            var camGO = new GameObject("Main Camera");
            cam = camGO.AddComponent<Camera>();
            cam.tag = "MainCamera";
        }

        var follow = cam.GetComponent<CameraFollow>();
        if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();
        follow.target = player.transform;

        // Reasonable default offset to see jumping clearly
        if (follow.offset == Vector3.zero)
            follow.offset = new Vector3(0f, 6f, -8f);
    }
}
#endif
