namespace API_PushtoAzure.Models
{
    public class NhanVien
    {
        public Guid Id { get; set; }
        public string Ten { get; set; }
        public int Tuoi { get; set; }
        public int Role { get; set; } 
        public string Email { get; set; } 
        public int Luong { get; set; }
        public bool TrangThai { get; set; }
    }
}
