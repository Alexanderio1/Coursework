%{
#include <stdio.h>

int yylex(void);

extern int token_start_line;
extern int token_start_column;
extern int token_end_line;
extern int token_end_column;
extern char token_text[256];

void ReportParserError(
    const char* message,
    int startLine,
    int startColumn,
    int endLine,
    int endColumn,
    const char* lexeme
);

void yyerror(const char* s);
%}

%token VAL
%token LISTOF
%token IDENTIFIER
%token STRING
%token INT
%token DOUBLE
%token TRUE
%token FALSE
%token LPAREN
%token RPAREN
%token COMMA
%token SEMICOLON
%token ASSIGN
%token PLUS
%token MINUS
%token INVALID

%start program

%%

program
    : statement
    ;

statement
    : VAL IDENTIFIER ASSIGN LISTOF LPAREN elements RPAREN SEMICOLON
    ;

elements
    : element
    | elements COMMA element
    ;

element
    : STRING
    | TRUE
    | FALSE
    | signed_number
    ;

signed_number
    : INT
    | DOUBLE
    | PLUS INT
    | MINUS INT
    | PLUS DOUBLE
    | MINUS DOUBLE
    ;

%%

void yyerror(const char* s)
{
    ReportParserError(
        s != NULL ? s : "syntax error",
        token_start_line,
        token_start_column,
        token_end_line,
        token_end_column,
        token_text
    );
}