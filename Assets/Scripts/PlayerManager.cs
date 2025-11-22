using UnityEngine;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    [Header("Base Data")]
    [SerializeField] private PlayerBaseData baseData;
    // ゲーム中に変動する「現在の所持デッキ」
    // private set なので、外部からは読み取り専用
    public List<CardData> MasterDeck {get; private set;} = new List<CardData>();

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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
