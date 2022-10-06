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
