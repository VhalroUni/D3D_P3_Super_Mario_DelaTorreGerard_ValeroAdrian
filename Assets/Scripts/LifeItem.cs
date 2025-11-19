using UnityEngine;

public class LifeItem : Item
{
    public int m_LifeCount;
    public override void Pick()
    {
        base.Pick();
        GameManager.GetGameManager().GetPLayer().AddLife(m_LifeCount);
    }
    public override bool CanPick()
    {
        PlayerController m_Player = GameManager.GetGameManager().GetPLayer();
        if (m_Player.m_Life >= 100) //Max de vida
            return false;
        else
            return true;
    }
}
