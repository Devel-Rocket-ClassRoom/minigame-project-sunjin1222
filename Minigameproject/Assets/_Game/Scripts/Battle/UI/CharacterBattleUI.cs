using TMPro;
using UnityEngine;

public class CharacterBattleUI : MonoBehaviour
{
    [SerializeField] private CharacterData targetCharacter;
    [SerializeField] private TextMeshProUGUI evidenceText;

    public bool Matches(CharacterData character)
    {
        return targetCharacter != null && targetCharacter == character;
    }

    public void Bind(BattleController battleController)
    {
        if (battleController == null)
            return;

        if (evidenceText != null)
            battleController.SetEvidenceText(evidenceText);
    }
}
