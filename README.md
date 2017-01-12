# Subliminal
Subliminal is a software library that implements a minimax algorithm with alpha-beta pruning to play against a human player in the popular game of Connect Four. Connect Four GUI is the application that interacts with the library and uses WPF for its design.

![alt text](http://i.imgur.com/tq5JzNQ.png "Subliminal GUI")

# How to Play
Using your mouse, left-click in an empty slot on the board to insert player disc. The disc will fall into the lowest unoccupied slot within selected column. Win the game by connecting four of the same colored discs horizontally, vertically, or diagonally.

# System Requirements
- Windows 7 or above
- .NET Framework 4.5
- Visual Studio 2013

# GUI Features
- Options
 - Reset:	Click button to reset current game.
 - Exit:	Click button to exit application.
 - Difficulty:	Adjust slider the change difficulty (look-ahead) of computer player between 1 and 7. The higher the value, the longer computation time between moves.
- Board
 - Export Image:	Right click board and select “Export Image” to export the currently displayed connect four game to a PNG picture.
