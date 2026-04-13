grammar KotlinList;

file
    : statement* EOF
    ;

statement
    : declaration
    | declarationNoVal
    ;

declaration
    : VAL IDENTIFIER ASSIGN LISTOF LPAREN elementsOpt RPAREN SEMI
    ;

declarationNoVal
    : IDENTIFIER ASSIGN LISTOF LPAREN elementsOpt RPAREN SEMI
    ;

elementsOpt
    : elements
    |
    ;

elements
    : element (COMMA element)*
    ;

element
    : STRING
    | CHAR
    | TRUE
    | FALSE
    | numberLiteral
    ;

numberLiteral
    : INT
    | DOUBLE
    | sign INT
    | sign DOUBLE
    ;

sign
    : PLUS
    | MINUS
    ;

VAL     : 'val';
LISTOF  : 'listOf';
TRUE    : 'true';
FALSE   : 'false';
ASSIGN  : '=';
LPAREN  : '(';
RPAREN  : ')';
COMMA   : ',';
SEMI    : ';';
PLUS    : '+';
MINUS   : '-';

DOUBLE  : DIGIT+ '.' DIGIT+;
INT     : DIGIT+;

CHAR
    : '\'' (~['\\\r\n] | '\\' .) '\''
    ;

STRING
    : '"' (~["\\\r\n] | '\\' .)* '"'
    ;

IDENTIFIER
    : LETTER (LETTER | DIGIT | '_')*
    ;

WS
    : [ \t\r\n]+ -> skip
    ;

fragment DIGIT  : [0-9];
fragment LETTER : [a-zA-Z];