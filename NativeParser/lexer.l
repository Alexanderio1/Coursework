%{
#include "parser.tab.h"
#include <string.h>

int yycolumn = 1;
int token_start_line = 1;
int token_start_column = 1;
int token_end_line = 1;
int token_end_column = 1;
char token_text[256] = "";

static void mark_token()
{
    int len = yyleng < 255 ? yyleng : 255;

    token_start_line = yylineno;
    token_start_column = yycolumn;
    token_end_line = yylineno;
    token_end_column = yycolumn + yyleng - 1;

    memcpy(token_text, yytext, len);
    token_text[len] = '\0';

    yycolumn += yyleng;
}
%}

%option noyywrap
%option yylineno
%option noinput
%option nounput

DIGIT   [0-9]
ID      [A-Za-z_][A-Za-z0-9_]*
INVALID_SEQ [^[:space:]A-Za-z_0-9(),;=+\"\'-]+

%%

[ \t\r]+                { yycolumn += yyleng; }
\n                      { yycolumn = 1; }

"val"                   { mark_token(); return VAL; }
"listOf"                { mark_token(); return LISTOF; }
"true"                  { mark_token(); return TRUE; }
"false"                 { mark_token(); return FALSE; }

"("                     { mark_token(); return LPAREN; }
")"                     { mark_token(); return RPAREN; }
","                     { mark_token(); return COMMA; }
";"                     { mark_token(); return SEMICOLON; }
"="                     { mark_token(); return ASSIGN; }
"+"                     { mark_token(); return PLUS; }
"-"                     { mark_token(); return MINUS; }

\"([^"\\\r\n]|\\.)*\"   { mark_token(); return STRING; }
\'([^'\\\r\n]|\\.)\'    { mark_token(); return CHAR; }

{DIGIT}+"."{DIGIT}+     { mark_token(); return DOUBLE; }
{DIGIT}+                { mark_token(); return INT; }

{ID}                    { mark_token(); return IDENTIFIER; }

{INVALID_SEQ}           { mark_token(); return INVALID; }
.                       { mark_token(); return INVALID; }
%%