# What is Scopus #
Scopus is framework that enables .NET developers to write parsers. The framework features in-code grammar definition using [BNF](http://en.wikipedia.org/wiki/Backus%E2%80%93Naur_Form)-like syntax including semantic actions. Therefore it is possible to make refactoring in semantic actions. The aim of our project is to write parser generator that generates parallel parsers (multithreaded) for better performance. However the current version does not provide this functionality yet.

## Current Version ##
The current (alpha) version supports regular expressions (set in a object form, the parser for regexps is in development). Parsing algorithm that is used is SLR (as described in 'Dragon Book'). Scopus lexer supports any byte-aligned encoding that implements System.Text.Encoding abstract class. Unicode encodings: UTF-8, UTF-16le, UTF-16be as well as old ASCII encoding. UTF-7 is not supported since it is not byte-aligned.

**The first milestones are (in order of priority):** (updated)

1) complete parser for regular expressions (POSIX notation)

Although Scopus framework looks like it is something unfinished, it is well tested and could be easily integrated into your code. See wiki page for an example. Soon we will release first version (1.0) [I complete the regexps parser](after.md). After this we start implementing the parallelization part.

If you have any ideas or suggestions regarding Scopus, don't hesitate to write us.