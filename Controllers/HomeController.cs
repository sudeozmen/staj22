using Microsoft.AspNetCore.Mvc;
using staj22.Models;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc.Formatters;
using static staj22.Controllers.HomeController;

namespace staj22.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var sql = new Sql();
            List<Books> list = sql.GetTables();
            List<BorrowedBooks> recordList = sql.GetRecords();
            List<string> barcodes = recordList.Select(record => record.BBARCODE).ToList();
            List<Books> availableBooks = list.Where(book => !barcodes.Contains(book.BARCODE)).ToList();
            ViewData["AvailableBooks"] = availableBooks;
            return View(availableBooks);
        }

        [HttpPost]
        public IActionResult Index(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                var sql = new Sql();
                var list = sql.Search(input);
                List<BorrowedBooks> recordList = sql.GetRecords();
                List<string> barcodes = recordList.Select(record => record.BBARCODE).ToList();
                List<Books> availableBooks = list.Where(book => !barcodes.Contains(book.BARCODE)).ToList();
                List<Books> borrowedBooks = list.Where(book => barcodes.Contains(book.BARCODE)).ToList();
                ViewData["AvailableBooks"] = availableBooks;
                if (borrowedBooks.Count>0)
                {
                    ViewBag.sonuc = "THIS BOOK IS ALREADY BORROWED. PLEASE CHECK RECORDS PAGE";
                    return View();
                }
                else if (!(availableBooks.Count > 0))
                {
                    ViewBag.sonuc = "INVALID NAME";
                    return View();
                }
                else
                {
                    return View(availableBooks);
                }
            }
            return View();
        }

        [HttpGet]
        public IActionResult Records()
        {
            var sql = new Sql();
            List<BorrowedBooks> list = sql.GetRecords();
            return View(list);
        }
        [HttpPost]
        public IActionResult Records(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                var sql = new Sql();
                var list = sql.Find(input);
                if (!(list.Count > 0))
                {
                    ViewBag.sonuc = "NO RECORD FOUNDED";
                    return View();
                }
                else
                {
                    return View(list);
                }
            }
            return View();

        }

        [HttpGet]
        public IActionResult Insert()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Insert(string barcode, string name, string author, string page)
        {
            var sql = new Sql();
            List<Books> bookList = sql.GetTables();
            List<string> barcodes = bookList.Select(book => book.BARCODE).ToList();
            if (barcodes.Contains(barcode))
            {
                ViewBag.result = "A Book With The Same Barcode Already Exists. Please Check Your Inputs";
                return View();
            }
            else
            {
                string result = sql.Insert(barcode, name, author, page);
                ViewBag.result = result;
            }
            return View();
        }

        [HttpGet]
        public IActionResult Registry(string BBARCODE)
        {
            var sql = new Sql();
            var bookInfo = sql.fetchBook(BBARCODE).FirstOrDefault(); 
            if (bookInfo == null)
            {
                return RedirectToAction("Index"); 
            }
            ViewBag.barcode = bookInfo.BARCODE;
            ViewBag.bookName = bookInfo.NAME;
            return View();
        }


        [HttpPost]
        public IActionResult Registry(string ID_NUMBER, string FULL_NAME, string PHONE_NUMBER, string FDATE, string LDATE, string BBARCODE)
        {
            if (string.IsNullOrEmpty(BBARCODE))
            {
                ViewBag.res = "Invalid book barcode";
                return View();
            }
            var sql = new Sql();
            var book = sql.fetchBook(BBARCODE).FirstOrDefault();
            if (book == null)
            {
                ViewBag.res = "Invalid book barcode";
            }
            else
            {
                var newRecord = new BorrowedBooks
                {
                    ID_NUMBER = ID_NUMBER,
                    FULL_NAME = FULL_NAME,
                    PHONE_NUMBER = PHONE_NUMBER,
                    FDATE = FDATE,
                    LDATE = LDATE,
                    BBARCODE = book.BARCODE,
                    BOOK_NAME = book.NAME
                };
                string res = sql.Registry(newRecord);
                if (res.Contains("Result: Successful"))
                {
                    ViewBag.res = "Record added successfully";
                }
                else
                {
                    ViewBag.res = "Error adding record";
                }
            }

            return View();
        }

        [HttpGet]
        public IActionResult Delete(string ID_NUMBER, string FULL_NAME, string BOOK_NAME)
        {
            var sql = new Sql();
            var recordInfo = sql.fetchRecord(ID_NUMBER).FirstOrDefault();
            if (recordInfo == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.idnumber = ID_NUMBER;
            ViewBag.fullname = FULL_NAME;
            ViewBag.bookname = BOOK_NAME;
            return View();
        }

        [HttpPost]
        public IActionResult Delete(string ID_NUMBER)
        {
            var sql = new Sql();
            var deneme = sql.fetchRecord(ID_NUMBER).FirstOrDefault();
            var result = sql.Delete(ID_NUMBER);
            if (result.Contains("Result: Successful"))
            {
                ViewBag.res = "Record deleted successfully";
            }
            else
            {
                ViewBag.res = "Error deleting record";
            }
            return View();

        }


        public class Sql
        {
            private List<Books> tables = new List<Books>();
            private List<BorrowedBooks> records = new List<BorrowedBooks>();
            public List<Books> GetTables()
            {
                using var SqlConnection = GetSqlConnection();
                try
                {
                    SqlConnection.Open();
                    string query = "SELECT * from BOOKS";
                    SqlCommand command = new SqlCommand(query, SqlConnection);
                    using var rdr = command.ExecuteReader();
                    while (rdr.Read())
                        tables.Add(new Books
                        {
                            BARCODE = rdr[0].ToString(),
                            NAME = rdr[1].ToString(),
                            AUTHOR = rdr[2].ToString(),
                            PAGE = rdr[3].ToString()
                        });
                    rdr.Close();
                    return tables;
                }
                finally
                {
                    SqlConnection.Close();
                }
            }

            public List<BorrowedBooks> GetRecords()
            {
                using var SqlConnection = GetSqlConnection();
                try
                {
                    SqlConnection.Open();
                    string query = "SELECT * from BORROWED_BOOKS";
                    SqlCommand command = new SqlCommand(query, SqlConnection);
                    using var rdr = command.ExecuteReader();
                    while (rdr.Read())
                        records.Add(new BorrowedBooks
                        {
                            ID_NUMBER = rdr[0].ToString(),
                            FULL_NAME = rdr[1].ToString(),
                            PHONE_NUMBER = rdr[2].ToString(),
                            FDATE = rdr[3].ToString(),
                            LDATE = rdr[4].ToString(),
                            BBARCODE = rdr[5].ToString(),
                            BOOK_NAME = rdr[6].ToString(),
                        });
                    rdr.Close();
                    return records;
                }
                finally
                {
                    SqlConnection.Close();
                }
            }

            public List<Books> Search(string a)
            {
                using var SqlConnection = GetSqlConnection();
                try
                {
                    SqlConnection.Open();
                    string query = "SELECT * from BOOKS WHERE NAME=@Name";
                    SqlCommand command = new SqlCommand(query, SqlConnection);
                    command.Parameters.AddWithValue("@Name", a);
                    using var rdr = command.ExecuteReader();
                    while (rdr.Read())
                        tables.Add(new Books
                        {
                            BARCODE = rdr[0].ToString(),
                            NAME = rdr[1].ToString(),
                            AUTHOR = rdr[2].ToString(),
                            PAGE = rdr[3].ToString()
                        });
                    rdr.Close();
                    return tables;
                }
                finally
                {
                    SqlConnection.Close();
                }
            }
            public List<BorrowedBooks> Find(string a)
            {
                using var SqlConnection = GetSqlConnection();
                try
                {
                    SqlConnection.Open();
                    string query = "SELECT * from BORROWED_BOOKS WHERE FULL_NAME LIKE '%' + @FName + '%' BOOK_NAME LIKE '%' +BName+ '%'";
                    SqlCommand command = new SqlCommand(query, SqlConnection);
                    command.Parameters.AddWithValue("@FName", a);
                    command.Parameters.AddWithValue("@BName", a);
                    using var rdr = command.ExecuteReader();
                    while (rdr.Read())
                        records.Add(new BorrowedBooks
                        {
                            ID_NUMBER = rdr[0].ToString(),
                            FULL_NAME = rdr[1].ToString(),
                            PHONE_NUMBER = rdr[2].ToString(),
                            FDATE = rdr[3].ToString(),
                            LDATE = rdr[4].ToString(),
                            BBARCODE = rdr[5].ToString(),
                            BOOK_NAME = rdr[6].ToString(),
                        });
                    rdr.Close();
                    return records;
                }
                finally
                {
                    SqlConnection.Close();
                }
            }

            public List<Books> fetchBook(string a)
            {
                using var SqlConnection = GetSqlConnection();
                try
                {
                    SqlConnection.Open();
                    string query = "SELECT * FROM BOOKS WHERE BARCODE=@barcode";
                    SqlCommand command = new SqlCommand(query, SqlConnection);
                    command.Parameters.AddWithValue("@barcode", a);
                    using var rdr = command.ExecuteReader();
                    while (rdr.Read())
                        tables.Add(new Books
                        {
                            BARCODE = rdr[0].ToString(),
                            NAME = rdr[1].ToString(),
                            AUTHOR = rdr[2].ToString(),
                            PAGE = rdr[3].ToString(),
                        });
                    rdr.Close();
                    return tables;
                }
                finally
                {
                    SqlConnection.Close();
                }
            }
            public List<BorrowedBooks> fetchRecord(string a)
            {
                using var SqlConnection = GetSqlConnection();
                try
                {
                    SqlConnection.Open();
                    string query = "SELECT * FROM BORROWED_BOOKS WHERE ID_NUMBER=@idnumber";
                    SqlCommand command = new SqlCommand(query, SqlConnection);
                    command.Parameters.AddWithValue("@idnumber", a);
                    using var rdr = command.ExecuteReader();
                    while (rdr.Read())
                        records.Add(new BorrowedBooks
                        {
                            ID_NUMBER = rdr[0].ToString(),
                            FULL_NAME = rdr[1].ToString(),
                            PHONE_NUMBER = rdr[2].ToString(),
                            FDATE = rdr[3].ToString(),
                            LDATE = rdr[3].ToString(),
                            BBARCODE = rdr[3].ToString(),
                            BOOK_NAME = rdr[3].ToString(),
                        });
                    rdr.Close();
                    return records;
                }
                finally
                {
                    SqlConnection.Close();
                }
            }
            public string Insert(string barcode, string name, string author, string page)
            {
                string result = "";

                using var SqlConnection = GetSqlConnection();
                try
                {
                    SqlConnection.Open();
                    string query = "INSERT INTO BOOKS(BARCODE,NAME,AUTHOR,PAGE) VALUES('" + barcode + "','" + name + "','" + author + "','" + page + "')";
                    SqlCommand command = new SqlCommand(query, SqlConnection);
                    command.ExecuteNonQuery();
                    result += "Result: Successful";
                    return result;
                }
                catch (SqlException ex)
                {
                    result += ex;
                }
                finally
                {
                    SqlConnection.Close();
                }
                return result;
            }
            public string Registry(BorrowedBooks newRecord)
            {
                string result = "";
                using var SqlConnection = GetSqlConnection();
                try
                {
                    SqlConnection.Open();
                    string query = "INSERT INTO BORROWED_BOOKS(ID_NUMBER,FULL_NAME,PHONE_NUMBER,FDATE,LDATE,BBARCODE,BOOK_NAME) VALUES(@ID_NUMBER, @FULL_NAME, @PHONE_NUMBER, @FDATE, @LDATE, @BBARCODE, @BOOK_NAME)";
                    SqlCommand command = new SqlCommand(query, SqlConnection);
                    command.Parameters.AddWithValue("@ID_NUMBER", newRecord.ID_NUMBER);
                    command.Parameters.AddWithValue("@FULL_NAME", newRecord.FULL_NAME);
                    command.Parameters.AddWithValue("@PHONE_NUMBER", newRecord.PHONE_NUMBER);
                    command.Parameters.AddWithValue("@FDATE", newRecord.FDATE);
                    command.Parameters.AddWithValue("@LDATE", newRecord.LDATE);
                    command.Parameters.AddWithValue("@BBARCODE", newRecord.BBARCODE);
                    command.Parameters.AddWithValue("@BOOK_NAME", newRecord.BOOK_NAME);
                    command.ExecuteNonQuery();
                    result += "Result: Successful";
                    return result;
                }
                catch (SqlException ex)
                {
                    result += ex;
                }
                finally
                {
                    SqlConnection.Close();
                }
                return result;
            }

            public string Delete(string ID_NUMBER)
            {
                string result = "";
                using var SqlConnection = GetSqlConnection();
                try
                {
                    SqlConnection.Open();
                    string query = "DELETE FROM BORROWED_BOOKS WHERE ID_NUMBER = @ID_NUMBER";
                    SqlCommand command = new SqlCommand(query, SqlConnection);
                    command.Parameters.AddWithValue("@ID_NUMBER", ID_NUMBER);
                    command.ExecuteNonQuery();
                    result = "Result: Successful";
                    return result;
                }
                catch(SqlException ex)
                {
                    result += ex;
                }
                finally
                {
                    SqlConnection.Close();
                }
                return result;
            }


            public SqlConnection GetSqlConnection()
            {
                string _connectionString = "Data Source=DESKTOP-5M8O5JE\\MSSQLSERVER01;Initial Catalog=staj22;Integrated Security=True;TrustServerCertificate=True";

                return new SqlConnection(_connectionString);
            }
        }
    }
}

