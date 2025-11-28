using UnityEngine;
using static CoinsController;

public class CoinsController
{
    int m_Coins = 0;
    public delegate void OnCoinsChangedFn(CoinsController _CoinsController);
    public event OnCoinsChangedFn m_OnCoinsChanged;

    public CoinsController()    
    {
        DependencyInjector.AddDependency<CoinsController>(this);
    }
    public void AddCoins(int Coins)
    {
        m_Coins += Coins;
        m_OnCoinsChanged.Invoke(this);
    }
    public int GetValeu() { return m_Coins; }
}
