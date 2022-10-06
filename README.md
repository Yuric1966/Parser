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
Parser parser = new();
parser.VarAdd("a", 6);
parser.VarAdd("b", 3);
parser.ProgText = "-1+a/b+(-1)";
var val = parser.Run();
Console.WriteLine(parser.ProgText + "= {0}", val);
