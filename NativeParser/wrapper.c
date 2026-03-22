#include <stdio.h>

typedef void* YY_BUFFER_STATE;

int yyparse(void);
YY_BUFFER_STATE yy_scan_string(const char* str);
void yy_delete_buffer(YY_BUFFER_STATE buffer);
extern int yylineno;

typedef void(__cdecl* ErrorCallback)(int line, const char* message);

static ErrorCallback g_errorCallback = NULL;

void yyerror(const char* s)
{
    if (g_errorCallback != NULL)
    {
        g_errorCallback(yylineno, s != NULL ? s : "syntax error");
    }
}

__declspec(dllexport) int __cdecl ParseSourceCode(const char* sourceCode, ErrorCallback errorCallback)
{
    YY_BUFFER_STATE buffer;
    int result;

    g_errorCallback = errorCallback;
    yylineno = 1;

    if (sourceCode == NULL)
    {
        if (g_errorCallback != NULL)
            g_errorCallback(1, "empty input");
        return 1;
    }

    buffer = yy_scan_string(sourceCode);
    if (buffer == NULL)
    {
        if (g_errorCallback != NULL)
            g_errorCallback(1, "failed to create lexer buffer");
        return 1;
    }

    result = yyparse();

    yy_delete_buffer(buffer);
    g_errorCallback = NULL;

    return (result == 0) ? 0 : 1;
}