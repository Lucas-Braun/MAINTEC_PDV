using System.Collections.Concurrent;

namespace PDV.Infrastructure.Services;

public class PdvLogger : IDisposable
{
    private readonly string _logDir;
    private readonly ConcurrentQueue<string> _queue = new();
    private readonly Timer _flushTimer;
    private bool _disposed;

    public PdvLogger(string? logDir = null)
    {
        _logDir = logDir ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PDV", "logs");
        Directory.CreateDirectory(_logDir);

        // Flush a cada 2 segundos
        _flushTimer = new Timer(_ => Flush(), null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));

        // Limpa logs antigos (>30 dias)
        LimparLogsAntigos(30);
    }

    public void Info(string mensagem) => Enqueue("INFO", mensagem);
    public void Warn(string mensagem) => Enqueue("WARN", mensagem);
    public void Erro(string mensagem) => Enqueue("ERRO", mensagem);
    public void Erro(string mensagem, Exception ex) => Enqueue("ERRO", $"{mensagem} | {ex.Message}");

    public void Operacao(string operador, string acao, string? detalhe = null)
    {
        var msg = $"[{operador}] {acao}";
        if (!string.IsNullOrEmpty(detalhe))
            msg += $" | {detalhe}";
        Enqueue("OPER", msg);
    }

    private void Enqueue(string nivel, string mensagem)
    {
        var linha = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{nivel}] {mensagem}";
        _queue.Enqueue(linha);
    }

    private void Flush()
    {
        if (_queue.IsEmpty) return;

        try
        {
            var arquivo = Path.Combine(_logDir, $"pdv_{DateTime.Now:yyyyMMdd}.log");
            var linhas = new List<string>();

            while (_queue.TryDequeue(out var linha))
                linhas.Add(linha);

            if (linhas.Count > 0)
                File.AppendAllLines(arquivo, linhas);
        }
        catch { /* nao pode falhar */ }
    }

    private void LimparLogsAntigos(int diasRetencao)
    {
        try
        {
            var limite = DateTime.Now.AddDays(-diasRetencao);
            foreach (var arquivo in Directory.GetFiles(_logDir, "pdv_*.log"))
            {
                if (File.GetCreationTime(arquivo) < limite)
                    File.Delete(arquivo);
            }
        }
        catch { }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _flushTimer.Dispose();
            Flush(); // flush final
            _disposed = true;
        }
    }
}
