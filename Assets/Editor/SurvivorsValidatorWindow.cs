#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SurvivorsValidatorWindow : EditorWindow
{
    [MenuItem("Kong Survivors/Validator")]
    public static void ShowWindow()
    {
        var win = GetWindow<SurvivorsValidatorWindow>("Kong Survivors Validator");
        win.minSize = new Vector2(480, 420);
        win.Scan();
    }

    class Issue
    {
        public string Title;
        public System.Action Fix;
        public bool HasFix => Fix != null;
    }

    Vector2 scroll;
    List<Issue> issues = new List<Issue>();
    GUIStyle warnStyle, okStyle, headStyle;

    void OnGUI()
    {
        if (headStyle == null)
        {
            headStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };
            okStyle = new GUIStyle(EditorStyles.label); okStyle.normal.textColor = new Color(0.2f, 0.6f, 0.2f);
            warnStyle = new GUIStyle(EditorStyles.label); warnStyle.normal.textColor = new Color(0.75f, 0.2f, 0.2f);
        }

        GUILayout.Label("Kong Survivors — Project Validator", headStyle);
        GUILayout.Space(6);
        if (GUILayout.Button("Rescan Project & Scene", GUILayout.Height(28)))
        {
            Scan();
        }
        GUILayout.Space(6);

        scroll = GUILayout.BeginScrollView(scroll);
        if (issues.Count == 0)
        {
            GUILayout.Label("✅ No blocking issues found. You can press Play.", okStyle);
        }
        else
        {
            foreach (var it in issues.ToList())
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label("⚠ " + it.Title, warnStyle);
                if (it.HasFix && GUILayout.Button("Apply Auto-Fix"))
                {
                    try { it.Fix?.Invoke(); }
                    catch (System.Exception ex) { Debug.LogError("Auto-Fix failed: " + ex.Message); }
                    finally { Scan(); }
                }
                GUILayout.EndVertical();
            }
        }
        GUILayout.EndScrollView();

        GUILayout.Space(8);
        EditorGUILayout.HelpBox("Tip: Keep this window docked. Re-run after importing new prefabs or making changes.", MessageType.Info);
    }

    void AddIssue(string title, System.Action fix = null) => issues.Add(new Issue { Title = title, Fix = fix });

    void Scan()
    {
        issues.Clear();

        // --- Scene checks ---
        var player = GameObject.FindGameObjectWithTag("Player");
        var gm = GameObject.FindObjectOfType<GameManager>();
        var spawner = GameObject.FindObjectOfType<EnemySpawner>();
        var cam = Camera.main;

        if (cam == null)
        {
            AddIssue("No Main Camera in scene (Display 1).",
                () =>
                {
                    var go = new GameObject("Main Camera");
                    var c = go.AddComponent<Camera>();
                    c.orthographic = true;
                    go.tag = "MainCamera";
                    go.transform.position = new Vector3(0, 0, -10);
                });
        }
        else
        {
            if (!cam.orthographic) AddIssue("Main Camera is not Orthographic.", () => cam.orthographic = true);
            if (cam.targetDisplay != 0) AddIssue("Main Camera Target Display is not Display 1.", () => cam.targetDisplay = 0);
        }

        if (player == null)
        {
            AddIssue("No GameObject tagged 'Player' found in scene.",
                () =>
                {
                    var p = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    p.name = "Player";
                    p.tag = "Player";
                    var rb = p.AddComponent<Rigidbody2D>(); rb.gravityScale = 0;
                    p.AddComponent<CircleCollider2D>().isTrigger = false;
                    p.AddComponent<PlayerController>();
                    p.AddComponent<LevelSystem>();
                    p.AddComponent<AutoAimWeapon>();
                    Selection.activeObject = p;
                });
        }
        else
        {
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb == null) AddIssue("Player missing Rigidbody2D.", () => { player.AddComponent<Rigidbody2D>().gravityScale = 0; });
            else if (rb.gravityScale != 0) AddIssue("Player Rigidbody2D GravityScale should be 0 for top-down.", () => rb.gravityScale = 0);

            if (player.GetComponent<PlayerController>() == null) AddIssue("Player missing PlayerController.", () => player.AddComponent<PlayerController>());
            if (player.GetComponent<LevelSystem>() == null) AddIssue("Player missing LevelSystem.", () => player.AddComponent<LevelSystem>());
            if (player.GetComponent<AutoAimWeapon>() == null) AddIssue("Player missing AutoAimWeapon.", () => player.AddComponent<AutoAimWeapon>());
        }

        if (gm == null)
        {
            AddIssue("No GameManager in scene.", () =>
            {
                var go = new GameObject("GameManager");
                var g = go.AddComponent<GameManager>();
                go.AddComponent<PlanetProgression>();
                var sp = GameObject.FindObjectOfType<EnemySpawner>();
                g.spawner = sp;
                Selection.activeObject = go;
            });
        }

        if (spawner == null)
        {
            AddIssue("No EnemySpawner in scene.", () =>
            {
                var go = new GameObject("Spawner");
                var s = go.AddComponent<EnemySpawner>();
                if (player != null) s.player = player.transform;
                Selection.activeObject = go;
            });
        }
        else
        {
            // Spawner sanity
            if (spawner.player == null)
            {
                AddIssue("Spawner: Player reference not set. (Spawner will try auto-find by tag at runtime.)",
                    () => { if (player != null) spawner.player = player.transform; });
            }
            if (spawner.enemyPrefab == null) AddIssue("Spawner: Enemy Prefab not assigned.");
            if (spawner.bossPrefab == null) AddIssue("Spawner: Boss Prefab not assigned.");
            if (spawner.xpOrbPrefab == null) AddIssue("Spawner: Xp Orb Prefab not assigned.");
        }

        // --- Prefab checks ---
        var allPrefabs = AssetDatabase.FindAssets("t:Prefab");
        GameObject enemyPrefab = null, bossPrefab = null, projectilePrefab = null, xpOrbPrefab = null;

        foreach (var guid in allPrefabs)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go.GetComponent<Enemy>() != null && enemyPrefab == null) enemyPrefab = go;
            if (go.GetComponent<Enemy>() != null && go.name.ToLower().Contains("boss")) bossPrefab = go;
            if (go.GetComponent<Projectile>() != null) projectilePrefab = go;
            if (go.GetComponent<XpOrb>() != null) xpOrbPrefab = go;
        }

        // Projectile prefab requirements
        if (projectilePrefab == null)
        {
            AddIssue("No Projectile prefab found (needs Projectile + Rigidbody2D(Kinematic) + Collider2D(IsTrigger)).");
        }
        else
        {
            var rb = projectilePrefab.GetComponent<Rigidbody2D>();
            var col = projectilePrefab.GetComponent<Collider2D>();
            if (rb == null || !rb.isKinematic) AddIssue("Projectile prefab: Rigidbody2D should be present and Kinematic.");
            if (col == null || !col.isTrigger) AddIssue("Projectile prefab: Collider2D should be present and set to Is Trigger.");
        }

        // XP orb prefab requirements
        if (xpOrbPrefab == null)
        {
            AddIssue("No XpOrb prefab found (needs XpOrb + Rigidbody2D(Kinematic) + Collider2D(IsTrigger)).");
        }
        else
        {
            var rb = xpOrbPrefab.GetComponent<Rigidbody2D>();
            var col = xpOrbPrefab.GetComponent<Collider2D>();
            if (rb == null || !rb.isKinematic) AddIssue("XpOrb prefab: Rigidbody2D should be present and Kinematic.");
            if (col == null || !col.isTrigger) AddIssue("XpOrb prefab: Collider2D should be present and set to Is Trigger.");
        }

        // AutoAimWeapon linkage (scene player if available)
        if (player != null)
        {
            var weap = player.GetComponent<AutoAimWeapon>();
            if (weap != null && weap.projectilePrefab == null)
            {
                if (projectilePrefab != null)
                {
                    AddIssue("AutoAimWeapon: Projectile Prefab not assigned.",
                        () => weap.projectilePrefab = projectilePrefab.GetComponent<Projectile>());
                }
                else
                {
                    AddIssue("AutoAimWeapon: Projectile Prefab not assigned and no projectile prefab found in Assets.");
                }
            }
        }

        // Physics 2D sanity
        if (!Physics2D.GetIgnoreLayerCollision(0,0) && !Physics2D.queriesHitTriggers)
        {
            // queriesHitTriggers false is fine; projectile uses trigger; just surfaced as info
        }

        // Final hint if anything missing
        if (issues.Count > 0)
        {
            Debug.LogWarning("[Kong Survivors Validator] Found " + issues.Count + " issue(s). See Validator window for details.");
        }
        else
        {
            Debug.Log("[Kong Survivors Validator] All checks passed.");
        }
    }
}
#endif