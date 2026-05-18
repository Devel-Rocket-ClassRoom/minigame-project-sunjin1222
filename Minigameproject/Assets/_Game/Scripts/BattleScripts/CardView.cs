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
        cardName.text = data.cardName;
        cardDescription.text = data.description;
        if (data.icon != null)
        {
            cardImage.sprite = data.icon;
        }

    }
}
