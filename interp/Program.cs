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
Parser interp = new()
{
    //interp.progText = "\"1.1\" + \"2.2\"";
    //progText = "2 * 3"
    //progText = "ab + 1"
    //progText = "-Sum(1+2*(-3),Sum(2,3))"
};
interp.VarAdd("ab", 2);
interp.FuncAdd("Sum", Sum);
interp.ProgText = "iif(6/4==2, \"Выражение истинно\", \"Выражение ложно\" )+ \" сегодня\"";
var val = interp.Run();
Console.WriteLine(interp.ProgText + "= {0}", val);


//Lexem[] lex = new Lexem[3];
//int all = 0;
//var i = interp.GetLexem(interp.progText, all, out lex[0]);
//all += i;   // конец взятого слова
//++all;      // выходим за слово
//if (all < interp.progText.Length) // если не дошли до конца текста
//    i = interp.GetLexem(interp.progText, all, out lex[1]);
//all += i;
//++all;
//if (all < interp.progText.Length) // если не дошли до конца текста
//    i = interp.GetLexem(interp.progText, all, out lex[2]);

//string[] arr = new string[] { "2", "+", "3", "*", "(", "4", "-", "5", ")", "+", "6", "-", "7" };
//RecursiveDescentParser parser = new(arr);
//var d = parser.Parse();
//string str = "";
//foreach (string s in arr)
//    str += s;
//str += "=";
//Console.WriteLine(str +"{0}", d);
