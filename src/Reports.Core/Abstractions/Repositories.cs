using Reports.Core.Domain;

namespace Reports.Core.Abstractions;

public interface IReferenceRepository
{
    Task<IReadOnlyList<UavType>> GetUavTypesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TargetType>> GetTargetTypesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MunitionType>> GetMunitionTypesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Settlement>> GetSettlementsAsync(CancellationToken ct = default);

    Task SaveAsync<T>(T item, CancellationToken ct = default) where T : ReferenceItem;
    Task DeleteAsync<T>(T item, CancellationToken ct = default) where T : ReferenceItem;
}

public interface IPersonRepository
{
    Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken ct = default);
    Task<Person?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task SaveAsync(Person person, CancellationToken ct = default);
    Task DeleteAsync(Person person, CancellationToken ct = default);
}

public interface IMissionRepository
{
    Task<IReadOnlyList<Mission>> GetAllAsync(CancellationToken ct = default);

    Task<Mission?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<IReadOnlySet<string>> GetImportedKeysAsync(CancellationToken ct = default);

    Task AddRangeAsync(IEnumerable<Mission> missions, CancellationToken ct = default);
    Task SaveAsync(Mission mission, CancellationToken ct = default);
}

public interface IJournalRepository
{
    Task<IReadOnlyList<JournalEntry>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(JournalEntry entry, CancellationToken ct = default);
}

public interface IUserRepository
{
    Task<AppUser?> FindByUsernameAsync(string username, CancellationToken ct = default);
    Task AddAsync(AppUser user, CancellationToken ct = default);
    Task UpdatePasswordHashAsync(int userId, string newHash, CancellationToken ct = default);
    Task<bool> AnyAsync(CancellationToken ct = default);
}
