
using UnityEngine;

[System.Serializable]
public class PlanetList { public Planet[] planets; }

[System.Serializable]
public class Planet
{
    public string code;
    public string name;
    public int unlockCostOMEGA;
    public int sublevels;
}

public class PlanetProgression : MonoBehaviour
{
    public PlanetList data;

    void Awake()
    {
        Load();
    }

    public void Load()
    {
        var ta = Resources.Load<TextAsset>("Data/planets");
        if (ta != null)
        {
            data = JsonUtility.FromJson<PlanetList>(ta.text);
        }
        else
        {
            data = new PlanetList { planets = new Planet[0] };
            Debug.LogWarning("Missing Resources/Data/planets.json");
        }
    }

    public int PlanetCount => data?.planets != null ? data.planets.Length : 0;

    public int IndexOf(string code)
    {
        if (data?.planets == null) return -1;
        for (int i = 0; i < data.planets.Length; i++)
            if (data.planets[i].code == code) return i;
        return -1;
    }

    public Planet Get(int index)
    {
        if (data?.planets == null || index < 0 || index >= data.planets.Length) return null;
        return data.planets[index];
    }
}
