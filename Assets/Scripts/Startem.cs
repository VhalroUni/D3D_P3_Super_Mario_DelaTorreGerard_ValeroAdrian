public class StarItem : Item
{
    public int m_StarCount;
    public override void Pick()
    {
        base.Pick();
        GameManager.GetGameManager().GetPLayer().AddCoin(m_StarCount);
    }
    public override bool CanPick()
    {
        return true;
    }
}