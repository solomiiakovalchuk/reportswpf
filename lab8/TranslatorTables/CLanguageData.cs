namespace TranslatorTables;

/// <summary>
/// Дані для ініціалізації таблиць транслятора з мови СІ.
/// Обсяг узятий за вимогами роботи: 25 ключових слів (серед них — усі,
/// пов'язані з описами), усі вбудовані типи, усі операції мови,
/// 8 бібліотечних процедур.
/// </summary>
public static class CLanguageData
{
    /// <summary>
    /// 25 найуживаніших ключових слів. Слова описів (int, char, struct,
    /// typedef, const, …) присутні обов'язково — саме вони заповнюють
    /// таблицю ідентифікаторів під час трансляції описової частини.
    /// </summary>
    public static IReadOnlyList<IdentifierRecord> Keywords { get; } = new[]
    {
        // Описи типів і класів пам'яті
        new IdentifierRecord("int",      "int",  IdentifierKind.Keyword),
        new IdentifierRecord("char",     "char", IdentifierKind.Keyword),
        new IdentifierRecord("float",    "float", IdentifierKind.Keyword),
        new IdentifierRecord("double",   "double", IdentifierKind.Keyword),
        new IdentifierRecord("void",     "void", IdentifierKind.Keyword),
        new IdentifierRecord("short",    "short", IdentifierKind.Keyword),
        new IdentifierRecord("long",     "long", IdentifierKind.Keyword),
        new IdentifierRecord("signed",   "int",  IdentifierKind.Keyword),
        new IdentifierRecord("unsigned", "unsigned int", IdentifierKind.Keyword),
        new IdentifierRecord("const",    "void", IdentifierKind.Keyword),
        new IdentifierRecord("static",   "void", IdentifierKind.Keyword),
        new IdentifierRecord("extern",   "void", IdentifierKind.Keyword),
        new IdentifierRecord("register", "void", IdentifierKind.Keyword),
        new IdentifierRecord("auto",     "void", IdentifierKind.Keyword),
        new IdentifierRecord("volatile", "void", IdentifierKind.Keyword),
        new IdentifierRecord("struct",   "void", IdentifierKind.Keyword),
        new IdentifierRecord("union",    "void", IdentifierKind.Keyword),
        new IdentifierRecord("enum",     "void", IdentifierKind.Keyword),
        new IdentifierRecord("typedef",  "void", IdentifierKind.Keyword),
        new IdentifierRecord("sizeof",   "unsigned int", IdentifierKind.Keyword),

        // Керування виконанням
        new IdentifierRecord("if",     "void", IdentifierKind.Keyword),
        new IdentifierRecord("else",   "void", IdentifierKind.Keyword),
        new IdentifierRecord("for",    "void", IdentifierKind.Keyword),
        new IdentifierRecord("while",  "void", IdentifierKind.Keyword),
        new IdentifierRecord("return", "void", IdentifierKind.Keyword)
    };

    /// <summary>Усі вбудовані типи мови СІ з їхніми розмірами.</summary>
    public static IReadOnlyList<TypeRecord> Types { get; } = new[]
    {
        new TypeRecord("char",               1, true,  TypeCategory.Character),
        new TypeRecord("signed char",        1, true,  TypeCategory.Character),
        new TypeRecord("unsigned char",      1, false, TypeCategory.Character),
        new TypeRecord("short",              2, true,  TypeCategory.Integer),
        new TypeRecord("unsigned short",     2, false, TypeCategory.Integer),
        new TypeRecord("int",                4, true,  TypeCategory.Integer),
        new TypeRecord("unsigned int",       4, false, TypeCategory.Integer),
        new TypeRecord("long",               4, true,  TypeCategory.Integer),
        new TypeRecord("unsigned long",      4, false, TypeCategory.Integer),
        new TypeRecord("long long",          8, true,  TypeCategory.Integer),
        new TypeRecord("unsigned long long", 8, false, TypeCategory.Integer),
        new TypeRecord("float",              4, true,  TypeCategory.Real),
        new TypeRecord("double",             8, true,  TypeCategory.Real),
        new TypeRecord("long double",       16, true,  TypeCategory.Real),
        new TypeRecord("void",               0, false, TypeCategory.Void),
        new TypeRecord("pointer",            8, false, TypeCategory.Pointer)
    };

    /// <summary>Операції мови СІ з пріоритетами за стандартом.</summary>
    public static IReadOnlyList<OperationRecord> Operations { get; } = new[]
    {
        new OperationRecord("()",  15, OperationArity.Unary,  true,  "виклик функції"),
        new OperationRecord("[]",  15, OperationArity.Binary, true,  "індексація масиву"),
        new OperationRecord(".",   15, OperationArity.Binary, true,  "доступ до поля"),
        new OperationRecord("->",  15, OperationArity.Binary, true,  "доступ до поля через вказівник"),
        new OperationRecord("++",  14, OperationArity.Unary,  false, "інкремент"),
        new OperationRecord("--",  14, OperationArity.Unary,  false, "декремент"),
        new OperationRecord("!",   14, OperationArity.Unary,  false, "логічне НЕ"),
        new OperationRecord("~",   14, OperationArity.Unary,  false, "побітове доповнення"),
        new OperationRecord("&",   14, OperationArity.Unary,  false, "взяття адреси"),
        new OperationRecord("*",   13, OperationArity.Binary, true,  "множення"),
        new OperationRecord("/",   13, OperationArity.Binary, true,  "ділення"),
        new OperationRecord("%",   13, OperationArity.Binary, true,  "залишок від ділення"),
        new OperationRecord("+",   12, OperationArity.Binary, true,  "додавання"),
        new OperationRecord("-",   12, OperationArity.Binary, true,  "віднімання"),
        new OperationRecord("<<",  11, OperationArity.Binary, true,  "зсув ліворуч"),
        new OperationRecord(">>",  11, OperationArity.Binary, true,  "зсув праворуч"),
        new OperationRecord("<",   10, OperationArity.Binary, true,  "менше"),
        new OperationRecord("<=",  10, OperationArity.Binary, true,  "менше або дорівнює"),
        new OperationRecord(">",   10, OperationArity.Binary, true,  "більше"),
        new OperationRecord(">=",  10, OperationArity.Binary, true,  "більше або дорівнює"),
        new OperationRecord("==",   9, OperationArity.Binary, true,  "дорівнює"),
        new OperationRecord("!=",   9, OperationArity.Binary, true,  "не дорівнює"),
        new OperationRecord("&b",   8, OperationArity.Binary, true,  "побітове І"),
        new OperationRecord("^",    7, OperationArity.Binary, true,  "побітове виключне АБО"),
        new OperationRecord("|",    6, OperationArity.Binary, true,  "побітове АБО"),
        new OperationRecord("&&",   5, OperationArity.Binary, true,  "логічне І"),
        new OperationRecord("||",   4, OperationArity.Binary, true,  "логічне АБО"),
        new OperationRecord("?:",   3, OperationArity.Ternary, false, "умовна операція"),
        new OperationRecord("=",    2, OperationArity.Binary, false, "присвоєння"),
        new OperationRecord("+=",   2, OperationArity.Binary, false, "додавання з присвоєнням"),
        new OperationRecord("-=",   2, OperationArity.Binary, false, "віднімання з присвоєнням"),
        new OperationRecord("*=",   2, OperationArity.Binary, false, "множення з присвоєнням"),
        new OperationRecord("/=",   2, OperationArity.Binary, false, "ділення з присвоєнням"),
        new OperationRecord("%=",   2, OperationArity.Binary, false, "залишок з присвоєнням"),
        new OperationRecord(",",    1, OperationArity.Binary, true,  "послідовне обчислення")
    };

    /// <summary>Константи, що трапилися у тексті програми.</summary>
    public static IReadOnlyList<ConstantRecord> Constants { get; } = new[]
    {
        new ConstantRecord("C_0",      "int",    "0"),
        new ConstantRecord("C_1",      "int",    "1"),
        new ConstantRecord("C_100",    "int",    "100"),
        new ConstantRecord("C_3_14",   "double", "3.14159"),
        new ConstantRecord("C_0x1F",   "int",    "0x1F"),
        new ConstantRecord("C_EPS",    "double", "1e-9"),
        new ConstantRecord("C_NL",     "char",   "'\\n'"),
        new ConstantRecord("C_HELLO",  "pointer", "\"Hello, world\"")
    };

    /// <summary>
    /// Мітки. Мітка <c>cleanup</c> лише використана у goto, але не оголошена —
    /// саме такі випадки транслятор і виявляє за таблицею міток.
    /// </summary>
    public static IReadOnlyList<LabelRecord> Labels { get; } = new[]
    {
        new LabelRecord("start",   12, true),
        new LabelRecord("loop",    28, true),
        new LabelRecord("finish",  57, true),
        new LabelRecord("error",   64, true),
        new LabelRecord("cleanup", 71, false)
    };

    /// <summary>Вісім процедур: сім бібліотечних і точка входу програми.</summary>
    public static IReadOnlyList<ProcedureRecord> Procedures { get; } = new[]
    {
        new ProcedureRecord("main",   "int",     new[] { "int", "pointer" },          false),
        new ProcedureRecord("printf", "int",     new[] { "pointer" },                 true),
        new ProcedureRecord("scanf",  "int",     new[] { "pointer" },                 true),
        new ProcedureRecord("malloc", "pointer", new[] { "unsigned int" },            true),
        new ProcedureRecord("free",   "void",    new[] { "pointer" },                 true),
        new ProcedureRecord("strlen", "unsigned int", new[] { "pointer" },            true),
        new ProcedureRecord("strcpy", "pointer", new[] { "pointer", "pointer" },      true),
        new ProcedureRecord("fopen",  "pointer", new[] { "pointer", "pointer" },      true)
    };

    /// <summary>Ідентифікатори користувача — доповнюють таблицю під час трансляції.</summary>
    public static IReadOnlyList<IdentifierRecord> UserIdentifiers { get; } = new[]
    {
        new IdentifierRecord("counter",   "int",     IdentifierKind.Variable,  "main", 0),
        new IdentifierRecord("total",     "double",  IdentifierKind.Variable,  "main", 8),
        new IdentifierRecord("buffer",    "pointer", IdentifierKind.Variable,  "main", 16),
        new IdentifierRecord("length",    "unsigned int", IdentifierKind.Variable, "main", 24),
        new IdentifierRecord("index",     "int",     IdentifierKind.Variable,  "main", 28),
        new IdentifierRecord("argc",      "int",     IdentifierKind.Parameter, "main", 32),
        new IdentifierRecord("argv",      "pointer", IdentifierKind.Parameter, "main", 40),
        new IdentifierRecord("threshold", "double",  IdentifierKind.Variable,  "",     48)
    };
}
