using library.Models;
using Npgsql;


namespace library.Data
{
    public class BookRepository
    {
        private readonly NpgsqlDataSource _dataSource;

        private static Book ReadBook(NpgsqlDataReader reader)
        {
            return new Book
            {
                BookId = reader.GetInt64(0),
                Title = reader.GetString(1),
                Category = reader.IsDBNull(2) ? null : reader.GetString(2),   // NULL becomes null
                Price = reader.IsDBNull(3) ? null : reader.GetDecimal(3)
            };
        }

        public async Task<List<Book>> GetAllAsync()
        {
            const string sql = "SELECT book_id, title, category, price FROM lending.book ORDER BY title;";

            var books = new List<Book>();

            // The command borrows a connection; 'await using' gives it back when the method ends.
            await using var command = _dataSource.CreateCommand(sql);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())        // once for each row
            {
                books.Add(ReadBook(reader));
            }

            return books;
        }


        public BookRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    }
}
