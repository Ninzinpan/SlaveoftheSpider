using UnityEngine;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    [Header("Base Data")]
    [SerializeField] private PlayerBaseData baseData;
    // ゲーム中に変動する「現在の所持デッキ」
    // private set なので、外部からは読み取り専用
    public List<CardData> MasterDeck {get; private set;} = new List<CardData>();
    public int CurrentEnergy { get; private set; } // 現在のエナジー

    private void Awake()
    {
        if (baseData != null)
        {
            MasterDeck = new List<CardData>(baseData.StartingDeck);
        }
    }
     // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 戦闘開始時、最大値まで回復
        if (baseData != null)
        {
            CurrentEnergy = baseData.BaseEnergy;
            Debug.Log($"エナジー初期化: {CurrentEnergy}");
        }
    }
    public void ConsumeEnergy(int cost)
    {
        CurrentEnergy -= cost;
        if (CurrentEnergy < 0) CurrentEnergy = 0;
        Debug.Log($"エナジー消費: {cost} (残り: {CurrentEnergy})");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
