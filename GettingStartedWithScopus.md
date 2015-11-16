# Getting Started with Scopus #

First, let me show you how you can describe your grammar with Scopus
Assume you have grammar for arithmetic expressions that include + and `*` operators: <br>

E --> E + T | T <br>
T --> T <code>*</code> F | F <br>
F --> ( E ) | id <br>

Here E, T and F are non-terminals and +, <code>*</code>, (, ) and id are terminals<br>
<br>
Here is how you may define it using Scopus:<br>
<br>
<pre><code>var E = new NonTerminal("E"); <br>
var T = new NonTerminal("T"); <br>
var F = new NonTerminal("F"); <br>
 <br>
var id = tokenizer.IntegerNumber; <br>
var plus = tokenizer.AddToken("+"); <br>
var mult = tokenizer.AddToken("*"); <br>
var leftBrace = tokenizer.AddToken("("); <br>
var rightBrace = tokenizer.AddToken(")");<br>
<br>
var grammar = new AugmentedGrammar() <br>
                  { <br>
                      E --&gt; E &amp; plus &amp; T,<br>
                      E --&gt; T, <br>
                      T --&gt; T &amp; mult &amp; F, <br>
                      T --&gt; F,<br>
                      F --&gt; leftBrace &amp; E &amp; rightBrace,<br>
                      F --&gt; id<br>
                  }; <br>
</code></pre>

The syntax is very close to <a href='http://en.wikipedia.org/wiki/Backus%E2%80%93Naur_Form'>BNF</a>. Now, let's add some semantic actions, so after parsing of arithmetic expression we will get the result:<br>
<pre><code>var stack = new Stack&lt;int&gt;();<br>
<br>
var grammar = new AugmentedGrammar(tokenizer.TotalTokensCount) <br>
                  { <br>
                      E --&gt; E &amp; plus &amp; T ^ (v =&gt; stack.Push(stack.Pop() + stack.Pop())),<br>
                      E --&gt; T, <br>
                      T --&gt; T &amp; mult &amp; F ^ (v =&gt; stack.Push(stack.Pop() * stack.Pop())),<br>
                      T --&gt; F, <br>
                      F --&gt; leftBrace &amp; E &amp; rightBrace, <br>
                      F --&gt; id ^ (v =&gt; stack.Push(v[id].AsInt())) <br>
                  };<br>
</code></pre>

As you can see semantic actions are assigned to production rules using ^ operator.<br>
<br>
<hr />
Whole working code is looks like this one took from Scopus unit tests:<br>
<pre><code>		[Test] <br>
		public void ArithmeticStatementFullyAutonomousParseTest() <br>
		{<br>
			const string INPUT = @"2*(3+4)";<br>
<br>
			string fileName = Path.GetTempFileName();<br>
			File.WriteAllText(fileName, INPUT);<br>
			RegExpTokenizer tokenizer = new RegExpTokenizer(); <br>
			ILexer lexer = new Lexer(tokenizer);<br>
			Stream fileStream = File.OpenRead(fileName); <br>
			lexer.SetDataSource(fileStream); <br>
<br>
			var stack = new Stack&lt;int&gt;(); <br>
			var parser = new LRParser(); <br>
<br>
			var E = new NonTerminal("E");<br>
			var T = new NonTerminal("T"); <br>
			var F = new NonTerminal("F"); <br>
<br>
			var id = tokenizer.IntegerNumber; <br>
			var plus = tokenizer.AddToken("+"); <br>
			var mult = tokenizer.AddToken("*"); <br>
			var leftBrace = tokenizer.AddToken("(");<br>
			var rightBrace = tokenizer.AddToken(")"); <br>
<br>
			var grammar = new AugmentedGrammar(tokenizer.TotalTokensCount) <br>
                              { <br>
                                  E --&gt; E &amp; plus &amp; T ^ (v =&gt; stack.Push(stack.Pop() + stack.Pop())), <br>
                                  E --&gt; T, <br>
                                  T --&gt; T &amp; mult &amp; F ^ (v =&gt; stack.Push(stack.Pop() * stack.Pop())), <br>
                                  T --&gt; F, <br>
                                  F --&gt; leftBrace &amp; E &amp; rightBrace, <br>
                                  F --&gt; id ^ (v =&gt; stack.Push(v[id].AsInt())) <br>
                              }; <br>
<br>
			Console.WriteLine("Grammar is being tested: \n{0}", grammar); <br>
			Console.WriteLine("Input is being parsed: {0}\n", INPUT); <br>
			Console.WriteLine("Parsing process:\n"); <br>
			parser.Grammar = grammar; <br>
			parser.Lexer = lexer; <br>
			parser.ParsingTable = new ParsingTable(grammar); <br>
			parser.InputAccepted += (sender, eventArgs) =&gt; Console.WriteLine("Accepted!"); <br>
			parser.ParseInput(); <br>
<br>
			Assert.That(stack.Pop(), Is.EqualTo(14)); <br>
			Assert.That(stack.Count, Is.EqualTo(0)); <br>
<br>
<br>
			fileStream.Close(); <br>
			File.Delete(fileName); <br>
		} <br>
</code></pre>