public class CoinItem : Item
{
    public int m_CoinCount;
    public override void Pick()
    {
        base.Pick();
        GameManager.GetGameManager().GetPLayer().AddCoin(m_CoinCount);
    }
    public override bool CanPick()
    {
        return true;
    }
}