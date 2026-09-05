using System.Text;
using TranslatorTables;
using TranslatorTables.Tables;

Console.OutputEncoding = Encoding.UTF8;

// Лабораторна робота №8 — Організація таблиць у трансляторах і робота з ними.
// Мова, з якої транслюємо: СІ.
// Реалізовано всі шість таблиць і всі способи організації, щоб можна було
// порівняти їх між собою на однакових даних.

Header("ЛАБОРАТОРНА РОБОТА №8. Таблиці транслятора з мови СІ");

// ---------------------------------------------------------------- 1
Header("1. Ініціалізація таблиць");

var identifiers = Fill(new HashChainTable(64, HashFunctions.Default),
    CLanguageData.Keywords.Concat<TableRecord>(CLanguageData.UserIdentifiers));
var types = Fill(new OrderedTable(), CLanguageData.Types);
var constants = Fill(new OrderedTable(), CLanguageData.Constants);
var operations = Fill(new HashChainTable(64, HashFunctions.Default), CLanguageData.Operations);
var labels = Fill(new BinaryTreeTable(), CLanguageData.Labels);
var procedures = Fill(new OrderedTable(), CLanguageData.Procedures);

Console.WriteLine($"  Ідентифікатори : {identifiers.Count,3} записів  ({identifiers.Organization})");
Console.WriteLine($"  Типи           : {types.Count,3} записів  ({types.Organization})");
Console.WriteLine($"  Константи      : {constants.Count,3} записів  ({constants.Organization})");
Console.WriteLine($"  Операції       : {operations.Count,3} записів  ({operations.Organization})");
Console.WriteLine($"  Мітки          : {labels.Count,3} записів  ({labels.Organization})");
Console.WriteLine($"  Процедури      : {procedures.Count,3} записів  ({procedures.Organization})");

// ---------------------------------------------------------------- 2
Header("2. Вміст таблиць");

PrintTable("Типи", types);
PrintTable("Процедури", procedures);
PrintTable("Мітки", labels);
Console.WriteLine("  Операції (перші 10 за пріоритетом):");
foreach (var op in operations.All().Cast<OperationRecord>()
                              .OrderByDescending(o => o.Priority).Take(10))
    Console.WriteLine($"    {op}");

// ---------------------------------------------------------------- 3
Header("3. Зв'язки між таблицями");

Console.WriteLine("  Ідентифікатор → тип → розмір у пам'яті:");
foreach (var name in new[] { "counter", "total", "buffer", "threshold" })
{
    var id = (IdentifierRecord?)identifiers.Find(name);
    var type = id is null ? null : (TypeRecord?)types.Find(id.TypeKey);
    Console.WriteLine($"    {name,-10} → {id?.TypeKey,-13} → " +
                      $"{(type is null ? "тип не знайдено" : type.SizeInBytes + " байт")}");
}

Console.WriteLine("\n  Процедура → типи параметрів:");
foreach (var proc in procedures.All().Cast<ProcedureRecord>().Take(4))
{
    var sizes = proc.ParameterTypes
        .Select(t => ((TypeRecord?)types.Find(t))?.SizeInBytes ?? 0);
    Console.WriteLine($"    {proc.Key,-8} параметрів {proc.ParameterCount}, " +
                      $"сумарно {sizes.Sum()} байт");
}

var undefined = labels.All().Cast<LabelRecord>().Where(l => !l.IsDefined).ToList();
Console.WriteLine($"\n  Контроль міток: невизначених — {undefined.Count}");
foreach (var label in undefined)
    Console.WriteLine($"    ПОМИЛКА: мітка «{label.Key}» використана в рядку {label.Line}, але не оголошена");

// ---------------------------------------------------------------- 4
Header("4. Тестові дані й контроль правильності операцій");

var testKeys = new (string Key, bool Expected)[]
{
    ("int",       true),   // ключове слово
    ("typedef",   true),   // ключове слово опису
    ("counter",   true),   // змінна користувача
    ("argv",      true),   // параметр функції
    ("Int",       false),  // інший регістр — інший ідентифікатор
    ("intx",      false),  // спільний префікс із наявним ключем
    ("printf",    false),  // є у таблиці процедур, а не ідентифікаторів
    ("undefined", false)
};

int failures = 0;
foreach (var (key, expected) in testKeys)
{
    bool found = identifiers.Find(key) is not null;
    bool ok = found == expected;
    if (!ok) failures++;

    Console.WriteLine($"  [{(ok ? "OK" : "ПОМИЛКА")}] пошук «{key}»: " +
                      $"очікувалося {(expected ? "знайдено" : "не знайдено")}, " +
                      $"отримано {(found ? "знайдено" : "не знайдено")}");
}

Console.WriteLine("\n  Повторне оголошення:");
bool addedAgain = identifiers.Add(new IdentifierRecord("counter", "int", IdentifierKind.Variable));
Console.WriteLine($"  [{(!addedAgain ? "OK" : "ПОМИЛКА")}] додавання наявного ключа «counter» " +
                  $"{(addedAgain ? "пройшло — дубль у таблиці" : "відхилено, як і має бути")}");
if (addedAgain) failures++;

// ---------------------------------------------------------------- 5
Header("5. Порівняння способів організації таблиці");

var records = CLanguageData.Keywords.Concat<TableRecord>(CLanguageData.UserIdentifiers).ToList();
var lookups = records.Select(r => r.Key)
                     .Concat(new[] { "missing1", "missing2", "Int", "intx" })
                     .ToList();

Console.WriteLine($"  Записів у таблиці: {records.Count}, пошуків: {lookups.Count}\n");
Console.WriteLine($"  {"Спосіб організації",-52} {"Порівнянь",10} {"На пошук",10}");
Console.WriteLine("  " + new string('-', 74));

foreach (var table in BuildAllOrganizations())
{
    foreach (var record in records) table.Add(record);

    table.ResetComparisons();
    foreach (var key in lookups) table.Find(key);

    double average = (double)table.Comparisons / lookups.Count;
    Console.WriteLine($"  {table.Organization,-52} {table.Comparisons,10} {average,10:F2}");
}

Console.WriteLine("\n  Висновок: лінійний пошук по невпорядкованій таблиці найдорожчий;");
Console.WriteLine("  двійковий пошук і дерево дають логарифмічну оцінку; перемішана");
Console.WriteLine("  таблиця виходить на майже одне порівняння за пошук.");

var tree = new BinaryTreeTable();
foreach (var record in records.OrderBy(r => r.Key, StringComparer.Ordinal)) tree.Add(record);
Console.WriteLine($"\n  Виродження дерева: при впорядкованому вході висота = {tree.Height} " +
                  $"при {tree.Count} записах (пошук стає лінійним).");

// ---------------------------------------------------------------- 6
Header("6. Порівняння хеш-функцій для таблиці імен");

const int TableSize = 64;
var keys = records.Select(r => r.Key).ToList();

Console.WriteLine($"  Розмір таблиці: {TableSize}, ключів: {keys.Count}\n");
Console.WriteLine($"  {"Хеш-функція",-28} {"Зайнято",8} {"Колізій",8} {"Найдовший",10} {"Порівнянь",10}");
Console.WriteLine("  " + new string('-', 68));

var hashStats = new List<(string Name, int Used, int Collisions, int Longest, long Comparisons)>();

foreach (var hash in HashFunctions.All)
{
    var table = new HashChainTable(TableSize, hash);
    foreach (var record in records) table.Add(record);

    table.ResetComparisons();
    foreach (var key in keys) table.Find(key);

    int collisions = table.Count - table.UsedBuckets;
    hashStats.Add((hash.Name, table.UsedBuckets, collisions, table.LongestChain, table.Comparisons));

    Console.WriteLine($"  {hash.Name,-28} {table.UsedBuckets,8} {collisions,8} " +
                      $"{table.LongestChain,10} {table.Comparisons,10}");
}

// Висновок будується за виміряними числами, а не задається наперед:
// на іншому наборі ключів переможець буде інший.
var best = hashStats.OrderBy(s => s.Comparisons).ThenByDescending(s => s.Used).First();
var worst = hashStats.OrderByDescending(s => s.Comparisons).ThenBy(s => s.Used).First();
double spread = worst.Comparisons == 0 ? 0
    : (worst.Comparisons - best.Comparisons) * 100.0 / worst.Comparisons;

Console.WriteLine($"\n  Найкраща на цьому наборі: {best.Name} — {best.Comparisons} порівнянь, " +
                  $"зайнято {best.Used} комірок із {TableSize}.");
Console.WriteLine($"  Найгірша на цьому наборі: {worst.Name} — {worst.Comparisons} порівнянь, " +
                  $"зайнято {worst.Used}.");
Console.WriteLine($"  Розрив між ними — {spread:F1} %.");
Console.WriteLine($"  На {keys.Count} коротких ключах різниця між функціями невелика: усі дають");
Console.WriteLine($"  найдовший ланцюжок {hashStats.Max(s => s.Longest)} і близьку кількість колізій.");
Console.WriteLine("  Розрив стає показовим на тисячах ідентифікаторів зі спільними префіксами,");
Console.WriteLine("  де сума кодів символів програє: вона не враховує порядок символів,");
Console.WriteLine("  тому анаграми (abc і cba) завжди дають одну комірку.");

// Це твердження перевіряється, а не приймається на віру.
var sumHash = new SumOfCharsHash();
bool anagramCollision = sumHash.Hash("abc", TableSize) == sumHash.Hash("cba", TableSize);
var pjw = new PjwHash();
bool pjwSeparates = pjw.Hash("abc", TableSize) != pjw.Hash("cba", TableSize);
Console.WriteLine($"\n  Перевірка: сума кодів для «abc» і «cba» " +
                  $"{(anagramCollision ? "збігається (колізія)" : "різна")}; " +
                  $"PJW/ELF їх {(pjwSeparates ? "розрізняє" : "не розрізняє")}.");

Header(failures == 0
    ? "ПІДСУМОК: усі перевірки пройдено"
    : $"ПІДСУМОК: невдалих перевірок — {failures}");

return failures == 0 ? 0 : 1;

// ---------------------------------------------------------------- допоміжні

static IEnumerable<ISymbolTable> BuildAllOrganizations() => new ISymbolTable[]
{
    new UnorderedTable(),
    new OrderedTable(),
    new BinaryTreeTable(),
    new HashChainTable(64, HashFunctions.Default),
    new HashOpenAddressTable(64, HashFunctions.Default)
};

static T Fill<T>(T table, IEnumerable<TableRecord> records) where T : ISymbolTable
{
    foreach (var record in records) table.Add(record);
    return table;
}

static void PrintTable(string title, ISymbolTable table)
{
    Console.WriteLine($"  {title}:");
    foreach (var record in table.All()) Console.WriteLine($"    {record}");
    Console.WriteLine();
}

static void Header(string text)
{
    Console.WriteLine();
    Console.WriteLine(new string('=', 78));
    Console.WriteLine(text);
    Console.WriteLine(new string('=', 78));
}
