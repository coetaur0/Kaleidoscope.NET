namespace Kaleidoscope.Parser;

/// <summary>
/// A lexical analyser.
/// </summary>
internal sealed class Lexer(Source source)
{
    private int _line = 1;

    private int _column = 1;

    private int _offset;

    private bool _depleted;

    /// <summary>
    /// Returns the next token in the source, or null if all of its token have already been consumed.
    /// </summary>
    public Token? Next()
    {
        if (_depleted)
        {
            return null;
        }

        SkipTrivia();
        var start = new Location(_line, _column, _offset);

        if (_offset >= source.Length)
        {
            _depleted = true;
            return new Token(TokenKind.Eof, new Range(start, start));
        }

        TokenKind kind;
        switch (source[_offset])
        {
            case var nextChar when char.IsLetter(nextChar):
                var symbol = Consume(c => char.IsLetter(c) || char.IsDigit(c) || c == '_');
                if (symbol == "def")
                {
                    kind = TokenKind.Def;
                }
                else if (symbol == "extern")
                {
                    kind = TokenKind.Extern;
                }
                else
                {
                    kind = TokenKind.Identifier;
                }

                break;

            case var nextChar when char.IsDigit(nextChar):
                Consume(char.IsDigit);

                if (source[_offset] == '.')
                {
                    Advance(1);
                }

                Consume(char.IsDigit);
                kind = TokenKind.Number;

                break;

            default:
                kind = source[_offset] switch
                {
                    '<' => TokenKind.Less,
                    '+' => TokenKind.Plus,
                    '-' => TokenKind.Minus,
                    '*' => TokenKind.Times,
                    '(' => TokenKind.LeftParen,
                    ')' => TokenKind.RightParen,
                    ',' => TokenKind.Comma,
                    _ => TokenKind.Unknown
                };
                Advance(1);
                break;
        }

        return new Token(kind, new Range(start, new Location(_line, _column, _offset)));
    }

    /// <summary>
    /// Advance the lexer's position in the source by some offset.
    /// </summary>
    private void Advance(int offset)
    {
        for (var i = 0; i < offset; i++)
        {
            if (_offset >= source.Length)
            {
                return;
            }

            if (source[offset] == '\n')
            {
                _line++;
                _column = 1;
            }
            else
            {
                _column++;
            }

            _offset++;
        }
    }

    /// <summary>
    /// Consumes characters in the source as long as they satisfy some predicate and returns them in a span.
    /// </summary>
    private ReadOnlySpan<char> Consume(Predicate<char> predicate)
    {
        var start = _offset;

        while (_offset < source.Length && predicate(source[_offset]))
        {
            if (source[_offset] == '\n')
            {
                _line++;
                _column = 1;
            }
            else
            {
                _column++;
            }

            _offset++;
        }

        return source[start, _offset];
    }

    /// <summary>
    /// Skips whitespace and comments in the source.
    /// </summary>
    private void SkipTrivia()
    {
        Consume(char.IsWhiteSpace);

        while (_offset < source.Length && source[_offset] == '#')
        {
            Consume(c => c != '\n');
            Consume(char.IsWhiteSpace);
        }
    }
}