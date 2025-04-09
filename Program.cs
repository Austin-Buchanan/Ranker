// Present main menu, loading ActionMenu based on user selection
using Ranker;
using System.Xml;

string mainOption = "";
XmlInterface menuXml = new("Menu.xml");
List<string> mainOptions = menuXml.QuickReadXml("MainMenu");
while (mainOption != "Quit")
{
    mainOption = SelectOption(mainOptions, "Welcome to Ranker! What media would you like to use?");
    if (mainOption == "Quit") { break; }
    RunActionMenu(mainOption);
}

void RunActionMenu(string mediaType)
{
    XmlInterface mediaXml = new($"{mediaType}.xml");
    string actionOption = "";
    List<string> actionOptions = menuXml.QuickReadXml("ActionMenu");
    while (actionOption != "Quit")
    {
        actionOption = SelectOption(actionOptions, $"You're working with {mediaType}. What would you like to do?");
        switch (actionOption)
        {
            case "Add":
                AddItems(mediaXml, mediaType);
                break;
            case "View":
                ViewItems(mediaXml);
                break;
            case "Rank":
                RankItems(mediaXml, mediaType, 64);
                break;
            case "Quit":
                break;
        }
    }
}

static string SelectOption(List<string> options, string header)
{
    int currentIndex = 0;
    ConsoleKey key;
    do
    {
        Console.Clear();
        Console.WriteLine(header);
        for (int i = 0; i < options.Count; i++)
        {
            if (i == currentIndex)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ResetColor();
            }
            Console.WriteLine(options[i]);
        }
        Console.ResetColor();

        key = Console.ReadKey(true).Key;

        switch (key)
        {
            case ConsoleKey.UpArrow:
                if (currentIndex > 0) currentIndex--;
                break;
            case ConsoleKey.DownArrow:
                if (currentIndex < options.Count - 1) currentIndex++;
                break;
        }
    } while (key != ConsoleKey.Enter);

    Console.Clear();
    return options[currentIndex];
}

static void AddItems(XmlInterface mediaXml, string mediaType)
{
    List<string> templateParts = mediaXml.QuickReadXml("Template");
    List<string> itemParts = [];
    string? userInput;
    bool doneAdding = false;
    bool itemPresent;
    XmlElement itemElement;
    XmlElement subElement;
    XmlElement itemELO;

    while (!doneAdding)
    {
        mediaXml.Refresh();
        itemELO = mediaXml.CreateElement("ELO", "1000"); 

        do
        {
            if (itemParts.Count > 0) { itemParts.Clear(); }
            Console.WriteLine("Please enter the requested elements.");
            foreach (string templatePart in templateParts)
            {
                do
                {
                    Console.WriteLine($"{templatePart}:");
                    userInput = Console.ReadLine();
                } while (userInput == null || userInput.Length == 0);
                itemParts.Add(userInput);
            }
            itemPresent = mediaXml.IsItemPresent(itemParts, mediaType[..^1]);
            if (itemPresent) { Console.WriteLine("The entered values are already present in the system. Please enter values for a new item."); }
        } while (itemPresent);

        itemElement = mediaXml.CreateElement(mediaType[..^1]);
        for (int i = 0; i < itemParts.Count; i++)
        {
            subElement = mediaXml.CreateElement(templateParts[i], itemParts[i]);
            itemElement.AppendChild(subElement);
        }
        itemElement.AppendChild(itemELO);
        mediaXml.AttachToRoot(itemElement);
        mediaXml.Refresh();

        bool validInput = false;
        string? continueInput;
        while (!validInput)
        {
            Console.WriteLine("Continue? Y/N:");
            continueInput = Console.ReadLine();
            if (continueInput == null || continueInput.Length == 0) continue;
            else if (continueInput.Equals("Y", StringComparison.CurrentCultureIgnoreCase))
            {
                validInput = true;
            }
            else if (continueInput.Equals("N", StringComparison.CurrentCultureIgnoreCase))
            {
                validInput = true;
                doneAdding = true;
            }
            else continue;
        }
        Console.Clear();
    }
}

static void ViewItems(XmlInterface mediaXml)
{
    List<XmlNode> itemNodeList = mediaXml.GetNonTemplateNodes();

    Dictionary<int, int> eloDict = [];
    for (int i = 0; i < itemNodeList.Count; i++)
    {
        eloDict.Add(i, GetElo(itemNodeList[i]));
    }

    string order = SelectOption(["Ascending", "Descending"], "Do you want to view the items with the ranking ascending or descending?");
    IOrderedEnumerable<KeyValuePair<int, int>> sortedDict;
    if (order == "Ascending") { sortedDict = eloDict.OrderBy(kvp => kvp.Value); }
    else { sortedDict = eloDict.OrderByDescending(kvp => kvp.Value); }

    Console.WriteLine("Score | Ranked Element");
    Console.WriteLine("----------------------");

    string itemText;
    foreach (var kvp in sortedDict)
    {
        itemText = "";
        foreach (XmlNode node in itemNodeList[kvp.Key].ChildNodes)
        {
            if (node.Name != "ELO") { itemText += $" {node.InnerText};"; }
        }
        Console.WriteLine($"{kvp.Value}  |{itemText[..^1]}");
    }

    Console.WriteLine("Press any key to continue.");
    Console.ReadKey();
}

static void RankItems(XmlInterface mediaXml, string mediaType, int k)
{
    List<XmlNode> items = mediaXml.GetNonTemplateNodes();

    Random random = new();
    while (true)
    {
        int indexA = random.Next(items.Count);
        int indexB;
        do
        {
            indexB = random.Next(items.Count);
        } while (indexB == indexA);

        int eloA = GetElo(items[indexA]);
        int eloB = GetElo(items[indexB]);
        double expectedScoreA = CalculateExpectedScore(eloA, eloB);
        double expectedScoreB = CalculateExpectedScore(eloB, eloA);
        string itemStrA = XmlNodeToString(items[indexA]);
        string itemStrB = XmlNodeToString(items[indexB]);

        List<string> selectionItems = [itemStrA, itemStrB, "Quit"];
        string selectedItem = SelectOption(selectionItems, $"Which {mediaType[..^1].ToLower()} do you prefer?");
        if (selectedItem == "Quit") break;
        double actualScoreA, actualScoreB;
        if (selectedItem == itemStrA)
        {
            actualScoreA = 1.0;
            actualScoreB = 0.0;
        }
        else
        {
            actualScoreA = 0.0;
            actualScoreB = 1.0;
        }

        SetElo(items[indexA], UpdateRating(eloA, actualScoreA, expectedScoreA, k));
        SetElo(items[indexB], UpdateRating(eloB, actualScoreB, expectedScoreB, k));
    }
    mediaXml.Refresh();
}

static int GetElo(XmlNode node)
{
    foreach (XmlNode subNode in node.ChildNodes)
    {
        if (subNode.Name == "ELO") { return int.Parse(subNode.InnerText); }
    }
    return -1;
}

static void SetElo(XmlNode node, int newElo)
{
    foreach (XmlNode subNode in node.ChildNodes)
    {
        if (subNode.Name == "ELO")
        {
            subNode.InnerText = newElo.ToString();
            break;
        }
    }
}

static string XmlNodeToString(XmlNode node)
{
    string result = "";
    foreach (XmlNode subNode in node.ChildNodes)
    {
        if (subNode.Name != "ELO")
        {
            result += $"{subNode.Name}: {subNode.InnerText}; ";
        }
    }
    return result[..^2];
}

static double CalculateExpectedScore(int ratingA, int ratingB)
{
    return 1 / (1 + Math.Pow(10, (ratingB - ratingA) / 400.0));
}

static int UpdateRating(int rating, double score, double expectedScore, int k)
{
    return rating + (int)(k * (score - expectedScore));
}

