# Ranker

This is a C# console application for ranking books, games, movies, and songs. 
It uses XML files for persistent storage and offers a simple interface for viewing your favorite media.

## Features 
- XML-based storage for easy portability and readability
- ELO-based ranking system for different media types
- Add and view entries via console interface

### Installation
1. Clone the repository:
```bash
git clone https://github.com/Austin-Buchanan/Ranker.git
```
2. Open the solution in Visual Studio or your preferred IDE for .NET projects.
3. Build and run the project.

### Adding Items

When adding items, you will be prompted for different data to identify the item. 
For example, when adding a book, you will be prompted to provide a title and author. 

Each time you add an item, the application will check if an identical item is already in the system. If so, you will be asked to provide data for a different item.

The application allows you to continue adding items until you would like to stop. 

### Viewing Items
When viewing items, you will be asked if you want to view items in ascending or descending order. 
The application uses the ELO score assigned to each item for this ranking. 

### Ranking Items

When ranking items, the application will randomly select two items of the selected media type. 
You will be asked which item you prefer. 
After selecting an item, the application will use [ELO calculations](https://en.wikipedia.org/wiki/Elo_rating_system) to update the ELO ranking for each item. 
You can view the ELO of each item by navigating to the View screen or by opening the associated XML file. 
