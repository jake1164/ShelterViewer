using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterViewer.Shared.Services.VaultServices;

/// <summary>
/// Room types are stored in a .json file on the file system.  Depending on the type of application
/// being used will determain how its loaded.  This interface is implemented in the MAUI or Blazor projects.
/// </summary>
public interface IRoomTypeLoader
{
    Task<Models.Data.RoomTypeRoot?> LoadRoomTypesAsync();
}
