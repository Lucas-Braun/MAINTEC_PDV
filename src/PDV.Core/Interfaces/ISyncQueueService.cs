namespace PDV.Core.Interfaces;

public interface ISyncQueueService : IDisposable
{
    void Iniciar();
    void Parar();
    Task<int> SincronizarAgora();
    int VendasPendentes { get; }
    event Action<int>? PendentesAlterados;
}
