using BigBooks.API.Core;
using BigBooks.API.Models;
using DataMaker;

namespace DummyConsole
{
    internal class BookHandler
    {
        private readonly List<string> _authors = new List<string>();

        public BookHandler(int authorCount)
        {
            for (int i = 0; i < authorCount; i++)
            {
                _authors.Add(RandomData.GenerateFullName());
            }
        }

        public List<BookAddUpdateDto> BuildBookDtos(int bookCount)
        {
            var bookList = new List<BookAddUpdateDto>();

            for (int i = 0; i < bookCount; i++)
            {
                var description = string.Format("{0} {1}",
                    RandomData.GenerateSentence(),
                    RandomData.GenerateSentence());

                var nextBook = new BookAddUpdateDto
                {
                    Title = RandomData.GenerateSentence(),
                    Author = RandomData.SelectFromList(_authors),
                    Isbn = Guid.NewGuid(),
                    Description = TruncateString(description, 500),
                    Genre = (Genre)RandomData.GenerateInt(1, 10),
                    Price = RandomData.GenerateDecimal(9, 31, 2),
                    StockQuantity = RandomData.GenerateInt(11, 50)
                };

                bookList.Add(nextBook);
            }

            return bookList;
        }

        private string TruncateString(string source, int max)
        {
            if (string.IsNullOrEmpty(source) || (source.Length <= max))
            {
                return source;
            }

            return source.Substring(0, max);
        }
    }
}
