namespace TranslatorTables.Tables;

/// <summary>
/// Невпорядкована таблиця: записи лежать у порядку надходження,
/// пошук — послідовний перебір.
/// <para>Додавання O(1) за місцем вставки, пошук O(n).</para>
/// </summary>
public sealed class UnorderedTable : SymbolTableBase
{
    private readonly List<TableRecord> _records = new();

    public override string Organization => "Невпорядкована (лінійний пошук)";
    public override int Count => _records.Count;

    public override bool Add(TableRecord record)
    {
        // Перевірка на повторне оголошення теж коштує перебору.
        if (Find(record.Key) is not null) return false;

        _records.Add(record);
        return true;
    }

    public override TableRecord? Find(string key)
    {
        foreach (var record in _records)
            if (KeysEqual(record.Key, key))
                return record;

        return null;
    }

    public override IEnumerable<TableRecord> All() => _records;
}

/// <summary>
/// Упорядкована таблиця: записи тримаються відсортованими за ключем,
/// пошук — двійковий.
/// <para>Пошук O(log n), але вставка O(n) через зсув хвоста масиву.</para>
/// </summary>
public sealed class OrderedTable : SymbolTableBase
{
    private readonly List<TableRecord> _records = new();

    public override string Organization => "Упорядкована (двійковий пошук)";
    public override int Count => _records.Count;

    public override bool Add(TableRecord record)
    {
        int index = LowerBound(record.Key, out bool exists);
        if (exists) return false;

        _records.Insert(index, record);
        return true;
    }

    public override TableRecord? Find(string key)
    {
        int index = LowerBound(key, out bool exists);
        return exists ? _records[index] : null;
    }

    /// <summary>Двійковий пошук позиції вставки; exists — чи ключ уже присутній.</summary>
    private int LowerBound(string key, out bool exists)
    {
        int low = 0, high = _records.Count - 1;
        exists = false;

        while (low <= high)
        {
            int middle = low + (high - low) / 2;
            int order = Compare(_records[middle].Key, key);

            if (order == 0)
            {
                exists = true;
                return middle;
            }

            if (order < 0) low = middle + 1;
            else high = middle - 1;
        }

        return low;
    }

    public override IEnumerable<TableRecord> All() => _records;
}

/// <summary>
/// Деревоподібна (бінарне дерево пошуку) таблиця.
/// <para>
/// Середній пошук O(log n) без зсувів при вставці. Слабке місце — вхід,
/// уже впорядкований за алфавітом: дерево вироджується у список,
/// і пошук стає O(n). У тестах це видно на порівнянні з упорядкованою таблицею.
/// </para>
/// </summary>
public sealed class BinaryTreeTable : SymbolTableBase
{
    private sealed class Node
    {
        public Node(TableRecord record) => Record = record;

        public TableRecord Record { get; }
        public Node? Left { get; set; }
        public Node? Right { get; set; }
    }

    private Node? _root;
    private int _count;

    public override string Organization => "Деревоподібна (бінарне дерево пошуку)";
    public override int Count => _count;

    /// <summary>Висота дерева — показник виродження.</summary>
    public int Height => HeightOf(_root);

    public override bool Add(TableRecord record)
    {
        if (_root is null)
        {
            _root = new Node(record);
            _count++;
            return true;
        }

        var current = _root;
        while (true)
        {
            int order = Compare(record.Key, current.Record.Key);
            if (order == 0) return false;

            if (order < 0)
            {
                if (current.Left is null)
                {
                    current.Left = new Node(record);
                    _count++;
                    return true;
                }
                current = current.Left;
            }
            else
            {
                if (current.Right is null)
                {
                    current.Right = new Node(record);
                    _count++;
                    return true;
                }
                current = current.Right;
            }
        }
    }

    public override TableRecord? Find(string key)
    {
        var current = _root;
        while (current is not null)
        {
            int order = Compare(key, current.Record.Key);
            if (order == 0) return current.Record;
            current = order < 0 ? current.Left : current.Right;
        }

        return null;
    }

    /// <summary>Симетричний обхід дає записи у порядку зростання ключа.</summary>
    public override IEnumerable<TableRecord> All()
    {
        var stack = new Stack<Node>();
        var current = _root;

        while (current is not null || stack.Count > 0)
        {
            while (current is not null)
            {
                stack.Push(current);
                current = current.Left;
            }

            current = stack.Pop();
            yield return current.Record;
            current = current.Right;
        }
    }

    private static int HeightOf(Node? node) =>
        node is null ? 0 : 1 + Math.Max(HeightOf(node.Left), HeightOf(node.Right));
}
