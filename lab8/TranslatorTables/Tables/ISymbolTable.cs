namespace TranslatorTables.Tables;

/// <summary>
/// Таблиця транслятора: множина записів із пошуком і додаванням за ключем.
/// <para>
/// Реалізації рахують кількість порівнянь ключів — саме цим і вимірюється
/// ефективність організації таблиці, а не часом на конкретній машині.
/// </para>
/// </summary>
public interface ISymbolTable
{
    /// <summary>Назва способу організації для звіту.</summary>
    string Organization { get; }

    int Count { get; }

    /// <summary>Кількість порівнянь ключів від останнього скидання лічильника.</summary>
    long Comparisons { get; }

    void ResetComparisons();

    /// <summary>Додає запис. Повертає false, якщо ключ уже є (повторне оголошення).</summary>
    bool Add(TableRecord record);

    /// <summary>Шукає запис за ключем; null, якщо не знайдено.</summary>
    TableRecord? Find(string key);

    /// <summary>Усі записи — для друку вмісту таблиці.</summary>
    IEnumerable<TableRecord> All();
}

/// <summary>Спільна основа: лічильник порівнянь і порівняння ключів.</summary>
public abstract class SymbolTableBase : ISymbolTable
{
    private long _comparisons;

    public abstract string Organization { get; }
    public abstract int Count { get; }

    public long Comparisons => _comparisons;

    public void ResetComparisons() => _comparisons = 0;

    public abstract bool Add(TableRecord record);
    public abstract TableRecord? Find(string key);
    public abstract IEnumerable<TableRecord> All();

    /// <summary>Порівняння ключів із урахуванням у лічильнику.</summary>
    protected int Compare(string left, string right)
    {
        _comparisons++;
        return string.CompareOrdinal(left, right);
    }

    protected bool KeysEqual(string left, string right) => Compare(left, right) == 0;
}
