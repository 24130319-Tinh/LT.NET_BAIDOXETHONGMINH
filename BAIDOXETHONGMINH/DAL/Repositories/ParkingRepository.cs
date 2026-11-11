using BAIDOXETHONGMINH.DAL.Database;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using BAIDOXETHONGMINH.DTO.Entities;

namespace BAIDOXETHONGMINH.GUI;

public class ParkingRepository
{
    
    //NV5: tra cứu lịch sử ra vào
    public List<ParkingSession> getParkingHistory(string keyword,string status , string tuNgay, string denNgay){
        
            var list = new List<ParkingSession>();
            using (var conn = SqliteDbContext.GetConnection())
            {
               string query = @"
    SELECT
        SessionId,
        CardCode,
        LicensePlate,
        VehicleTypeId,
        SlotCode,
        CheckInTime,
        CheckOutTime,
        CheckInImagePath,
        CheckOutImagePath,
        CheckInUserId,
        CheckOutUserId,
        IsMonthlyPass,
        ParkingFee,
        PenaltyFee,
        TotalAmount,
        Status
    FROM ParkingSessions
    WHERE
        (@Keyword IS NULL
            OR LicensePlate LIKE '%' || @Keyword || '%'
            OR CardCode LIKE '%' || @Keyword || '%')
        AND (@Status IS NULL OR Status = @Status)
        AND (@TuNgay IS NULL OR date(CheckInTime) >= date(@TuNgay))
        AND (@DenNgay IS NULL OR date(CheckInTime) <= date(@DenNgay))
    ORDER BY CheckInTime DESC;";
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = query;
                        cmd.Parameters.AddWithValue("@Keyword", string.IsNullOrEmpty(keyword) ? (object)DBNull.Value : keyword);
                        cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(status) ? (object)DBNull.Value : status);
                        cmd.Parameters.AddWithValue("@TuNgay", string.IsNullOrEmpty(tuNgay) ? (object)DBNull.Value : tuNgay);
                        cmd.Parameters.AddWithValue("@DenNgay", string.IsNullOrEmpty(denNgay) ? (object)DBNull.Value : denNgay);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                
                                   list.Add(new ParkingSession
{
                                    SessionId = reader.GetInt32(0),
                                    CardCode = reader.GetString(1),
                                    LicensePlate = reader.GetString(2),
                                    VehicleTypeId = Convert.ToString(reader.GetValue(3)) ?? String.Empty,
                                    SlotCode = reader.GetString(4),
                                    CheckInTime = DateTime.Parse(reader.GetString(5)),
                                    CheckOutTime = reader.IsDBNull(6) ? null : DateTime.Parse(reader.GetString(6)),
                                    CheckInImagePath = reader.IsDBNull(7) ? null : reader.GetString(7),
                                    CheckOutImagePath = reader.IsDBNull(8) ? null : reader.GetString(8),
                                    CheckInUserId = reader.GetInt32(9),
                                    CheckOutUserId = reader.IsDBNull(10) ? (int?)null : reader.GetInt32(10),
                                    IsMonthlyPass = reader.GetInt32(11) == 1,
                                    ParkingFee = reader.GetDecimal(12),
                                    PenaltyFee = reader.GetDecimal(13),
                                    TotalAmount = reader.GetDecimal(14),
                                    Status = reader.GetString(15)
                                    });
                            }
                        }
                    }   
            }
            return list;         
    }
    //NV5: tra cứu biên bản sự cố / mất thẻ

    //user = 
    public bool HandleLostCardReport(long sessionId, string cardCode, string licensePlate, string customerName, string phoneNumber, string customerIdCode, decimal parkingFee, decimal penaltyFee, int userId, long shiftId, string note)
    {
        using (var conn = SqliteDbContext.GetConnection()) 
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    decimal totalAmount = parkingFee + penaltyFee;
                    //1. Cập nhật trạng thái lịch sử ra vào thành "Mất thẻ" (ParkingSessions thành LostCard) - NV5
                    string updateSession = @" UPDATE ParkingSessions SET CheckOutTime = datetime('now', 'localtime'), ParkingFee = @ParkingFee, PenaltyFee = @PenaltyFee,
                    TotalAmount = @TotalAmount, Status = 'LostCard', CheckOutUserId = @UserId WHERE SessionId = @SessionId;";
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;

                        cmd.CommandText = updateSession;
                        cmd.Parameters.AddWithValue("@ParkingFee", parkingFee);
                        cmd.Parameters.AddWithValue("@PenaltyFee", penaltyFee);
                        cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@SessionId", sessionId);
                        
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                    {
                        trans.Rollback();
                        return false; // Không tìm thấy sessionId
                    }
                    } 
                    //2. Khoá thẻ(ParkingCards) - NV5
                    string updateCard = "UPDATE ParkingCards SET Status = 'Locked' WHERE CardCode = @CardCode;";
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.CommandText = updateCard;
                        cmd.Parameters.AddWithValue("@CardCode", cardCode);
                        cmd.ExecuteNonQuery();
                    }
                    //3. Thêm/Lưu thông tin sự cố mất thẻ(IncidentReports) - NV5
                   string insertIncident = @"
                        INSERT INTO IncidentReports
                        (
                            SessionId,
                            CardCode,
                            LicensePlate,
                            CustomerName,
                            CustomerPhone,
                            CustomerIdentityCard,
                            IncidentType,
                            CompensationFee,
                            Note,
                            HandledByUserId,
                            CreatedAt
                        )
                        VALUES
                        (
                            @SessionId,
                            @CardCode,
                            @LicensePlate,
                            @CustomerName,
                            @CustomerPhone,
                            @CustomerIdentityCard,
                            'LostCard',
                            @CompensationFee,
                            @Note,
                            @UserId,
                            datetime('now', 'localtime')
                        );";
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;

                        cmd.CommandText = insertIncident;
                        cmd.Parameters.AddWithValue("@SessionId", sessionId);
                        cmd.Parameters.AddWithValue("@CardCode", cardCode);
                        cmd.Parameters.AddWithValue("@LicensePlate", licensePlate);
                        cmd.Parameters.AddWithValue("@CustomerName", customerName);
                        cmd.Parameters.AddWithValue("@CustomerPhone", phoneNumber);
                        cmd.Parameters.AddWithValue("@CustomerIdentityCard", customerIdCode);
                        cmd.Parameters.AddWithValue("@CompensationFee", penaltyFee);
                        cmd.Parameters.AddWithValue("@Note", String.IsNullOrEmpty(note) ? (object)DBNull.Value : note);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.ExecuteNonQuery();
                    }
                    //4. Thêm giao dịch thu tiền (PaymentTransactions) - NV5
                    string insertPayment = @"
                        INSERT INTO PaymentTransactions
                        (
                            TransactionType,
                            ReferenceId,
                            Amount,
                            PaymentMethod,
                            CollectedByUserId,
                            ShiftId,
                            Note,
                            CreatedAt
                        )
                        VALUES
                        (
                            'LostCardPenalty',
                            @SessionId,
                            @TotalAmount,
                            'Cash',
                            @UserId,
                            @ShiftId,
                            @Note,
                            datetime('now', 'localtime')
                        );";
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = trans;
                        cmd.CommandText = insertPayment;
                        cmd.Parameters.AddWithValue("@SessionId", sessionId);
                        cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@ShiftId", shiftId == 0 ? (object)DBNull.Value : shiftId);
                        cmd.Parameters.AddWithValue("@Note", "Thu phí giữ xe và phạt mất thẻ");
                        cmd.ExecuteNonQuery();

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
            

    //NV6: Thống kê doanh thu theo ca làm việc, Quản lý ca trực..

       //1. Thống kê doanh thu theo ca trực..(work Shift) - NV6
    public Dictionary<string, object> GetShiftRevenueReport(long shiftId)
    {
        var result = new Dictionary<string, object>();
        using (var conn = SqliteDbContext.GetConnection())
        {
                string query = @"
                    SELECT
                        s.ShiftId,
                        s.ShiftName,
                        s.StartTime,
                        s.EndTime,
                        s.Status,
                        s.TotalVehiclesOut,

                        SUM(
                            CASE
                                WHEN p.TransactionType = 'ParkingFee'
                                THEN p.Amount
                                ELSE 0
                            END
                        ) AS RevenueNormal,

                        SUM(
                            CASE
                                WHEN p.TransactionType = 'LostCardPenalty'
                                THEN p.Amount
                                ELSE 0
                            END
                        ) AS RevenueLostCard,

                        SUM(p.Amount) AS TotalRevenue

                    FROM WorkShifts s
                    LEFT JOIN PaymentTransactions p
                        ON s.ShiftId = p.ShiftId

                    WHERE s.ShiftId = @ShiftId

                    GROUP BY
                        s.ShiftId,
                        s.ShiftName,
                        s.StartTime,
                        s.EndTime,
                        s.Status,
                        s.TotalVehiclesOut;";


            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@ShiftId", shiftId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())  
                    {
                        result["ShiftId"] = reader.GetInt64(0);
                        result["ShiftName"] = reader.GetString(1);
                        result["StartTime"] = reader.GetString(2);
                        result["EndTime"] = reader.IsDBNull(3) ? "Đang Mở" : reader.GetString(3);
                        result["Status"] = reader.GetString(4);
                        result["TotalVehiclesOut"] = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                        result["RevenueNormal"] = reader.IsDBNull(6) ? 0m : reader.GetDecimal(6);
                        result["RevenueLostCard"] = reader.IsDBNull(7) ? 0m : reader.GetDecimal(7);
                        result["TotalRevenue"] = reader.IsDBNull(8) ? 0m : reader.GetDecimal(8);
                    }
                }
            }
        }
        return result;
    }
    //2. Chốt ca trực (Close Shift) - NV6
    public bool CloseShift(long shiftId, decimal finalRevenue)
    {
        using var conn = SqliteDbContext.GetConnection();
        {
            string query = @"
                    UPDATE WorkShifts
                    SET EndTime = datetime('now', 'localtime'),
                        TotalRevenue = @FinalRevenue,
                        Status = 'Closed'
                    WHERE ShiftId = @ShiftId
                      AND Status = 'Open';";
                using (var cmd = conn.CreateCommand())
            {
             cmd.CommandText = query;
                cmd.Parameters.AddWithValue("@FinalRevenue", finalRevenue);
                cmd.Parameters.AddWithValue("@ShiftId", shiftId);
                int rowsAffected = cmd.ExecuteNonQuery();  
                return rowsAffected > 0;
            }  
        }  
    }
}