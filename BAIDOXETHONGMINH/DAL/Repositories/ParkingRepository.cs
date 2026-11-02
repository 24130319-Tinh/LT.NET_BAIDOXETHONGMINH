using BAIDOXETHONGMINH.DTO.Entities;
using Microsoft.EntityFrameworkCore;
namespace BAIDOXETHONGMINH.GUI;

public class ParkingRepository
{
    public async Task<List<ParkingSlot>> GetSlotsAsync()
    {
        // TODO: Database 
        return null;
    }


    public async Task<ParkingSlot?> GetSlotAsync(string slotCode)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateSlotAsync(ParkingSlot slot)
    {
        throw new NotImplementedException();
    }

    public async Task<List<ParkingZone>> GetZonesAsync()
    {
        throw new NotImplementedException();
    }
}