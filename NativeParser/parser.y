%{
#include <stdio.h>

int yylex(void);
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