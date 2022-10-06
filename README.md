# Parser
Парсер арифметических выражений
## Поддержка арифметических операций
```
Parser parser = new();
parser.ProgText = "-1+6/3+(-1)";
var val = parser.Run();
Console.WriteLine(parser.ProgText + "= {0}", val);
```
напечатает:

-1+6/3+(-1) = 0
## Поддержка переменных
```
Parser parser = new();
parser.VarAdd("a", 6);
parser.VarAdd("b", 3);
parser.ProgText = "-1+a/b+(-1)";
var val = parser.Run();
Console.WriteLine(parser.ProgText + "= {0}", val);
```
напечатает: 

-1+a/b+(-1)= 0

(Переменные должны быть добавлены до установки свойства ProgText)
## Поддержка if/else
Поддержка реализована через встроенную функцию
```
iif(<выражение для сравнения>, <возврат для иснины>, <возврат для лжи>)
```
Например:
```
Parser parser = new();
parser.ProgText = "iif(6/4==2, \"Выражение истинно\", \"Выражение ложно\" )+ \" сегодня\"";
var val = parser.Run();
Console.WriteLine(parser.ProgText + "= {0}", val);
```
напечатает:

iif(6/4==2, "Выражение истинно", "Выражение ложно" )+ " сегодня"= Выражение ложно сегодня
## Встраивание функций
В парсер можно встраивать собственные функции C#. Эти функции должны иметь вид:
```
public delegate Lexem CallBackFunction(Stack<Lexem> parameters); // "указатель" на функцию для вызова из программы
```
Функция может иметь любое число парамертов. Параметры передаются через собственный стэк парсера. Первым в стэк попадает первый параметр. Погследний парамерт попадает в стэк последним. Функция при вызове должна самостоятельно извлекать параметры из сэка. Поэтому последний параметр извлекается перым, а первый параметр извлекается последним.

Следовательно, если мы хотим встроить функцию Sum(a, b), которая принемает два параметра и складывает их, мы должны написать следующий текст:
```
static Lexem Sum(Stack<Lexem> parameters)
{
    Lexem lex2 = parameters.Pop();
    Lexem lex1 = parameters.Pop();
    return lex1 += lex2;
}
Parser parser = new();
parser.VarAdd("a", 6);
parser.VarAdd("b", 3);
parser.FuncAdd("Sum", Sum);
parser.ProgText = "-1+Sum(a/b,-1)";
var val = parser.Run();
Console.WriteLine(parser.ProgText + "= {0}", val);
```
при запуске прорамма напечатает:

-1+Sum(a/b,-1)= 0

Переменные и функции должны быть добавлены в парсер до установки свойства progText

