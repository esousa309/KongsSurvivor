
using UnityEngine;
using System.Collections;

public class EndOfLevelRewardWatcher : MonoBehaviour
{
    GameManager gm;
    LevelUpUI ui;
    bool wired;
    int lastPlanet = -999;
    int lastSub = -999;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (FindObjectOfType<EndOfLevelRewardWatcher>() == null)
            new GameObject("EndOfLevelRewardWatcher", typeof(EndOfLevelRewardWatcher));
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        ui = FindObjectOfType<LevelUpUI>();
        if (ui == null)
        {
            ui = new GameObject("LevelUpUI").AddComponent<LevelUpUI>();
            ui.Build();
            ui.Hide();
        }
    }

    void OnEnable() { TryWire(); }
    void OnDisable() { TryUnwire(); }
    void OnDestroy() { TryUnwire(); }

    void Update()
    {
        if (!wired) TryWire();
    }

    void TryWire()
    {
        var newGm = FindObjectOfType<GameManager>();
        if (newGm == null) return;

        if (gm != newGm)
        {
            TryUnwire();
            gm = newGm;
            gm.OnLevelWon += HandleLevelWon;
            wired = true;
        }
    }

    void TryUnwire()
    {
        if (gm != null)
        {
            gm.OnLevelWon -= HandleLevelWon;
        }
        wired = false;
    }

    void HandleLevelWon()
    {
        int planet = gm != null ? gm.PlanetIndex : -1;
        int sub    = gm != null ? gm.SubLevel   : -1;

        if (planet == lastPlanet && sub == lastSub) return;

        lastPlanet = planet;
        lastSub    = sub;

        StartCoroutine(ShowAfterFrame());
    }

    System.Collections.IEnumerator ShowAfterFrame()
    {
        yield return null;

        if (ui != null)
        {
            if (ui.player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) ui.player = p;
            }
            if (Time.timeScale != 0f)
            {
                ui.WireHandlers();
                ui.Show();
            }
        }
    }
}
