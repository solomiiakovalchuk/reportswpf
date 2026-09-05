namespace TranslatorTables.Tables;

/// <summary>
/// Перемішана таблиця з ланцюжками переповнення: кожна комірка тримає
/// список записів, що дали однаковий хеш.
/// <para>
/// Колізії не витісняють сусідів, тому таблиця не «деградує» при заповненні
/// й може містити більше записів, ніж комірок.
/// </para>
/// </summary>
public sealed class HashChainTable : SymbolTableBase
{
    private readonly List<TableRecord>?[] _buckets;
    private readonly IHashFunction _hash;
    private int _count;

    public HashChainTable(int size, IHashFunction hash)
    {
        _buckets = new List<TableRecord>?[size];
        _hash = hash;
    }

    public override string Organization => $"Перемішана, ланцюжки переповнення ({_hash.Name})";
    public override int Count => _count;

    public int Size => _buckets.Length;

    /// <summary>Скільки комірок реально зайнято — показник рівномірності хешу.</summary>
    public int UsedBuckets => _buckets.Count(b => b is { Count: > 0 });

    /// <summary>Найдовший ланцюжок: що він більший, то гірший розподіл.</summary>
    public int LongestChain => _buckets.Max(b => b?.Count ?? 0);

    public override bool Add(TableRecord record)
    {
        int index = _hash.Hash(record.Key, _buckets.Length);
        var bucket = _buckets[index] ??= new List<TableRecord>();

        foreach (var existing in bucket)
            if (KeysEqual(existing.Key, record.Key))
                return false;

        bucket.Add(record);
        _count++;
        return true;
    }

    public override TableRecord? Find(string key)
    {
        var bucket = _buckets[_hash.Hash(key, _buckets.Length)];
        if (bucket is null) return null;

        foreach (var record in bucket)
            if (KeysEqual(record.Key, key))
                return record;

        return null;
    }

    public override IEnumerable<TableRecord> All()
    {
        foreach (var bucket in _buckets)
        {
            if (bucket is null) continue;
            foreach (var record in bucket) yield return record;
        }
    }
}

/// <summary>
/// Перемішана таблиця з відкритою адресацією і лінійним випробуванням:
/// при колізії запис кладеться у першу вільну комірку далі за списком.
/// <para>
/// Немає додаткової пам'яті під списки, але при заповненні понад ~70 %
/// починається кластеризація, і пошук різко дорожчає. Тому таблиця
/// відмовляє у додаванні, коли вільних комірок не лишилося.
/// </para>
/// </summary>
public sealed class HashOpenAddressTable : SymbolTableBase
{
    private readonly TableRecord?[] _cells;
    private readonly IHashFunction _hash;
    private int _count;

    public HashOpenAddressTable(int size, IHashFunction hash)
    {
        _cells = new TableRecord?[size];
        _hash = hash;
    }

    public override string Organization => $"Перемішана, відкрита адресація ({_hash.Name})";
    public override int Count => _count;

    public int Size => _cells.Length;

    public double LoadFactor => (double)_count / _cells.Length;

    /// <summary>Скільки записів лягло не у «свою» комірку.</summary>
    public int Displaced { get; private set; }

    public override bool Add(TableRecord record)
    {
        if (_count == _cells.Length) return false;

        int home = _hash.Hash(record.Key, _cells.Length);

        for (int probe = 0; probe < _cells.Length; probe++)
        {
            int index = (home + probe) % _cells.Length;
            var cell = _cells[index];

            if (cell is null)
            {
                _cells[index] = record;
                _count++;
                if (probe > 0) Displaced++;
                return true;
            }

            if (KeysEqual(cell.Key, record.Key)) return false;
        }

        return false;
    }

    public override TableRecord? Find(string key)
    {
        int home = _hash.Hash(key, _cells.Length);

        for (int probe = 0; probe < _cells.Length; probe++)
        {
            int index = (home + probe) % _cells.Length;
            var cell = _cells[index];

            // Порожня комірка означає, що далі шукати нема сенсу:
            // при вставці запис зупинився б саме тут.
            if (cell is null) return null;

            if (KeysEqual(cell.Key, key)) return cell;
        }

        return null;
    }

    public override IEnumerable<TableRecord> All()
    {
        foreach (var cell in _cells)
            if (cell is not null) yield return cell;
    }
}
