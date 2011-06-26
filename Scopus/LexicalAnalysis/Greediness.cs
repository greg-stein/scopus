namespace Scopus.LexicalAnalysis
{
    /// <summary>
    /// Defines greediness of lexing, applied to tokens. Indicates whether lexer will accept on first or last possible match
    /// </summary>
    public enum Greediness
    {
        /// <summary>
        /// Indicates greedy quantification mode. For example, if aab* is a token, then in greedy quantification mode
        /// feeding input 'aabbbb' to lexer will cause a string 'aabbbb' to be matched. I.e. last possible match
        /// </summary>
        GreedyQuantification = 0x0,

        /// <summary>
        /// Indicates lazy quantification mode. For example, if aab* is a token, then in lazy quantification mode
        /// feeding input 'aabb' to lexer will cause a string 'aab' to be matched first. I.e. first possible match
        /// </summary>
        LazyQuantification
    }
}
