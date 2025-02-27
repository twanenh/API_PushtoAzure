using API_PushtoAzure.Models;
using API_PushtoAzure;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API_PushtoAzure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NhanVienController : ControllerBase
    {
        private readonly AppDbContext _context;
        public NhanVienController(AppDbContext context)
        {
            _context = context;
        }
        // GET: api/<NhanVienController>
        [HttpGet]
        public IActionResult Get()
        {
            var list = _context.nhanViens.ToList();
            return Ok(list);
        }

        // GET api/<NhanVienController>/5
        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var itemGet = _context.nhanViens.FirstOrDefault(x => x.Id == id);
            if (itemGet == null)
            {
                return NotFound("Không tìm thấy nhân viên có ID đó");
            }
            else
            {
                return Ok(itemGet);
            }

        }

        // POST api/<NhanVienController>
        [HttpPost]
        public IActionResult Post([FromBody] NhanVienDTO dTO)
        {
            var nhanVien = new NhanVien
            {
                Id = Guid.NewGuid(),
                Ten = dTO.Ten,
                Tuoi = dTO.Tuoi,
                Role = dTO.Role,
                Email = dTO.Email,
                Luong = dTO.Luong,
                TrangThai = dTO.TrangThai
            };
            _context.nhanViens.Add(nhanVien);
            _context.SaveChanges();
            return Ok(nhanVien);
        }

        // PUT api/<NhanVienController>/5
        [HttpPut("{id}")]
        public IActionResult Put(Guid id, [FromBody] NhanVienDTO DTO)
        {
            var itemUpdate = _context.nhanViens.FirstOrDefault(x => x.Id == id);
            if (itemUpdate == null) { return NotFound("Không tìm thấy nhân viên có ID như vậy"); }
            else
            {
                itemUpdate.Ten = DTO.Ten;
                itemUpdate.Tuoi = DTO.Tuoi;
                itemUpdate.Role = DTO.Role;
                itemUpdate.Email = DTO.Email;
                itemUpdate.Luong = DTO.Luong;
                itemUpdate.TrangThai = DTO.TrangThai;
                _context.nhanViens.Update(itemUpdate);
                _context.SaveChanges();
                return Ok(itemUpdate);
            }
        }

        // DELETE api/<NhanVienController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var itemDelete = _context.nhanViens.FirstOrDefault(x => x.Id == id);
            if (itemDelete == null) { return NotFound("Không tìm thấy nhân viên có ID như vậy"); }
            else
            {
                _context.nhanViens.Remove(itemDelete);
                _context.SaveChanges();
                return Ok("Xóa thành công");
            }
        }
    }
}
