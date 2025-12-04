using UnityEngine;
using static StarsController;

public class StarsController
{
    int m_Stars = 0;
    public delegate void OnStarsChangedFn(StarsController _StarsController);
    public event OnStarsChangedFn m_OnStarsChanged;

    public StarsController()
    {
        DependencyInjector.AddDependency<StarsController>(this);
    }
    public void AddStars(int Stars)
    {
        m_Stars += Stars;
        m_OnStarsChanged.Invoke(this);
    }
    public int GetValeu() { return m_Stars; }
}
