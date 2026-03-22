%{
#include "parser.tab.h"
%}

%option noyywrap
%option yylineno
%option noinput
%option nounput

DIGIT   [0-9]
ID      [A-Za-z_][A-Za-z0-9_]*

%%

"val"                   { return VAL; }
"listOf"                { return LISTOF; }
"true"                  { return TRUE; }
"false"                 { return FALSE; }

"("                     { return LPAREN; }
")"                     { return RPAREN; }
","                     { return COMMA; }
";"                     { return SEMICOLON; }
"="                     { return ASSIGN; }
"+"                     { return PLUS; }
"-"                     { return MINUS; }

\"([^"\\\r\n]|\\.)*\"   { return STRING; }

{DIGIT}+"."{DIGIT}+     { return DOUBLE; }
{DIGIT}+                { return INT; }

{ID}                    { return IDENTIFIER; }

[ \t\r\n]+              { /* skip whitespace */ }

.                       { return INVALID; }

%%