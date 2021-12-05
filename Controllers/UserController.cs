using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace FunnyServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _config;
        public UserController(IConfiguration config)
        {
            _config = config;
        }
        [Route("create")]
        [HttpPost]
        public ActionResult CreateUser([Required]string username, [Required]string password)
        {
            string salt = PasswordLogic.GetRandomString(8);

            string hash = PasswordLogic.ComputeSaltedHash(password, salt);

            username = username.ToLower();

            using var conn = new NpgsqlConnection(_config.GetConnectionString("Postgresql"));

            conn.Open();

            NpgsqlCommand cmd = conn.CreateCommand();

            cmd.CommandText = $"INSERT INTO users (username) VALUES ('{username}')";

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (PostgresException) 
            {
                return Conflict("Username already exists.");
            }

            cmd.CommandText = $"INSERT INTO passwords (user_id, password_hash, password_salt) SELECT user_id, '{hash}', '{salt}' FROM users WHERE username='{username}'";

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (PostgresException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Something has gone wrong.");
            }

            return Ok("User has been created.");
        }

        [HttpPost]
        public ActionResult<long> AuthenicateUser([Required]string username, [Required]string password)
        {
            username = username.ToLower();

            using var conn = new NpgsqlConnection(_config.GetConnectionString("Postgresql"));

            conn.Open();

            NpgsqlCommand cmd = conn.CreateCommand();

            cmd.CommandText = $@"SELECT CASE COUNT(*) 
                                 WHEN 0 THEN -1
                                 ELSE MAX(user_id)
                                 END
                                 FROM users
                                 WHERE username = '{username}'";

            long id = (long)cmd.ExecuteScalar();

            if (id == -1)
            {
                return NotFound(-1);
            }

            cmd.CommandText = $"SELECT password_salt FROM passwords WHERE user_id = '{id}'";

            string salt = cmd.ExecuteScalar().ToString();

            string hash = PasswordLogic.ComputeSaltedHash(password, salt);

            cmd.CommandText = $"SELECT COUNT(*) FROM passwords WHERE password_hash = '{hash}'";

            long count = (long)cmd.ExecuteScalar();

            if (count > 0) return Ok(id);

            return Unauthorized(-1);
        }
    }
}
