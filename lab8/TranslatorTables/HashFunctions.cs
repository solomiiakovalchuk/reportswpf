namespace TranslatorTables;

/// <summary>Хеш-функція для таблиці імен.</summary>
public interface IHashFunction
{
    string Name { get; }

    /// <summary>Обчислює індекс комірки в діапазоні [0, tableSize).</summary>
    int Hash(string key, int tableSize);
}

/// <summary>
/// Найпростіша функція: сума кодів символів за модулем розміру таблиці.
/// Дешева, але дає багато колізій на схожих іменах — анаграми
/// (<c>abc</c> і <c>cba</c>) потрапляють в одну комірку.
/// </summary>
public sealed class SumOfCharsHash : IHashFunction
{
    public string Name => "Сума кодів символів";

    public int Hash(string key, int tableSize)
    {
        int sum = 0;
        foreach (char c in key) sum += c;
        return Math.Abs(sum) % tableSize;
    }
}

/// <summary>
/// Метод ділення з поліномом за основою 31 (класика для рядків).
/// Порядок символів враховується, тож анаграми розходяться.
/// </summary>
public sealed class PolynomialHash : IHashFunction
{
    private const int Base = 31;

    public string Name => "Поліноміальна за основою 31";

    public int Hash(string key, int tableSize)
    {
        unchecked
        {
            int hash = 0;
            foreach (char c in key) hash = hash * Base + c;
            return (int)((uint)hash % (uint)tableSize);
        }
    }
}

/// <summary>
/// Мультиплікативний метод Кнута: дробова частина добутку на золотий перетин.
/// Не потребує простого розміру таблиці — добре працює і зі степенем двійки.
/// </summary>
public sealed class KnuthMultiplicativeHash : IHashFunction
{
    private const uint GoldenRatio = 2654435761;

    public string Name => "Мультиплікативний (Кнут)";

    public int Hash(string key, int tableSize)
    {
        unchecked
        {
            uint hash = 0;
            foreach (char c in key) hash = (hash + c) * GoldenRatio;
            return (int)(hash % (uint)tableSize);
        }
    }
}

/// <summary>
/// PJW/ELF — функція з таблиць символів UNIX-компонувальників.
/// Розроблена саме під ідентифікатори програм, тому дає рівномірний розподіл
/// на іменах зі спільними префіксами (<c>count1</c>, <c>count2</c>, …).
/// </summary>
public sealed class PjwHash : IHashFunction
{
    public string Name => "PJW/ELF";

    public int Hash(string key, int tableSize)
    {
        unchecked
        {
            uint hash = 0;
            foreach (char c in key)
            {
                hash = (hash << 4) + c;
                uint high = hash & 0xF0000000;
                if (high != 0) hash ^= high >> 24;
                hash &= ~high;
            }
            return (int)(hash % (uint)tableSize);
        }
    }
}

/// <summary>FNV-1a — швидка й рівномірна, поширена в сучасних трансляторах.</summary>
public sealed class Fnv1aHash : IHashFunction
{
    private const uint OffsetBasis = 2166136261;
    private const uint Prime = 16777619;

    public string Name => "FNV-1a";

    public int Hash(string key, int tableSize)
    {
        unchecked
        {
            uint hash = OffsetBasis;
            foreach (char c in key)
            {
                hash ^= c;
                hash *= Prime;
            }
            return (int)(hash % (uint)tableSize);
        }
    }
}

/// <summary>Набір хеш-функцій для порівняння у звіті.</summary>
public static class HashFunctions
{
    public static IReadOnlyList<IHashFunction> All { get; } = new IHashFunction[]
    {
        new SumOfCharsHash(),
        new PolynomialHash(),
        new KnuthMultiplicativeHash(),
        new PjwHash(),
        new Fnv1aHash()
    };

    /// <summary>
    /// Функція за замовчуванням для таблиць транслятора. FNV-1a обрана
    /// за результатами вимірювань у розділі 6 програми: найменше порівнянь
    /// і найрівномірніше заповнення комірок на наборі ключів мови СІ.
    /// </summary>
    public static IHashFunction Default => All[4];   // FNV-1a
}
