using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SQLite;
using WayPrecision.Domain.Models;

namespace WayPrecision.Domain.Data
{
    public class DatabaseContext
    {
        public SQLiteAsyncConnection Connection { get; }

        public DatabaseContext(string dbPath, string sqlContent = "")
        {
            bool dbExists = File.Exists(dbPath);

            Connection = new SQLiteAsyncConnection(dbPath);
            Connection.CreateTableAsync<Configuration>().Wait();
            Connection.CreateTableAsync<Track>().Wait();
            Connection.CreateTableAsync<Position>().Wait();
            Connection.CreateTableAsync<TrackPoint>().Wait();
            Connection.CreateTableAsync<Waypoint>().Wait();

            if (!dbExists && !string.IsNullOrWhiteSpace(sqlContent))
                InitializeFromSqlFileAsync(sqlContent).Wait();
        }

        /// <summary>
        /// Lee un fichero .sql y ejecuta todas las sentencias encontradas.
        /// Ejecuta todas las sentencias dentro de una única transacción para seguridad.
        /// </summary>
        /// <param name="sqlFilePath">Ruta del fichero .sql</param>
        /// <param name="cancellationToken">Token para cancelación opcional</param>
        public async Task InitializeFromSqlFileAsync(string sqlContent, CancellationToken cancellationToken = default)
        {
            // Eliminar comentarios /* ... */ multilínea
            sqlContent = Regex.Replace(sqlContent, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

            // Quitar comentarios de línea que comienzan con --
            var lines = sqlContent
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Select(l =>
                {
                    var index = l.IndexOf("--", StringComparison.Ordinal);
                    return index >= 0 ? l.Substring(0, index) : l;
                });

            var cleaned = string.Join("\n", lines);

            // Separar por ';' pero evitando los que están dentro de comillas simples
            var rawStatements = Regex.Split(cleaned, @";(?=(?:[^']*'[^']*')*[^']*$)");

            var statements = rawStatements
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();

            if (!statements.Any())
                return;

            // Ejecutar todas las sentencias dentro de una transacción
            await Connection.RunInTransactionAsync(conn =>
            {
                foreach (var stmt in statements)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    conn.Execute(stmt);
                }
            }).ConfigureAwait(false);
        }
    }
}