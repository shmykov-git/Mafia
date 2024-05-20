using static Microsoft.Maui.ApplicationModel.Permissions;

namespace Host.Permission;

public class PermissionFather 
{
    public async Task CheckAndRequestPermissions()
    {
        await CheckAndRequestPermission<StorageRead>();
        await CheckAndRequestPermission<StorageWrite>();
    }

    private async Task<PermissionStatus> CheckAndRequestPermission<TPermission>() where TPermission : BasePermission, new()
    {
        PermissionStatus status = await CheckStatusAsync<TPermission>();

        if (status == PermissionStatus.Granted)
            return status;

        //if (Permissions.ShouldShowRationale<Permissions.StorageRead>())
        //{
        //    // Prompt the user with additional information as to why the permission is needed
        //}

        status = await RequestAsync<StorageRead>();

        return status;
    }
}
