namespace TranslatorTables;

/// <summary>
/// Запис таблиці транслятора. Одне поле запису є ключем, за значенням
/// якого виконується пошук і додавання.
/// </summary>
public abstract class TableRecord
{
    protected TableRecord(string key) => Key = key;

    /// <summary>Ключове поле запису — ім'я лексеми.</summary>
    public string Key { get; }

    /// <summary>Назва таблиці, до якої належить запис.</summary>
    public abstract string TableName { get; }

    /// <summary>Опис запису для друку в тестах.</summary>
    public abstract string Describe();

    public override string ToString() => $"{Key} :: {Describe()}";
}

/// <summary>
/// Таблиця ідентифікаторів. Посилається на таблицю типів (поле TypeKey)
/// і на таблицю процедур (поле ScopeKey — у якій функції оголошено).
/// </summary>
public sealed class IdentifierRecord : TableRecord
{
    public IdentifierRecord(string key, string typeKey, IdentifierKind kind,
                            string scopeKey = "", int address = 0)
        : base(key)
    {
        TypeKey = typeKey;
        Kind = kind;
        ScopeKey = scopeKey;
        Address = address;
    }

    /// <summary>Зв'язок із таблицею типів.</summary>
    public string TypeKey { get; }

    public IdentifierKind Kind { get; }

    /// <summary>Зв'язок із таблицею процедур: область видимості.</summary>
    public string ScopeKey { get; }

    /// <summary>Зміщення у пам'яті, яке призначає транслятор.</summary>
    public int Address { get; }

    public override string TableName => "Ідентифікатори";

    public override string Describe() =>
        $"тип={TypeKey}, вид={Kind}, область={(ScopeKey.Length == 0 ? "глобальна" : ScopeKey)}, адреса={Address}";
}

public enum IdentifierKind
{
    /// <summary>Зарезервоване слово мови.</summary>
    Keyword,
    Variable,
    Parameter,
    Function
}

/// <summary>Таблиця типів: вбудовані типи мови з їхнім розміром.</summary>
public sealed class TypeRecord : TableRecord
{
    public TypeRecord(string key, int sizeInBytes, bool isSigned, TypeCategory category)
        : base(key)
    {
        SizeInBytes = sizeInBytes;
        IsSigned = isSigned;
        Category = category;
    }

    public int SizeInBytes { get; }
    public bool IsSigned { get; }
    public TypeCategory Category { get; }

    public override string TableName => "Типи";

    public override string Describe() =>
        $"розмір={SizeInBytes} б, {(IsSigned ? "знаковий" : "беззнаковий")}, категорія={Category}";
}

public enum TypeCategory
{
    Integer,
    Real,
    Character,
    Void,
    Pointer
}

/// <summary>Таблиця констант: літерали, що трапилися у тексті програми.</summary>
public sealed class ConstantRecord : TableRecord
{
    public ConstantRecord(string key, string typeKey, string value) : base(key)
    {
        TypeKey = typeKey;
        Value = value;
    }

    /// <summary>Зв'язок із таблицею типів.</summary>
    public string TypeKey { get; }

    public string Value { get; }

    public override string TableName => "Константи";

    public override string Describe() => $"тип={TypeKey}, значення={Value}";
}

/// <summary>Таблиця операцій: усі операції мови з пріоритетом і асоціативністю.</summary>
public sealed class OperationRecord : TableRecord
{
    public OperationRecord(string key, int priority, OperationArity arity,
                           bool leftAssociative, string description)
        : base(key)
    {
        Priority = priority;
        Arity = arity;
        LeftAssociative = leftAssociative;
        Description = description;
    }

    /// <summary>Пріоритет: що більший, то раніше виконується.</summary>
    public int Priority { get; }

    public OperationArity Arity { get; }
    public bool LeftAssociative { get; }
    public string Description { get; }

    public override string TableName => "Операції";

    public override string Describe() =>
        $"пріоритет={Priority}, {Arity}, {(LeftAssociative ? "ліво" : "право")}асоціативна — {Description}";
}

public enum OperationArity
{
    Unary,
    Binary,
    Ternary
}

/// <summary>
/// Таблиця міток. Мітка може зустрітися у goto раніше за своє оголошення,
/// тому запис має ознаку «визначена» і номер рядка для контролю.
/// </summary>
public sealed class LabelRecord : TableRecord
{
    public LabelRecord(string key, int line, bool isDefined) : base(key)
    {
        Line = line;
        IsDefined = isDefined;
    }

    public int Line { get; }

    /// <summary>Чи зустрілося оголошення мітки, а не лише перехід на неї.</summary>
    public bool IsDefined { get; }

    public override string TableName => "Мітки";

    public override string Describe() =>
        $"рядок={Line}, {(IsDefined ? "визначена" : "лише використана — помилка трансляції")}";
}

/// <summary>Таблиця процедур (підпрограм): сигнатури функцій.</summary>
public sealed class ProcedureRecord : TableRecord
{
    public ProcedureRecord(string key, string returnTypeKey, IReadOnlyList<string> parameterTypes,
                           bool isLibrary)
        : base(key)
    {
        ReturnTypeKey = returnTypeKey;
        ParameterTypes = parameterTypes;
        IsLibrary = isLibrary;
    }

    /// <summary>Зв'язок із таблицею типів: тип результату.</summary>
    public string ReturnTypeKey { get; }

    public IReadOnlyList<string> ParameterTypes { get; }

    public bool IsLibrary { get; }

    public int ParameterCount => ParameterTypes.Count;

    public override string TableName => "Процедури";

    public override string Describe() =>
        $"{ReturnTypeKey} {Key}({string.Join(", ", ParameterTypes)})" +
        (IsLibrary ? " — бібліотечна" : " — користувацька");
}
