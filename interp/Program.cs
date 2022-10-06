// See https://aka.ms/new-console-template for more information
using System.Runtime.CompilerServices;
using interpr;

//public delegate Lexem CallBackFunction(Stack<Lexem> parameters); // указатель на функцию для вызова из программы
static Lexem Sum(Stack<Lexem> parameters)
{
    Lexem lex2 = parameters.Pop();
    Lexem lex1 = parameters.Pop();
    return lex1 += lex2;
}
Parser parser = new();
//parser.ProgText = "iif(6/4==2, \"Выражение истинно\", \"Выражение ложно\" )+ \" сегодня\"";
parser.VarAdd("a", 6);
parser.VarAdd("b", 3);
parser.FuncAdd("Sum", Sum);
parser.ProgText = "-1+Sum(a/b,-1)";
var val = parser.Run();
Console.WriteLine(parser.ProgText + "= {0}", val);
val = parser.Run();
Console.WriteLine(parser.ProgText + "= {0}", val);


