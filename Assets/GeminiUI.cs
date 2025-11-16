using System.Collections; // if using TextMeshPro
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GeminiUI : MonoBehaviour
{
    private GeminiClient gemini;
    public GameObject ParameterPanel;
    public TMP_Dropdown toneDropdown;
    public TMP_Dropdown geographyDropdown;
    public TMP_Dropdown themeDropdown;

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
    string geographyLayer;
    string themeLayer;
    string additionalDescription;
    string contextLayer;
    string creatureAndMagicLayer;
    string end;

    void Start()
    {
        toneDropdown.ClearOptions();
        List<string> toneOptions = new List<string>();
        toneOptions.Add("Choose a Tone for the story");
        toneOptions.Add("Epic/Grand Focuses on monumental events, heroism, and the vast sweep of time. Elevated, formal language.");
        toneOptions.Add("Tragic/Somber Emphasizes loss, decline, fate, and the inevitable suffering of the people. Mournful and reflective.");
        toneOptions.Add("Mysterious/Cryptic Hints at secrets, unknown forces, and incomplete knowledge. Uses evocative, suggestive language.");
        toneOptions.Add("Triumphant/Heroic Celebrates victories, strong leadership, and overcoming adversity. Inspiring and declarative.");
        toneOptions.Add("Pragmatic/Objective Focuses on facts, consequences, and socio-economic factors. Neutral, academic, and dry.");
        toneOptions.Add("Fateful/Prophetic Suggests events were predetermined or guided by divine/magical forces. Often uses 'must,' 'shall,' or hints at future events.");
        toneOptions.Add("Cautionary/Didactic Aims to teach a moral lesson or warn future generations about past mistakes (e.g., hubris, greed).");
        toneOptions.Add("Dark/Ominous Focuses on terror, corruption, and the slow, insidious growth of evil. Creates tension and dread.");
        toneDropdown.AddOptions(toneOptions);


        geographyDropdown.ClearOptions();
        List<string> geographyOptions = new List<string>();
        geographyOptions.Add("Choose a Geographical Setting");
        geographyOptions.Add("Island Nation - Isolated maritime culture with naval traditions and sea-based economy.");
        geographyOptions.Add("Mountain Kingdom - High altitude realm with defensible peaks, mining heritage, and stone fortresses.");
        geographyOptions.Add("Forest Realm - Ancient woodland territory with druidic traditions and harmony with nature.");
        geographyOptions.Add("Desert Empire - Arid landscape with oasis cities, caravan routes, and sun-worshipping culture.");
        geographyOptions.Add("Coastal Trade Hub - Bustling port cities with diverse cultures and maritime commerce.");
        geographyOptions.Add("Underground Cities - Subterranean civilization with crystal caverns and bioluminescent architecture.");
        geographyOptions.Add("Plains/Grasslands - Vast open territories with nomadic tribes or agricultural settlements.");
        geographyOptions.Add("Arctic Tundra - Frozen wilderness with ice fortresses and survival-focused society.");
        geographyDropdown.AddOptions(geographyOptions);


        themeDropdown.ClearOptions();
        List<string> themeOptions = new List<string>();
        themeOptions.Add("Choose a theme");
        themeOptions.Add("Military Conquest - Emphasizes warfare, battles, territorial expansion, and the rise of military powers.");
        themeOptions.Add("Magical Discovery - Focuses on the unveiling of new spells, magical artifacts, arcane research, and mystical breakthroughs.");
        themeOptions.Add("Cultural Renaissance - Highlights artistic flourishing, intellectual advancement, philosophy, and cultural golden ages.");
        themeOptions.Add("Religious Movement - Centers on the rise of faiths, divine interventions, prophets, temples, and spiritual transformations.");
        themeOptions.Add("Exploration & Expansion - Chronicles voyages to unknown lands, colonization, mapping new territories, and discoveries.");
        themeOptions.Add("Survival & Adaptation - Emphasizes overcoming hardships, natural disasters, plagues, famines, and resilience against threats.");
        themeOptions.Add("Political Intrigue - Focuses on court politics, betrayals, diplomatic maneuvering, succession crises, and power struggles.");
        themeDropdown.AddOptions(themeOptions);


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

        instructionLayer = "Generate a strictly 500 to 600 word story with each paragraph be in a format of :" +
            "Date to Date : 30 word description of the event happening during this period" +
            "The description should focus on the describing how creatures were discovered, how the kingdom was being built or" +
            "any other key events" + 
            "Phrase the decriptions in a documentary style: " +
            "You do not need to give a header or name to the story"
            ;

        additionalDescription = "Additional info: Do not include voicelines! Tell in a 3rd person narration, " +
            "treat it as though you are recapping an entire world's history. Treat each paragraph as a different period in the Kingdom history " +
            "Add a header or title before each paragraph";

        creatureAndMagicLayer = "You may use any of these listed creatures and/or magic for reference: " +
                 string.Join(", ", Creatures) + ", " +
                 string.Join(", ", Magic) + ".";

        end = "You MUST end each paragraph with a semicolon ;";
    }

    public void GenerateStory()
    {
        // Dynamically Create image
        Canvas canvas = FindObjectOfType<Canvas>();
        GameObject imageObject = new GameObject("GeneratedImage");
        Image image = imageObject.AddComponent<Image>();
        imageObject.transform.SetParent(canvas.transform, false);
        imageObject.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        imageObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        imageObject.SetActive(false);

        toneLayer = "Use this tone for the story: " + toneDropdown.options[toneDropdown.value].text;
        geographyLayer = "Use this geographical setting for the story: " + geographyDropdown.options[geographyDropdown.value].text;
        themeLayer = "Use this theme for the story: " + themeDropdown.options[themeDropdown.value].text;

        Debug.Log("Sending Prompt..");
        Debug.Log("View Settings:");
        Debug.Log("toneLayer : " + toneLayer);
        Debug.Log("geographyLayer : " + geographyLayer);
        Debug.Log("themeLayer : " + themeLayer);
        Debug.Log("creatureAndMagicLayer : " + creatureAndMagicLayer);

        LoadingScreen.SetServerMessage("Sending Prompt to Gemini..");

        ParameterPanel.SetActive(false);

        gemini.SendPrompt(contextLayer + instructionLayer + toneLayer + geographyLayer + additionalDescription + creatureAndMagicLayer + end);
    }

    public void ResetToHome()
    {
        // Reset all dropdowns to index 0 (the "Choose..." options)
        toneDropdown.value = 0;
        geographyDropdown.value = 0;
        themeDropdown.value = 0;

        // Show the parameter panel again
        if (ParameterPanel != null)
        {
            ParameterPanel.SetActive(true);
        }

        // Call GeminiClient's reset function to clear generated content
        if (gemini != null)
        {
            gemini.ResetAll();
        }

        // Hide loading screen if it's showing
        LoadingScreen.Hide();

        Debug.Log("Returned to home!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateStory();
        }
    }

}
