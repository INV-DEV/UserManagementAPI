namespace UserManagementAPI.Model
{
    public interface IPaginationInfo
    {
        int PageNumber { get; }
        int PageSize { get; }
    }
}
