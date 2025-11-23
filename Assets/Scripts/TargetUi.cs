using UnityEngine;
using UnityEngine.InputSystem; // 【重要】これを追加

public class TargetingUI : MonoBehaviour
{
    [SerializeField] private GameObject reticleImage; // 照準の画像オブジェクト

    private void Start()
    {
        // 最初は隠しておく
        Hide();
    }

    private void Update()
    {
        // 照準が表示されているなら、マウスの位置に移動させる
        if (reticleImage.activeSelf)
        {
            // 【修正】新しいInput Systemでのマウス座標取得
            if (Mouse.current != null)
            {
                reticleImage.transform.position = Mouse.current.position.ReadValue();
            }
        }
    }

    public void Show()
    {
        reticleImage.SetActive(true);
        
        // 表示した瞬間に位置合わせ
        if (Mouse.current != null)
        {
            reticleImage.transform.position = Mouse.current.position.ReadValue();
        }
    }

    public void Hide()
    {
        reticleImage.SetActive(false);
    }
}