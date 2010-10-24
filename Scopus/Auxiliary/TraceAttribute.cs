//using System;
//using log4net;
//using ParsLib.Lexemes;
//using PostSharp.Laos;

//namespace ParsLib.Auxiliary
//{
//    /// <summary>
//    /// Tells PostSharp to trace Visit methods of the Lexer class
//    /// </summary>
//    [Serializable]
//    public class TraceAttribute : OnMethodBoundaryAspect
//    {
//        public static bool EnableTracer { get; set; }

//        private static readonly ILog LOG = LogManager.GetLogger(typeof(Lexer));

//        static TraceAttribute()
//        {
//            EnableTracer = true;
//        }

//        public override void OnExit(MethodExecutionEventArgs eventArgs)
//        {
//            if (!EnableTracer)
//                return;

//            var lexer = (Lexer)eventArgs.Instance;
//            var lexeme = (Lexeme)eventArgs.GetReadOnlyArgumentArray()[0];

//            LOG.DebugFormat("Lexer.Visit for {0}:'{1}' transition to state: {2}",
//                typeof(Lexeme), lexeme, lexer.CurrentState);
//        }
//    }
//}
