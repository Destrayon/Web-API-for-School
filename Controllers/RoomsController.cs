using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FunnyServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private static readonly Dictionary<string, int> Rooms = new();
        [HttpPost]
        public ActionResult CreateOrAdd([Required]string room)
        {
            if (Rooms.ContainsKey(room)) Rooms[room]++;
            else Rooms[room] = 1;

            return Ok();
        }

        [HttpDelete]
        public ActionResult RemoveFromRoom([Required]string room)
        {
            if (!Rooms.ContainsKey(room)) return Ok();
            Rooms[room]--;

            if (Rooms[room] == 0) Rooms.Remove(room);

            return Ok();
        }

        [HttpGet]
        public ActionResult<int> AmountInRoom([Required]string room)
        {
            if (Rooms.TryGetValue(room, out int value)) return Ok(value);

            return Ok(0);
        }


    }
}
