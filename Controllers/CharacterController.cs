using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace FunnyServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CharacterController : ControllerBase
    {
        private readonly string connString;

        public CharacterController(IConfiguration config)
        {
            connString = config.GetConnectionString("Postgresql");
        }
        [HttpPost]
        public ActionResult CreateCharacter([Required]long user_id, [Required]string character_name, [Required]ushort position, [Required]string color)
        {
            using NpgsqlConnection conn = new(connString);

            conn.Open();

            NpgsqlCommand command = conn.CreateCommand();

            command.CommandText = $"INSERT INTO characters (user_id, character_name) VALUES ('{user_id}', '{character_name}')";

            try
            {
                command.ExecuteNonQuery();
            }
            catch (PostgresException)
            {
                return Conflict("Character name already exists.");
            }

            command.CommandText = $@"INSERT INTO character_positions (character_id, character_position) 
                                     SELECT character_id, '{position}' 
                                     FROM characters 
                                     WHERE character_name = '{character_name}'";

            command.ExecuteNonQuery();

            command.CommandText = $@"INSERT INTO character_colors (character_id, character_color) 
                                     SELECT character_id, '{color}' 
                                     FROM characters 
                                     WHERE character_name = '{character_name}'";

            command.ExecuteNonQuery();

            command.CommandText = $@"INSERT INTO character_levels (character_id, character_level)
                                     SELECT character_id, '0'
                                     FROM characters
                                     WHERE character_name = '{character_name}'";

            command.ExecuteNonQuery();

            return Ok();
        }

        [HttpGet]
        public ActionResult<IEnumerator<Character>> GetCharacters([Required]long user_id)
        {
            using NpgsqlConnection conn = new(connString);

            conn.Open();

            var cmd = conn.CreateCommand();

            cmd.CommandText = $@"SELECT characters.character_id, character_name, character_position, character_color, character_level
                                 FROM characters, character_positions, character_colors, character_levels
                                 WHERE characters.character_id = character_positions.character_id
                                 AND characters.character_id = character_colors.character_id
                                 AND characters.character_id = character_levels.character_id
                                 AND user_id = '{user_id}'";

            List<Character> characters = new();

            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                characters.Add(new Character
                {
                    CharacterId = (long)reader["character_id"],
                    CharacterName = reader["character_name"].ToString(),
                    CharacterPosition = ushort.Parse(reader["character_position"].ToString()),
                    CharacterColor = reader["character_color"].ToString(),
                    CharacterLevel = (int)reader["character_level"]
                });
            }

            return Ok(characters);
        }

        [HttpDelete]
        public ActionResult DeleteCharacter([Required]long character_id)
        {
            using NpgsqlConnection conn = new(connString);

            conn.Open();

            var cmd = conn.CreateCommand();

            cmd.CommandText = $"DELETE FROM characters WHERE character_id='{character_id}'";

            cmd.ExecuteNonQuery();

            return Ok();
        }
    }

    public struct Character
    { 
        public long CharacterId { get; set; }
        public string CharacterName { get; set; }
        public ushort CharacterPosition { get; set; }
        public string CharacterColor { get; set; }
        public int CharacterLevel { get; set; }
    }
}
