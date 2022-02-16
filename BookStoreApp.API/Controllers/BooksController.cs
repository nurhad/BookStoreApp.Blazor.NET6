#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStoreApp.API.Data;
using AutoMapper;
using BookStoreApp.API.Models.Book;
using AutoMapper.QueryableExtensions;
using BookStoreApp.API.Static;

namespace BookStoreApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BookStoreDbContext _context;
        private readonly IMapper mapper;
        private readonly ILogger<BooksController> logger;

        public BooksController(BookStoreDbContext context, IMapper mapper, ILogger<BooksController> logger)
        {
            _context = context;
            this.mapper = mapper;
            this.logger = logger;
        }

        // GET: api/Books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookReadOnlyDto>>> GetBooks()
        {
            logger.LogInformation($"Request to {nameof(GetBooks)}");
            try
            {
                //return await _context.Books.ToListAsync();

                //Menggunakan DTO
                var bookDtos = await _context.Books
                    .Include(q => q.Author)
                    .ProjectTo<BookReadOnlyDto>(mapper.ConfigurationProvider)
                    .ToListAsync();
                return Ok(bookDtos);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error Performing GET in {nameof(GetBooks)}");
                return StatusCode(500, Messages.Error500Message);
            };
        }

        // GET: api/Books/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookDetailsDto>> GetBook(int id)
        {
            logger.LogInformation($"Request to {nameof(GetBook)} - ID {id}");
            try
            {
                //var book = await _context.Books.FindAsync(id);

                var book = await _context.Books
                    .Include(q => q.Author)
                    .ProjectTo<BookDetailsDto>(mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (book == null)
                {
                    logger.LogWarning($"Request Not Found: {nameof(GetBook)} - ID {id}");
                    return NotFound();
                }

                //return book;

                //return Ok(book);

                //Menggunakan DTO
                var bookDto = mapper.Map<BookReadOnlyDto>(book);
                return Ok(bookDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error Performing GET in {nameof(GetBook)}");
                return StatusCode(500, Messages.Error500Message);
            }
        }

        // PUT: api/Books/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, BookUpdateDto bookDto)
        {
            logger.LogInformation($"Request to {nameof(PutBook)} - ID {id}");
            if (id != bookDto.Id)
            {
                logger.LogWarning($"Update ID invalid in {nameof(PutBook)} - ID {id}");
                return BadRequest();
            }

            //_context.Entry(bookDto).State = EntityState.Modified;

            //Menggunakan DTO
            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                logger.LogWarning($"{nameof(Book)} record not found in {nameof(PutBook)} - ID {id}");
                return NotFound();
            }

            mapper.Map(bookDto, book);
            _context.Entry(book).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!BookExists(id))
                {
                    return NotFound();
                }
                else
                {
                    logger.LogError(ex, $"Error Performing PUT in {nameof(PutBook)}");
                    return StatusCode(500, Messages.Error500Message);
                }
            }

            return NoContent();
        }

        // POST: api/Books
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<BookCreateDto>> PostBook(BookCreateDto bookDto)
        {
            logger.LogInformation($"Request to {nameof(PostBook)}");
            try
            {
                //_context.Books.Add(book);
                //await _context.SaveChangesAsync();

                //return CreatedAtAction("GetBook", new { id = book.Id }, book);

                var book = mapper.Map<Book>(bookDto);
                await _context.Books.AddAsync(book);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error Performing POST in {nameof(PostBook)}");
                return StatusCode(500, Messages.Error500Message);
            }
        }

        // DELETE: api/Books/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            logger.LogInformation($"Request to {nameof(DeleteBook)} - ID {id}");
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null)
                {
                    logger.LogWarning($"{nameof(Book)} record not found in {nameof(DeleteBook)} - ID {id}");
                    return NotFound();
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error Performing DELETE in {nameof(DeleteBook)}");
                return StatusCode(500, Messages.Error500Message);
            }
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
