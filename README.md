# scopus
Automatically exported from code.google.com/p/scopus

Getting Started with Scopus
First, let me show you how you can describe your grammar with Scopus Assume you have grammar for arithmetic expressions that include + and * operators: 

E --> E + T | T 
T --> T * F | F 
F --> ( E ) | id 

Here E, T and F are non-terminals and +, *, (, ) and id are terminals
Here is how you may define it using Scopus:


    var E = new NonTerminal("E"); 
    var T = new NonTerminal("T"); 
    var F = new NonTerminal("F"); 
    
    var id = tokenizer.IntegerNumber; 
    var plus = tokenizer.AddToken("+"); 
    var mult = tokenizer.AddToken("*"); 
    var leftBrace = tokenizer.AddToken("("); 
    var rightBrace = tokenizer.AddToken(")");
    
    var grammar = new AugmentedGrammar() 
                  { 
                      E --> E & plus & T,
                      E --> T, 
                      T --> T & mult & F, 
                      T --> F,
                      F --> leftBrace & E & rightBrace,
                      F --> id
                  }; 

The syntax is very close to http://en.wikipedia.org/wiki/Backus%E2%80%93Naur_Form'>BNF. Now, let's add some semantic actions, so after parsing of arithmetic expression we will get the result:

    var stack = new Stack<int>();
    
    var grammar = new AugmentedGrammar(tokenizer.TotalTokensCount) 
                  { 
                      E --> E & plus & T ^ (v => stack.Push(stack.Pop() + stack.Pop())),
                      E --> T, 
                      T --> T & mult & F ^ (v => stack.Push(stack.Pop() * stack.Pop())),
                      T --> F, 
                      F --> leftBrace & E & rightBrace, 
                      F --> id ^ (v => stack.Push(v[id].AsInt())) 
                  };

As you can see semantic actions are assigned to production rules using ^ operator.
Whole working code looks like this one took from Scopus unit tests:

     [Test] 
        public void ArithmeticStatementFullyAutonomousParseTest() 
        {
            const string INPUT = @"2*(3+4)";

            string fileName = Path.GetTempFileName();
            File.WriteAllText(fileName, INPUT);
            RegExpTokenizer tokenizer = new RegExpTokenizer(); 
            ILexer lexer = new Lexer(tokenizer);
            Stream fileStream = File.OpenRead(fileName); 
            lexer.SetDataSource(fileStream); 

            var stack = new Stack<int>(); 
            var parser = new LRParser(); 

            var E = new NonTerminal("E");
            var T = new NonTerminal("T"); 
            var F = new NonTerminal("F"); 

            var id = tokenizer.IntegerNumber; 
            var plus = tokenizer.AddToken("+"); 
            var mult = tokenizer.AddToken("*"); 
            var leftBrace = tokenizer.AddToken("(");
            var rightBrace = tokenizer.AddToken(")"); 

            var grammar = new AugmentedGrammar(tokenizer.TotalTokensCount) 
                              { 
                                  E --> E & plus & T ^ (v => stack.Push(stack.Pop() + stack.Pop())), 
                                  E --> T, 
                                  T --> T & mult & F ^ (v => stack.Push(stack.Pop() * stack.Pop())), 
                                  T --> F, 
                                  F --> leftBrace & E & rightBrace, 
                                  F --> id ^ (v => stack.Push(v[id].AsInt())) 
                              }; 

            Console.WriteLine("Grammar is being tested: \n{0}", grammar); 
            Console.WriteLine("Input is being parsed: {0}\n", INPUT); 
            Console.WriteLine("Parsing process:\n"); 
            parser.Grammar = grammar; 
            parser.Lexer = lexer; 
            parser.ParsingTable = new ParsingTable(grammar); 
            parser.InputAccepted += (sender, eventArgs) => Console.WriteLine("Accepted!"); 
            parser.ParseInput(); 

            Assert.That(stack.Pop(), Is.EqualTo(14)); 
            Assert.That(stack.Count, Is.EqualTo(0)); 

            fileStream.Close(); 
            File.Delete(fileName); 
        } 
