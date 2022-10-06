using System;
using System.Dynamic;
using System.Text;
using System.Net.Http;
using System.Data.SqlTypes;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using Microsoft.CSharp;

namespace interpr
{
    public enum LexemType
    {
        ltDoble, ltLiteral, ltBoolean, ltOperation, ltBracketOpn, ltBracketCls, ltVariable, ltFunction
    }
    public delegate Lexem CallBackFunction(Stack<Lexem> parameters); // указатель на функцию для вызова из программы
    public class Lexem
    {
        public static readonly string operationStr = "+-*/";
        public static readonly string singleLexems = "+-*/(){}[],";
        public static readonly List<string> operationsBool = new List<string>() { "==", "!=", ">=", "<=", ">", "<"};
        string _content;
        LexemType _type;
        dynamic _value;
        public String Content 
        { 
            get => _content;  
            set
            {
                if (value[0] == '"')// литерал
                {
                    _content = value;
                    _type = LexemType.ltLiteral;
                    _value = value.Trim('"');
                }
                else if (char.IsLetter(value[0]) || (value[0] == '_'))
                {
                    _content = value;
                    _type= LexemType.ltFunction;
                    _value = null;
                }
                else if (char.IsDigit(value[0]))
                {
                    _content = value;
                    _type = LexemType.ltDoble;
                    _value = Convert.ToDouble(value);
                }
                else if (IsOperation(value[0]))
                {
                    _content = value;
                    _type= LexemType.ltOperation;
                    _value = null;
                }
                else if (value[0] == '(')
                {
                    _content= value[0].ToString();
                    _type= LexemType.ltBracketOpn;
                    _value = null;
                }
                else if (value[0] == ')')
                {
                    _content = value[0].ToString();
                    _type = LexemType.ltBracketCls;
                    _value= null;
                }
                else if (value[0] == ',')
                {
                    _content = value[0].ToString();
                    _type = LexemType.ltBracketCls;
                    _value = null;
                }
                else if (IsBoolOperation(value))
                {
                    _content = value;
                    _type = LexemType.ltOperation;
                    _value = null;
                }
            }
        }
        public LexemType Type { get => _type; set => _type = value; }
        public dynamic Value 
        { 
            get => _value; 
            set
            {
                if (value is Double)
                {
                    _content = value.ToString();
                    _type = LexemType.ltDoble;
                    _value = value;
                }
                else if (value is string)
                {
                    _content = '"'+value.ToString()+'"';
                    _type = LexemType.ltLiteral;
                    _value = (value as string).Trim('"');
                }
                else if (value is bool)
                {
                    _content= value.ToString();
                    _type = LexemType.ltBoolean;
                    _value = value;
                }
                else throw new ArgumentException("Значение должно быть числом двойной точности");
            }
        }
        public string Name { get; set; } // имя переменной или функции
        //public List<dynamic> Parameters = new List<dynamic>(); // параметры функции
        public CallBackFunction Function; // указатель на функцию
        public static Lexem operator + (Lexem lex1, Lexem lex2)
        {
            if(lex1.Value.GetType() == lex2.Value.GetType())
            {
                if(lex1.Value.GetType() == typeof(Double)) 
                    lex1.Value += lex2.Value;
                else if(lex1.Value.GetType() == typeof(string))
                    lex1.Value += lex2.Value;
            }
            else throw new Exception("Нельзя сложить строки и числа:" + lex1.Content + " и " + lex2.Content);
            return lex1;
        }
        public static Lexem operator - (Lexem lex1, Lexem lex2)
        {
            if ((lex1.Value.GetType() == lex2.Value.GetType()) && (lex1.Value.GetType() == typeof(System.Double)))
                lex1.Value -= lex2.Value;
            else throw new Exception("Невозможно вычесть строки:" + lex1.Content + " и " + lex2.Content);
            return lex1;
        }
        public static Lexem operator * (Lexem lex1, Lexem lex2)
        {
            if ((lex1.Value.GetType() == lex2.Value.GetType()) && (lex1.Value.GetType() == typeof(System.Double)))
                lex1.Value *= lex2.Value;
            else throw new Exception("Невозможно умножить строки:" + lex1.Content + " и " + lex2.Content);
            return lex1;
        }
        public static Lexem operator / (Lexem lex1, Lexem lex2)
        {
            if ((lex1.Value.GetType() == lex2.Value.GetType()) && (lex1.Value.GetType() == typeof(System.Double)))
                lex1.Value /= lex2.Value;
            else throw new Exception("Невозможно поделить строки:" + lex1.Content + " и " + lex2.Content);
            return lex1;
        }
        /// <summary>
        /// Проверка: является ли символ оператором
        /// </summary>
        /// <param name="c">проверяемый символ</param>
        /// <returns>true/false</returns>
        public static bool IsOperation(char c) => operationStr.Contains(c);
        /// <summary>
        /// проверка входит ли сивол в число лексем, состоящих из однного символа
        /// </summary>
        /// <param name="c">символ, который необходимо проверить</param>
        /// <returns>true/false</returns>
        public static bool IsSingleLexem(char c) => singleLexems.Contains(c);
        public static bool IsBoolOperation(string s) => operationsBool.IndexOf(s) != -1;
    }
    public class Parser
    {
        string progText;
        public string ProgText
        {
            get { return progText; }
            set 
            { 
                progText = value;
                pos = 0;
                Lexems.Clear();
                stack.Clear();
                ReadProg(ProgText, Lexems);
            }
        }
        private int pos = 0; // индекс текущего токена в списке лексем ()
        public List<Lexem> Lexems = new List<Lexem>();// Текст программы, разбитый по словам
        public List<Lexem> Variables = new List<Lexem>(); // переменные программы
        public List<Lexem> Functions = new List<Lexem>();// встроенные функции программы
        public Stack<Lexem> stack = new Stack<Lexem>(); // стэк для размещения параметров функций
        /// <summary>
        /// очистка внутренностей интерпретатора
        /// </summary>
        public void Reset()
        { 
            pos = 0;
            progText = String.Empty;
            Lexems.Clear();
            Variables.Clear();
            Functions.Clear();
            FuncAdd("iif", iif);// iif считаем встроенной функцией
            stack.Clear();
        }
        /// <summary>
        /// Добавление новой переменной в программу
        /// </summary>
        /// <param name="name">Имя переменной</param>
        /// <param name="value">Значение переменной</param>
        public void VarAdd(string name, double value)
        {
            Lexem lex = new Lexem() { Name = name, Value = value, Type = LexemType.ltVariable };
            Variables.Add(lex);
        }
        /// <summary>
        /// Добавление новой переменной в программу
        /// </summary>
        /// <param name="name">Имя переменной</param>
        /// <param name="value">Значение переменной</param>
        public void VarAdd(string name, string value) => Variables.Add(new Lexem() { Name = name, Value = value });
        /// <summary>
        /// Добавлние функции
        /// </summary>
        /// <param name="name">имя функции</param>
        /// <param name="function">указательна функцию (делегат)</param>
        public void FuncAdd(string name, CallBackFunction function) => Functions.Add(item: new Lexem() { Name = name.ToLower(), Content = name, Function = function });
        static Lexem iif(Stack<Lexem> pars)
        {
            Lexem lexFalse = pars.Pop();
            Lexem lexTrue = pars.Pop();
            Lexem lexBool = pars.Pop();
            return lexBool.Value ? lexTrue : lexFalse;
        }
        /// <summary>
        /// считываем слово из строки str, начиная с index, в lex
        /// </summary>
        /// <param name="str">строка, из которой считываем</param>
        /// <param name="index">номер символа в строке, с которого начинаем считывать</param>
        /// <param name="lex">выходные данные, которые считали</param>
        /// <returns>возвращаем длину считанных данных</returns>
        int GetLexem(string str, int index, out Lexem lex)
        {
            lex = new Lexem();
            int i = index;
            while ((i < str.Length) && Char.IsWhiteSpace(str[i])) ++i;// Пропускаем пробелы
            int iFirst = i;// индекс начала лексемы
            if (i < str.Length)
            { 
                if (Lexem.IsSingleLexem(str[i]))    // лексемы из одного знака
                {
                    lex.Content = str[i++].ToString();
                    return i - index;
                }
                if (str[i] == '"')  // литералы
                {
                    i = str.IndexOf('"', ++i);
                    if (i == -1)
                        throw new Exception("Нет закрывающей кавычки");
                    ++i;// пропускаем закрывающую кавычку
                    lex.Content = str.Substring(iFirst, i - iFirst);
                    return i - index;
                }
                if("><=!".Contains(str[i]))
                {
                    if (str[++i] == '=') 
                        lex.Content = str.Substring(iFirst, 2);
                    else 
                        lex.Content = str[--i].ToString();
                    return ++i - index;
                }
                while ((i < str.Length) && (Char.IsDigit(str[i]) || (str[i] == '.'))) ++i;// считываем слово (лексему)
                if (i != iFirst)    // это цифра
                {
                    lex.Content = str.Substring(iFirst, i - iFirst);
                    return i - index;
                }
                while ((i < str.Length) && (Char.IsLetterOrDigit(str[i]) || (str[i] == '.'))) ++i;// считываем слово (лексему)
                if (i != iFirst)    // это переменная или функция
                {
                    string name = str.Substring(iFirst, i - iFirst);
                    // Ищем ( после имени переменной
                    int j;
                    for (j = i; (j < str.Length) && Char.IsWhiteSpace(str[j]); j++);// Пропускаем пробелы
                    if (str[j] == '(') // Функция
                    {
                        j = str.IndexOf(")", j + 1);
                        if (j == -1)
                            throw new Exception("Нет ')'");
                        Lexem lexem = Functions.Find(x => ((x.Name == name.ToLower()) && (x.Type == LexemType.ltFunction)));
                        if (lexem != null)
                        {
                            lex.Content = lexem.Content;
                            lex.Name = lexem.Name;
                            lex.Type = LexemType.ltFunction;
                            lex.Function = lexem.Function;
                        }
                        else throw new Exception($"Функция \"{name}\" не найдена");
                        return i - index;
                    }
                    else  // переменная
                    {
                        Lexem lexem = Variables.Find(x => ((x.Name == name) && (x.Type == LexemType.ltVariable)));
                        if (lexem != null)
                        {
                            lex.Value = lexem.Value;
                            lex.Name = lexem.Name;
                            lex.Type = LexemType.ltVariable;
                        }
                        else throw new Exception($"переменная \"{name}\" не найдена");
                        return i - index;
                    }
                }
            }
            return 0;
        }
        /// <summary>
        /// Считываем программу в список лексем
        /// </summary>
        /// <param name="str">строка с программой</param>
        /// <param name="lexems">список лексем</param>
        void ReadProg(string str, List<Lexem> lexems)
        {
            int i = 0;
            while (i < str.Length)
            {
                var j = GetLexem(str, i, out Lexem lex);
                if (j != 0) 
                {
                    i += j;
                    lexems.Add(lex);
                }
                else break;
            }
        }
        public Parser()
        {
            FuncAdd("iif", iif);
        }
        public dynamic Run()
        {
            Lexem result = Expression();
            if (pos != Lexems.Count)
                throw new Exception("Ошибка в выражении на позиции: " + Lexems[pos].Content);
            return result.Value;
        }
        /// <summary>
        /// полный расчёт выражения из разобранных лексем
        /// </summary>
        /// <returns></returns>
        // E -> T±T±T±T± ... ±T
        private Lexem Expression()
        {
            bool minusFirst = false;
            // находим первое слагаемое
            Lexem first = Term();
            if (first.Content == "-") // если первое слагаемое знак минуса (-)
            {
                minusFirst = true;
                first = Term();
                if (first.Type == LexemType.ltDoble)
                    first.Value = -first.Value;
            }

            while (pos < Lexems.Count)
            {
                if (first.Type == LexemType.ltFunction)
                {
                    Lexem param = Term(addToStack: true);
                    first = first.Function(stack);
                    if(minusFirst && (first.Type == LexemType.ltDoble))
                        first.Value = -first.Value;
                }
                else
                {
                    Lexem aOperator = Lexems[pos];
                    if ((aOperator.Content == "+") || (aOperator.Content == "-"))
                    {
                        pos++;
                        // находим второе слагаемое (вычитаемое)
                        Lexem second = Term();
                        if (aOperator.Content == "+")
                            first += second;
                        else
                            first -= second;
                    }
                    else if (Lexem.IsBoolOperation(aOperator.Content))
                    {
                        pos++;
                        // находим второе выражениедля сравнения
                        Lexem second = Term();
                        if (aOperator.Content == "==") first.Value = first.Value == second.Value;
                        else if (aOperator.Content == "!=") first.Value = first.Value != second.Value;
                        else if (aOperator.Content == ">=") first.Value = first.Value >= second.Value;
                        else if (aOperator.Content == "<=") first.Value = first.Value <= second.Value;
                        else if (aOperator.Content == ">") first.Value = first.Value > second.Value;
                        else if (aOperator.Content == "<") first.Value = first.Value < second.Value;
                    }
                    else break;
                }
            }
            return first;
        }
        /// <summary>
        /// Читает выражение из строки программы
        /// </summary>
        /// <param name="addToStack">true/false - определяет, помещать ли результирующую лексему в стек для использования в качестве параметра функции</param>
        /// <returns>выдаёт лексему с результатом</returns>
        // T -> F*/F*/F*/*/ ... */F
        private Lexem Term(bool addToStack = false)
        {
            bool minusFirst = false;
            // находим первый множитель
            Lexem first = Factor(addToStack);
            while (pos < Lexems.Count)
            {
                if (first.Type == LexemType.ltFunction)
                {
                    Lexem param = Term(addToStack: true);
                    first = first.Function(stack);
                    if (minusFirst && (first.Type == LexemType.ltDoble))
                        first.Value = -first.Value;
                }
                else
                {
                    Lexem aOperator = Lexems[pos];
                    if (!aOperator.Content.Equals("*") && !aOperator.Content.Equals("/"))
                        break;
                    else
                        pos++;
                    // находим второй множитель (делитель)
                    Lexem second = Factor();
                    if (aOperator.Content.Equals("*"))
                        first *= second;
                    else
                        first /= second;
                }
            }
            return first;
        }
        /// <summary>
        /// Читает выражение из строки программы
        /// </summary>
        /// <param name="addToStack">true/false - определяет, помещать ли результирующую лексему в стек для использования в качестве параметра функции</param>
        /// <returns>выдаёт лексему с результатом</returns>
        /// <exception cref="Exception"></exception>
        // F -> N | (E)
        private Lexem Factor(bool addToStack = false)
        {
            Lexem next = Lexems[pos];
            Lexem result;
            if (next.Content == "(")
            {
                pos++;
                do
                {
                    // если выражение в скобках, то рекурсивно переходим на обработку подвыражения типа Е
                    result = Expression();
                    Lexem closingBracket;
                    if (pos < Lexems.Count)
                        closingBracket = Lexems[pos];
                    else
                        throw new Exception("Выражение неожиданно закончилось");
                    if ((pos < Lexems.Count) && (closingBracket.Content == ")"))
                    {
                        pos++;
                        if (addToStack) stack.Push(result);
                        return result;
                    }
                    if ((pos < Lexems.Count) && (closingBracket.Content == ","))
                    {
                        pos++;
                        if (addToStack) stack.Push(result);
                    }
                    else
                        throw new Exception("ожидалась ')', но пришло: " + closingBracket);
                }while (pos < Lexems.Count);
            }
            pos++;
            // в противном случае токен должен быть числом
            return next;
        }
    }

    public class RecursiveDescentParser
    {
        private String[] tokens;
        private int pos = 0; // индекс текущего токена

        public RecursiveDescentParser(String[] tokens)
        {
            this.tokens = tokens;
        }

        public Double Parse()
        {
            double result = Expression();
            if (pos != tokens.Length)
                throw new Exception("Ошибка в выражении на позиции: " + tokens[pos]);
            return result;
        }

        // E -> T±T±T±T± ... ±T
        private Double Expression()
        {
            // находим первое слагаемое
            Double first = Term();

            while (pos < tokens.Length)
            {
                String aOperator = tokens[pos];
                if (!aOperator.Equals("+") && !aOperator.Equals("-")) 
                    break;
                else
                    pos++;
                // находим второе слагаемое (вычитаемое)
                Double second = Term();
                if (aOperator.Equals("+")) 
                    first += second;
                else
                    first -= second;
            }
            return first;
        }

        // T -> F*/F*/F*/*/ ... */F
        private Double Term()
        {
            // находим первый множитель
            Double first = Factor();
            while (pos < tokens.Length)
            {
                String aOperator = tokens[pos];
                if (!aOperator.Equals("*") && !aOperator.Equals("/")) 
                    break;
                else
                    pos++;
                // находим второй множитель (делитель)
                Double second = Factor();
                if (aOperator.Equals("*")) 
                    first *= second;
                else
                    first /= second;
            }
            return first;
        }

        // F -> N | (E)
        private Double Factor()
        {
            String next = tokens[pos];
            Double result;
            if (next.Equals("("))
            {
                pos++;
                // если выражение в скобках, то рекурсивно переходим на обработку подвыражения типа Е
                result = Expression();
                String closingBracket;
                if (pos < tokens.Length)
                    closingBracket = tokens[pos];
                else
                    throw new Exception("Выражение неожиданно закончилось");
                if (pos < tokens.Length && closingBracket.Equals(")"))
                {
                    pos++;
                    return result;
                }
                throw new Exception("ожидалась ')', но пришло: " + closingBracket);
            }
            pos++;
            // в противном случае токен должен быть числом
            return Double.Parse(next);
        }
    }
}
