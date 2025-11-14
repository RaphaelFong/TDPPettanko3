using System.Collections; // if using TextMeshPro
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;


public class GeminiUI : MonoBehaviour
{
    private GeminiClient gemini;
    public TMP_Dropdown toneDropdown;

    private string[] Creatures = {
    // Good Creatures
    "Fairies", "Spirits", "Honey Bear", "Trolls", "Centaur", "Griffin", "Snakes",
    // Neutral Creatures  
    "Gnomes", "Deity", "Dragon", "Golems", "Mermaid", "Phoenix", "Pegasus", "Unicorn", "Giant",
    // Bad Creatures
    "Fay", "Ghoul", "Lich", "Ogre", "Salamander", "ShapeShifter", "Seraphim", "Siren",
    "Kraken", "Charybdis", "Demons", "Devil", "Vampire", "Chimera", "Cerberus", "Hydra",
    "Manticore", "Minotaur", "SandWorms"
    };

    private string[] Magic = {
    // Light Magic
    "Photon", "Purify", "Heal", "Sparks Ray", "Blind", "Reflect", "Refract", "Dispell",
    // Dark Magic
    "ShadowBind", "Shadow Fangs", "Corrupt", "Necromance", "Summoning", "Curses",
    // Wind Magic
    "Gust of Storm", "Swift", "Cyclone",
    // Sound Magic
    "Pierce", "Shrill", "Illusion", "Telepathy",
    // Water Magic
    "Ripple", "Wave", "Acid",
    // Fire Magic
    "Solar Blast", "Inferno",
    // Fauna Magic
    "Weave", "Weathy", "Thorn Bind",
    // Random Magic
    "Lightning Arrow", "Freeze", "Teleport", "Magic Blade"
    };
    
    string instructionLayer;
    string toneLayer;
    string additionalDescription;
    string contextLayer;
    string creatureAndMagicLayer;
    string end;

    void Start()
    {
        toneDropdown.ClearOptions();
        List<string> newOptions = new List<string>();
        newOptions.Add("Epic/Grand Focuses on monumental events, heroism, and the vast sweep of time. Elevated, formal language");
        newOptions.Add("Tragic/Somber Emphasizes loss, decline, fate, and the inevitable suffering of the people. Mournful and reflective");
        newOptions.Add("Mysterious/Cryptic Hints at secrets, unknown forces, and incomplete knowledge. Uses evocative, suggestive language.");
        newOptions.Add("Triumphant/Heroic Celebrates victories, strong leadership, and overcoming adversity. Inspiring and declarative");
        newOptions.Add("Pragmatic/Objective Focuses on facts, consequences, and socio-economic factors. Neutral, academic, and dry");
        newOptions.Add("Fateful/Prophetic Suggests events were predetermined or guided by divine/magical forces. Often uses 'must,' 'shall,' or hints at future events.");
        newOptions.Add("Cautionary/Didactic Aims to teach a moral lesson or warn future generations about past mistakes (e.g., hubris, greed).");
        newOptions.Add("Dark/Ominous Focuses on terror, corruption, and the slow, insidious growth of evil. Creates tension and dread.");

        toneDropdown.ClearOptions();
        toneDropdown.AddOptions(newOptions);


        //toneLayer = "Tone: Epic/Grand Focuses on monumental events, heroism, and the vast sweep of time. Elevated, formal language.\r\n";
        //Debug.Log(toneDropdown.value);
        //toneLayer = toneDropdown.options[toneDropdown.value].text;
        //Debug.Log("toneLayer: " + toneLayer);

        // Find the GeminiClient in the scene (or drag-assign in inspector)
        gemini = FindObjectOfType<GeminiClient>();

        if (gemini == null)
            Debug.LogError("GEMINI CLIENT IS NULL");

        contextLayer = "High fantasy is a fictional genre set in an entirely invented universe with its own geography, " +
            "mythologies, cultures, and supernatural systems. It emphasizes epic storytelling where the outcomes of conflicts shape " +
            "the fate of kingdoms or the entire world. For this project, the tool will generate high-quality historical timelines that describe" +
            " major eras, the rise and fall of civilizations, magical developments, legendary figures, races, and world-defining events. " +
            "This approach ensures that the resulting content represents rich high fantasy worldbuilding, distinct from real-world medieval settings" +
            " and grounded in immersive, original lore.";

        instructionLayer = "Generate a strictly 600 word or less story with each paragraph be in a format of :" +
            "Date to Date : 30 word description of the event happening during this period" +
            "The description should focus on the describing how creatures were discovered, how the kingdom was being built or" +
            "any other key events" + 
            "Phrase the decriptions in a documentary style: ";

        additionalDescription = "Additional info: Do not include voicelines! Tell in a 3rd person narration, " +
            "treat it as though you are recapping an entire world's history. Treat each paragraph as a different period in the Kingdom history " +
            "Add a header or title before each paragraph";

        creatureAndMagicLayer = "You may use any of these listed creatures and/or magic for reference: " +
                 string.Join(", ", Creatures) + ", " +
                 string.Join(", ", Magic) + ".";

        end = "You MUST end each paragraph with a semicolon ;";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            toneLayer = toneDropdown.options[toneDropdown.value].text;

            Debug.Log("Sending Prompt..");
            Debug.Log("View Settings:");
            Debug.Log("toneLayer : " + toneLayer);
            Debug.Log("creatureAndMagicLayer : " + creatureAndMagicLayer);

            gemini.SendPrompt(contextLayer + instructionLayer + toneLayer + additionalDescription + creatureAndMagicLayer + end);
        }
    }

}
