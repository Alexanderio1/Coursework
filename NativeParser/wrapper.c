typedef void* YY_BUFFER_STATE;

int yyparse(void);
YY_BUFFER_STATE yy_scan_string(const char* str);
void yy_delete_buffer(YY_BUFFER_STATE buffer);

extern int yylineno;
extern int yycolumn;
extern int token_start_line;
extern int token_start_column;
extern int token_end_line;
extern int token_end_column;
extern char token_text[256];

typedef void(__cdecl* ErrorCallback)(
    int startLine,
    int startColumn,
    int endLine,
    int endColumn,
    const char* message,
    const char* lexeme
    );

static ErrorCallback g_errorCallback = 0;

void ReportParserError(
    const char* message,
    int startLine,
    int startColumn,
    int endLine,
    int endColumn,
    const char* lexeme
)
{
    if (g_errorCallback != 0)
    {
        g_errorCallback(
            startLine,
            startColumn,
            endLine,
            endColumn,
            message != 0 ? message : "syntax error",
            lexeme != 0 ? lexeme : ""
        );
    }
}

__declspec(dllexport) int __cdecl ParseSourceCode(const char* sourceCode, ErrorCallback errorCallback)
{
    YY_BUFFER_STATE buffer;
    int result;

    g_errorCallback = errorCallback;

    yylineno = 1;
    yycolumn = 1;
    token_start_line = 1;
    token_start_column = 1;
    token_end_line = 1;
    token_end_column = 1;
    token_text[0] = '\0';

    if (sourceCode == 0)
    {
        ReportParserError("empty input", 1, 1, 1, 1, "");
        g_errorCallback = 0;
        return 1;
    }

    buffer = yy_scan_string(sourceCode);
    if (buffer == 0)
    {
        ReportParserError("failed to create lexer buffer", 1, 1, 1, 1, "");
        g_errorCallback = 0;
        return 1;
    }

    result = yyparse();

    yy_delete_buffer(buffer);
    g_errorCallback = 0;

    return (result == 0) ? 0 : 1;
}