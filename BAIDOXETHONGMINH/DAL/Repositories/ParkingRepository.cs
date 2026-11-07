namespace BAIDOXETHONGMINH.GUI;

public class ParkingRepository
{
    private readonly string _connectionString = "Server=localhost;Database=QuanLyXe;Trusted_Connection=true;trustServerCertificate=true;";
    
    //NV5: tra cứu lịch sử ra vào
    public Datatable getParkingHistory(string bienSo, DateTime tuNgay, DateTime denNgay, int trangThai){
        using var conn = new SqlConnection(_connectionString);
        {
            string query = @"SELECT MaLichSu, MaThe, BienSoVao, BienSoRa, ThoiGianVao, ThoiGianRa, AnhVao, AnhRa, GiaTien, TrangThai 
                    FROM LichSuVaoRa 
                    WHERE (@BienSo IS NULL OR BienSoVao LIKE '%' + @BienSo + '%' OR BienSoRa LIKE '%' + @BienSo + '%')
                      AND (@TuNgay IS NULL OR ThoiGianVao >= @TuNgay)
                      AND (@DenNgay IS NULL OR ThoiGianVao <= @DenNgay)
                      AND (@TrangThai IS NULL OR TrangThai = @TrangThai)
                    ORDER BY ThoiGianVao DESC";
                    using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@BienSo", string.IsNullOrEmpty(bienSo) ? (object)DBNull.Value : bienSo);
                cmd.Parameters.AddWithValue("@TuNgay", tuNgay.HasValue ? (object)tuNgay.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@DenNgay", denNgay.HasValue ? (object)denNgay.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@TrangThai", trangThai.HasValue ? (object)trangThai.Value : DBNull.Value);

                var dt = new DataTable();
                var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                return dt;
            }
        }
    }
    //NV5: tra cứu biên bản sự cố / mất thẻ
    public bool HandleLostCardReport(long maLichSu, string mathe, string hoten, string cccd, string sdt, decimal tienGui, decimal tienPhat, string maNV)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    //1. Cập nhật trạng thái lịch sử ra vào thành "Mất thẻ" (TrangThai = 2)
                    string sqlUpdateHistory = "UPDATE LichSuVaoRa SET ThoiGianRa = GETDATE(), GiaTien = @TongTien, TrangThai = 2 WHERE MaLichSu = @MaLichSu ";
                    using (var cmd1 = new sqlCommand(sqlUpdateHistory, conn, trans))
                    {
                        cmd1.Parameters.AddWithValue(@TongTien, tienGui + tienPhat);
                        cmd1.Parameters.AddWithValue(@MaLichSu, maLichSu);
                        cmd1.executeNonQuery();
                    } 
                    //2. Khoá thẻ(TrangThai = 0)
                    string sqlLockCard = "UPDATE TheXe SET TrangThai = 0 WHERE MaThe = @MaThe ";
                    using (var cmd2 = new sqlCommand(sqlLockCard, conn, trans))
                    {
                        cmd2.Parameters.AddWithValue(@MaThe, mathe);
                        cmd2.executeNonQuery();
                    }
                    //3. Thêm/Lưu thông tin sự cố mất thẻ
                    string sqlSuCo = @"INSERT INTO SuCoMatThe(MaLichSu, MaThe, HoTenChuThe, CCCD, SDT, TienPhat, ThoiGianGiaiQuyet, MaNV)
                    VALUES (@MaLichSu, @MaThe, @HoTenChuThe, @CCCD, @SDT, @TienPhat, GETDATE(), @MaNV)";
                    using (var cmd3 = new sqlCommand(sqlSuCo, conn, trans))
                    {
                        cmd3.Parameters.AddWithValue(@MaLichSu, maLichSu);
                        cmd3.Parameters.AddWithValue(@MaThe, mathe);
                        cmd3.Parameters.AddWithValue(@HoTenChuThe, hoten);
                        cmd3.Parameters.AddWithValue(@CCCD, cccd);
                        cmd3.Parameters.AddWithValue(@SDT, sdt);
                        cmd3.Parameters.AddWithValue(@TienPhat, tienPhat);
                        cmd3.Parameters.AddWithValue(@MaNV, maNV);
                        cmd3.executeNonQuery();
                    }
                    trans.Commit();
                    return true;
                           
                }
                catch
                {
                    trans.Rollback();
                    return false;
                }
            }    
        }
    }


    //NV6: Thống kê doanh thu theo ca làm việc
    public DataTable GetRevenueByShift(int maCa)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            string query = @" 
            SELECT c.MaCa, c.TenCa, c.GioBatDau, c.GioKetThuc, 
                   COUNT(ls.MaLichSu) AS TongLuotXeRa,
                     SUM(CASE WHEN ls.TrangThai = 1 THEN ls.GiaTien ELSE 0 END) AS DoanhThuThuong
                     SUM(CASE WHEN ls.TrangThai = 2 THEN ls.GiaTien ELSE 0 END) AS DoanhThuMatThe
                SUM(ls.GiaTien) AS TongDoanhThu
            FROM CaTruc c
            LEFT JOIN LichSuVaoRa ls ON ls.ThoiGianRa BETWEEN c.GioBatDau AND isNULL(c.GioKetThuc, GETDATE())
            WHERE c.MaCa = @MaCa
            GROUP BY c.MaCa, c.TenCa, c.GioBatDau, c.GioKetThuc";
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue(@MaCa, maCa);
                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    return dt;
                
            }
        }
    }





}