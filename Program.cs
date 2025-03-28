// Present main menu, loading ActionMenu based on user selection
using System.Xml;

string mainOption = "";
List<string> mainOptions = QuickReadXML("Menu.xml", "MainMenu");
while (mainOption != "Quit")
{
    mainOption = SelectOption(mainOptions, "Welcome to Ranker! What media would you like to use?");
    if (mainOption == "Quit") { break; }
    RunActionMenu(mainOption);
}

static void RunActionMenu(string mediaType)
{
    string actionOption = "";
    List<string> actionOptions = QuickReadXML("Menu.xml", "ActionMenu");
    while (actionOption != "Quit")
    {
        actionOption = SelectOption(actionOptions, $"You're working with {mediaType}. What would you like to do?");
        switch (actionOption)
        {
            case "Add":
                AddItems(mediaType);
                break;
            case "View":
                ViewItems(mediaType);
                break;
            case "Rank":
                RankItems(mediaType, 64);
                break;
            case "Quit":
                break;
        }
    }
}

static List<string> QuickReadXML(string xmlName, string nodeName)
{
    List<string> result = [];
    XmlDocument doc = new();
    doc.Load($"../../../{xmlName}");
    XmlElement? root = doc.DocumentElement;
    if (root == null) { return result; }
    foreach (XmlNode node in root.ChildNodes)
    {
        if (node.Name == nodeName)
        {
            foreach (XmlNode subNode in node.ChildNodes) { result.Add(subNode.InnerText); }
            break;
        }
    }
    return result;
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

static void AddItems(string mediaType)
{
    XmlDocument doc = new();
    List<string> templateParts = QuickReadXML($"{mediaType}.xml", "Template");
    List<string> itemParts = [];
    string? userInput;
    bool doneAdding = false;
    bool itemPresent;
    XmlElement itemElement;
    XmlElement subElement;
    XmlElement? root;
    XmlElement itemELO;

    while (!doneAdding)
    {
        doc.Load($"../../../{mediaType}.xml");
        root = doc.DocumentElement;
        if (root == null) { return; }
        itemELO = doc.CreateElement("ELO");
        itemELO.InnerText = "1000";

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
            itemPresent = IsItemPresent(itemParts, root, mediaType[..^1]);
            if (itemPresent) { Console.WriteLine("The entered values are already present in the system. Please enter values for a new item."); }
        } while (itemPresent);

        itemElement = doc.CreateElement(mediaType[..^1]);
        for (int i = 0; i < itemParts.Count; i++)
        {
            subElement = doc.CreateElement(templateParts[i]);
            subElement.InnerText = itemParts[i];
            itemElement.AppendChild(subElement);
        }
        itemElement.AppendChild(itemELO);
        root.AppendChild(itemElement);
        doc.Save($"../../../{mediaType}.xml");

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

static bool IsItemPresent(List<string> itemParts, XmlElement root, string nodeName)
{
    List<string> compItemParts = [];
    foreach (XmlNode node in root.ChildNodes)
    {
        if (compItemParts.Count > 0) { compItemParts.Clear(); }
        if (node.Name == nodeName)
        {
            foreach (XmlNode subNode in node.ChildNodes)
            {
                if (subNode.Name != "ELO") { compItemParts.Add(subNode.InnerText); }
            }
            for (int i = 0; i < itemParts.Count; i++)
            {
                if (itemParts[i] != compItemParts[i]) { break; }
                else if (i == itemParts.Count - 1) { return true; }
            }
        }
    }
    return false;
}

static void ViewItems(string mediaType)
{
    XmlDocument doc = new();
    doc.Load($"../../../{mediaType}.xml");
    List<XmlNode> itemNodeList = [];
    Dictionary<int, int> eloDict = [];
    string itemText;

    XmlElement? root = doc.DocumentElement;
    if (root == null) { return; }
    foreach (XmlNode node in root.ChildNodes)
    {
        if (node.Name != "Template") { itemNodeList.Add(node); }
    }

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

static void RankItems(string mediaType, int k)
{
    // Load the items 
    XmlDocument doc = new();
    doc.Load($"../../../{mediaType}.xml");
    List<XmlNode> items = [];

    XmlElement? root = doc.DocumentElement;
    if (root == null) { return; }
    foreach (XmlNode node in root.ChildNodes)
    {
        if (node.Name != "Template") { items.Add(node); }
    }

    // Rank the items
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
    doc.Save($"../../../{mediaType}.xml");
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

