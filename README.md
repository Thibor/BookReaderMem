# BookReaderMem
BoookReaderMem can be used as normal UCI chess engine in chess GUI like Arena.
This program can read and add moves to chess openig book with mem extension.
To use this program you need install  <a href="https://dotnet.microsoft.com/download/dotnet-framework/net48">.NET Framework 4.8</a>

## Parameters

**-bn** mem opening Book file Name<br/>
**-ef** chess Engine File name<br/>
**-ea** chess Engine Arguments<br/>
**-w** add new moves to the book<br/>

## Console commands

**book load** [filename].[mem] - clear and add<br/>
**book save** [filename].[mem] - save book to the file<br/>
**book addfile** [filename].[mem] - add moves to the book<br/>
**book clear** - clear all moves from the book<br/>

### Examples

-bn book.mem -ef stockfish.exe<br/>
book.mem -ef stockfish.exe

The program will first try to find move in chess opening book named book.mem, and if it doesn't find any move in it, it will run a chess engine named stockfish.exe

-bn bigmem.mem -w -ef stockfish.exe
bigmem -ef stockfish.exe -w

The program will first try to find move in chess opening book named bigmem.mem, and if it doesn't find any move in it, it will run a chess engine named stockfish.exe. If engine loose game, winner moves will be added to the book. If the book does not exist, the program will create it.


