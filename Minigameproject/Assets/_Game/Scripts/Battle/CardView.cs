using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    public TextMeshProUGUI cardName;
    public Image cardImage;
    public TextMeshProUGUI cardDescription;



    public void Setup(CardData data)
    {
  
        if (data == null)
        {
            Debug.LogError("[CardView] Setup에 null data가 전달되었습니다.");
            return;
        }


        if (cardName != null) cardName.text = data.cardName;
        else Debug.LogWarning("[CardView] cardName 텍스트가 미할당입니다.");

        if (cardDescription != null) cardDescription.text = data.description;
        else Debug.LogWarning("[CardView] cardDescription 텍스트가 미할당입니다.");


        if (cardImage != null)
        {
            if (data.icon != null)
            {
                cardImage.sprite = data.icon;
                cardImage.enabled = true;
            }
            else
            {
                cardImage.sprite = null;
                cardImage.enabled = false;
            }
        }
    }
}
