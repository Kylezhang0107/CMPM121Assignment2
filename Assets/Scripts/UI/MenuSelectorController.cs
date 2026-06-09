using UnityEngine;
using TMPro;

public class MenuSelectorController : MonoBehaviour
{
    private enum SelectorType
    {
        Level,
        CharacterClass
    }

    public TextMeshProUGUI label;
    public string level;
    private string characterClass;
    private SelectorType selectorType = SelectorType.Level;
    public EnemySpawner spawner;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetLevel(string text)
    {
        selectorType = SelectorType.Level;
        level = text;
        label.text = text;
    }

    public void SetCharacterClass(string text)
    {
        selectorType = SelectorType.CharacterClass;
        characterClass = text;
        label.text = text;
    }

    public void StartLevel()
    {
        if (selectorType == SelectorType.CharacterClass)
        {
            spawner.SelectCharacterClass(characterClass);
            return;
        }

        spawner.StartLevel(level);
    }
    
}
