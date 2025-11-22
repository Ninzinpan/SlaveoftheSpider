using UnityEngine;
using UnityEngine.UI; // Imageコンポーネント用
using TMPro;          // TextMeshPro用
using UnityEngine.EventSystems; // クリック処理用

public class CardDisplay : MonoBehaviour,IPointerClickHandler
{
    [Header("UI Parts (Drag & Drop here)")]
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;       // 新規追加
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image cardArtImage;             // カードの絵（今回は背景兼メイン画像）

    // 外部からこのカードが何のデータか参照するためのプロパティ
    public CardData CardData { get; private set; }


    /// <summary>
    /// データを注入して表示を更新するメインメソッド
    /// </summary>
    public void Setup(CardData data)
    {
        // 安全装置1: データがない場合はエラーを出して中断
        if (data == null)
        {
            Debug.LogError("CardDisplay: データがnullです");
            return;
        }
        

        // データを保持（後でクリック処理などに使う）
        CardData = data;

        // --- テキストの反映 ---
        
        // 安全装置2: インスペクタでの設定忘れチェック (?. は「nullじゃなければ実行」)
        costText?.SetText(data.Cost.ToString());
        nameText?.SetText(data.CardName);
        typeText?.SetText(data.Type.ToString());
        descriptionText?.SetText(data.Description);



        // --- 画像の反映 ---
        if (cardArtImage != null && data.Icon != null)
        {
            cardArtImage.sprite = data.Icon;
        }
    }

private BattleManager battleManager;
    private void Start()
    {
        try 
        {
            battleManager = FindFirstObjectByType<BattleManager>();
        }
        catch
        {
            Debug.LogError("CardDisplay: BattleManagerが見つかりません！");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // BattleManagerに「このカード (this) が押されたよ」と報告
        if (battleManager != null)
        {
            battleManager.OnCardClicked(this);
            Debug.Log("CardDisplay: カードがクリックされました.");
        }
        else
        {
            Debug.LogError("CardDisplay: BattleManagerがセットされていません！");
        }
    }

}